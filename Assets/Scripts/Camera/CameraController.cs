using UnityEngine;
using UnityEngine.InputSystem;

namespace GenesisWorld.CameraSystem
{
    /// <summary>
    /// 面向第三人称探索玩法的基础轨道摄像机。
    /// 仅依赖目标 Transform，避免摄像机模块与玩家控制实现互相耦合。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class CameraController : MonoBehaviour
    {
        private const float ScrollWheelStep = 120f;

        [Header("跟随目标")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = Vector3.zero;

        [Header("旋转")]
        [SerializeField, Min(0f)] private float mouseSensitivity = 0.15f;
        [SerializeField] private float minPitch = -20f;
        [SerializeField] private float maxPitch = 65f;

        [Header("距离")]
        [SerializeField, Min(0f)] private float distance = 5f;
        [SerializeField, Min(0f)] private float minDistance = 2f;
        [SerializeField, Min(0f)] private float maxDistance = 8f;
        [SerializeField, Min(0f)] private float zoomSpeed = 1.5f;

        [Header("平滑跟随")]
        [SerializeField, Min(0.001f)] private float followSmoothTime = 0.08f;

        private Vector2 rotationInput;
        private Vector3 followVelocity;
        private float scrollInput;
        private float yaw;
        private float pitch = 20f;
        private bool isCursorLocked;

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Vector3 currentAngles = transform.eulerAngles;
            yaw = currentAngles.y;
            pitch = NormalizeAngle(currentAngles.x);
            LockCursor();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                UnlockCursor();
            }
        }

        private void Update()
        {
            HandleCursor();
            ReadInput();
            CalculateRotation();
            HandleZoom();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            // 玩家在 Update 中完成位移后再更新摄像机，可减少跟随产生的视觉抖动。
            Vector3 focusPoint = target.position + targetOffset;
            Vector3 desiredPosition = CalculateCameraPosition(focusPoint);
            ApplyCameraTransform(focusPoint, desiredPosition);
        }

        private void ReadInput()
        {
            rotationInput = Vector2.zero;
            scrollInput = 0f;

            if (!isCursorLocked)
            {
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            rotationInput = mouse.delta.ReadValue();
            scrollInput = mouse.scroll.ReadValue().y / ScrollWheelStep;
        }

        private void CalculateRotation()
        {
            yaw += rotationInput.x * mouseSensitivity;
            pitch -= rotationInput.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private Vector3 CalculateCameraPosition(Vector3 focusPoint)
        {
            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            return focusPoint - orbitRotation * Vector3.forward * distance;
        }

        private void ApplyCameraTransform(Vector3 focusPoint, Vector3 desiredPosition)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref followVelocity,
                followSmoothTime);

            Vector3 lookDirection = focusPoint - transform.position;
            if (lookDirection.sqrMagnitude > Mathf.Epsilon)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }

        private void HandleZoom()
        {
            distance -= scrollInput * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        private void HandleCursor()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                UnlockCursor();
                return;
            }

            Mouse mouse = Mouse.current;
            if (!isCursorLocked && mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                LockCursor();
            }
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isCursorLocked = true;
        }

        private void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isCursorLocked = false;
        }

        private void OnValidate()
        {
            minDistance = Mathf.Max(0f, minDistance);
            maxDistance = Mathf.Max(minDistance, maxDistance);
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            if (maxPitch < minPitch)
            {
                maxPitch = minPitch;
            }

            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
