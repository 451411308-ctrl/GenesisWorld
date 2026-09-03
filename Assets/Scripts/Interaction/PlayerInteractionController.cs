using GenesisWorld.NPC;
using GenesisWorld.Player;
using GenesisWorld.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GenesisWorld.Interaction
{
    /// <summary>
    /// 从摄像机中心射线选择一个目标，并把交互请求交给目标与 DialogueController。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerInteractionController : MonoBehaviour
    {
        private const int HitBufferSize = 16;

        [Header("References")]
        [SerializeField] private Camera interactionCamera;
        [SerializeField] private DialogueController dialogueController;
        [SerializeField] private PlayerController playerController;

        [Header("Detection")]
        [SerializeField, Min(0.1f)] private float interactionDistance = 4f;
        [SerializeField, Min(0.1f)] private float raycastDistance = 12f;
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField] private bool drawDebugRay;

        private readonly RaycastHit[] hitBuffer = new RaycastHit[HitBufferSize];
        private IInteractable currentTarget;

        public IInteractable CurrentTarget => currentTarget;
        public bool HasTarget => currentTarget != null;
        public float InteractionDistance => interactionDistance;
        public DialogueController DialogueController => dialogueController;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;

            if (dialogueController != null && dialogueController.IsOpen)
            {
                SetTarget(null);

                if (keyboard != null &&
                    (keyboard.eKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
                {
                    dialogueController.CloseDialogue();
                }

                return;
            }

            EvaluateTarget();

            if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
            {
                InteractWithCurrentTarget();
            }
        }

        public void InteractWithCurrentTarget()
        {
            if (currentTarget != null && currentTarget.CanInteract)
            {
                currentTarget.Interact(this);
            }
        }

        public void BeginDialogue(NPCActor npc)
        {
            dialogueController?.OpenDialogue(npc);
        }

        private void EvaluateTarget()
        {
            if (interactionCamera == null || dialogueController == null)
            {
                SetTarget(null);
                return;
            }

            Ray ray = interactionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            int hitCount = Physics.RaycastNonAlloc(
                ray,
                hitBuffer,
                raycastDistance,
                interactionLayers,
                QueryTriggerInteraction.Collide);

            if (drawDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.cyan);
            }

            RaycastHit nearestVisibleHit = default;
            float nearestDistance = float.PositiveInfinity;

            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = hitBuffer[index];
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    nearestVisibleHit = hit;
                }
            }

            if (nearestVisibleHit.collider == null)
            {
                SetTarget(null);
                return;
            }

            IInteractable interactable = FindInteractable(nearestVisibleHit.collider);
            if (interactable == null || !interactable.CanInteract)
            {
                SetTarget(null);
                return;
            }

            // 以射线命中的碰撞体表面计算距离，避免 NPC 身高或 UI 锚点抬高有效距离。
            float playerDistance = Vector3.Distance(
                transform.position,
                nearestVisibleHit.collider.ClosestPoint(transform.position));
            SetTarget(playerDistance <= interactionDistance ? interactable : null);
        }

        private static IInteractable FindInteractable(Collider hitCollider)
        {
            MonoBehaviour[] behaviours = hitCollider.GetComponentsInParent<MonoBehaviour>(true);
            for (int index = 0; index < behaviours.Length; index++)
            {
                if (behaviours[index] is IInteractable interactable)
                {
                    return interactable;
                }
            }

            return null;
        }

        private void SetTarget(IInteractable target)
        {
            if (ReferenceEquals(currentTarget, target))
            {
                return;
            }

            currentTarget = target;
            if (currentTarget == null)
            {
                dialogueController?.HideInteractionPrompt();
            }
            else
            {
                dialogueController?.ShowInteractionPrompt(currentTarget.InteractionPrompt);
            }
        }

        private void ResolveReferences()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }

            if (interactionCamera == null)
            {
                interactionCamera = Camera.main;
            }

            if (dialogueController == null)
            {
                dialogueController = FindObjectOfType<DialogueController>();
            }
        }

        private void OnDisable()
        {
            SetTarget(null);
            dialogueController?.CloseDialogue();
        }

        private void OnValidate()
        {
            interactionDistance = Mathf.Max(0.1f, interactionDistance);
            raycastDistance = Mathf.Max(interactionDistance, raycastDistance);
        }
    }
}
