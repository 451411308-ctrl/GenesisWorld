using System.Collections;
using GenesisWorld.Interaction;
using UnityEngine;

namespace GenesisWorld.NPC
{
    /// <summary>
    /// 场景中的 NPC 实体，只桥接 Profile 与交互入口，不负责输入、UI 或 AI Provider。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class NPCActor : MonoBehaviour, IInteractable
    {
        [SerializeField] private NPCProfile profile;
        [SerializeField] private Transform interactionPoint;

        [Header("Placement")]
        [SerializeField] private bool snapToGroundOnStart = true;
        [SerializeField, Min(0.1f)] private float groundProbeHeight = 5f;
        [SerializeField, Min(0.1f)] private float groundProbeDistance = 12f;

        private readonly RaycastHit[] groundHits = new RaycastHit[16];

        public NPCProfile Profile => profile;
        public string NpcId => profile != null ? profile.NpcId : "npc_unknown";
        public string DisplayName => profile != null ? profile.DisplayName : "Unknown NPC";
        public string Role => profile != null ? profile.Role : "Resident";
        public string Description => profile != null ? profile.Description : string.Empty;
        public string Greeting => profile != null ? profile.Greeting : "Hello, traveler.";
        public string InteractionPrompt => $"[E] Talk to {DisplayName}";
        public Transform InteractionTransform => interactionPoint != null ? interactionPoint : transform;
        public bool CanInteract => isActiveAndEnabled;

        private IEnumerator Start()
        {
            if (!snapToGroundOnStart)
            {
                yield break;
            }

            // 等待程序化地形在 Start 阶段完成 MeshCollider 更新。
            yield return null;
            SnapToGround();
        }

        public void Interact(PlayerInteractionController interactor)
        {
            if (CanInteract && interactor != null)
            {
                interactor.BeginDialogue(this);
            }
        }

        private void SnapToGround()
        {
            Vector3 origin = transform.position + Vector3.up * groundProbeHeight;
            int hitCount = Physics.RaycastNonAlloc(
                origin,
                Vector3.down,
                groundHits,
                groundProbeHeight + groundProbeDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            float nearestDistance = float.PositiveInfinity;
            Vector3 groundPoint = transform.position;
            bool foundGround = false;

            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = groundHits[index];
                if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    groundPoint = hit.point;
                    foundGround = true;
                }
            }

            if (foundGround)
            {
                transform.position = groundPoint;
            }
        }

        private void OnValidate()
        {
            groundProbeHeight = Mathf.Max(0.1f, groundProbeHeight);
            groundProbeDistance = Mathf.Max(0.1f, groundProbeDistance);
        }
    }
}
