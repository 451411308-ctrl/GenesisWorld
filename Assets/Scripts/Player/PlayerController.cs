using UnityEngine;
using UnityEngine.InputSystem;

namespace GenesisWorld.Player
{
    /// <summary>
    /// 基于 CharacterController 的基础玩家控制器。
    /// 仅负责移动、冲刺、跳跃和重力，不包含摄像机或其他玩法逻辑。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("移动参数")]
        [SerializeField, Min(0f)] private float walkSpeed = 4f;
        [SerializeField, Min(0f)] private float runSpeed = 7f;
        [SerializeField] private Transform movementReference;

        [Header("跳跃与重力")]
        [SerializeField, Min(0f)] private float jumpForce = 7f;
        [SerializeField] private float gravity = -20f;
        [SerializeField, Min(0f)] private float groundedForce = 2f;

        [Header("动画接口（预留）")]
        [SerializeField] private Animator animator;
        [SerializeField] private string moveSpeedParameter = "MoveSpeed";

        private CharacterController characterController;
        private float verticalVelocity;
        private int moveSpeedParameterHash;

        /// <summary>当前是否接触地面。</summary>
        public bool IsGrounded => characterController != null && characterController.isGrounded;

        /// <summary>当前竖直速度，供后续动画或状态系统读取。</summary>
        public float VerticalVelocity => verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            moveSpeedParameterHash = Animator.StringToHash(moveSpeedParameter);

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                ApplyGravityAndMove(Vector3.zero);
                UpdateAnimator(0f);
                return;
            }

            Vector2 moveInput = ReadMoveInput(keyboard);
            bool isSprinting = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            float currentSpeed = isSprinting ? runSpeed : walkSpeed;

            Vector3 horizontalMovement = CalculateHorizontalMovement(moveInput, currentSpeed);

            if (IsGrounded && verticalVelocity < 0f)
            {
                // 保持一个很小的向下速度，使 CharacterController 稳定贴地。
                verticalVelocity = -groundedForce;
            }

            if (IsGrounded && keyboard.spaceKey.wasPressedThisFrame)
            {
                verticalVelocity = jumpForce;
            }

            ApplyGravityAndMove(horizontalMovement);
            UpdateAnimator(moveInput.magnitude * currentSpeed);
        }

        private static Vector2 ReadMoveInput(Keyboard keyboard)
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (keyboard.aKey.isPressed) horizontal -= 1f;
            if (keyboard.dKey.isPressed) horizontal += 1f;
            if (keyboard.sKey.isPressed) vertical -= 1f;
            if (keyboard.wKey.isPressed) vertical += 1f;

            // 防止斜向移动速度高于单轴移动速度。
            return Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
        }

        private Vector3 CalculateHorizontalMovement(Vector2 moveInput, float currentSpeed)
        {
            if (movementReference == null)
            {
                return new Vector3(moveInput.x, 0f, moveInput.y) * currentSpeed;
            }

            // 只取相机在水平面的朝向，避免俯仰角影响角色的地面移动速度。
            Vector3 forward = Vector3.ProjectOnPlane(movementReference.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(movementReference.right, Vector3.up).normalized;
            return (right * moveInput.x + forward * moveInput.y) * currentSpeed;
        }

        private void ApplyGravityAndMove(Vector3 horizontalMovement)
        {
            verticalVelocity += gravity * Time.deltaTime;
            Vector3 movement = horizontalMovement + Vector3.up * verticalVelocity;
            characterController.Move(movement * Time.deltaTime);
        }

        private void UpdateAnimator(float movementSpeed)
        {
            // Animator Controller 将在后续动画阶段配置；没有 Controller 时不写入参数。
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                return;
            }

            float normalizedSpeed = runSpeed > 0f ? movementSpeed / runSpeed : 0f;
            animator.SetFloat(moveSpeedParameterHash, normalizedSpeed);
        }

        private void OnValidate()
        {
            if (runSpeed < walkSpeed)
            {
                runSpeed = walkSpeed;
            }

            if (gravity > 0f)
            {
                gravity = -gravity;
            }

            moveSpeedParameterHash = Animator.StringToHash(moveSpeedParameter);
        }
    }
}
