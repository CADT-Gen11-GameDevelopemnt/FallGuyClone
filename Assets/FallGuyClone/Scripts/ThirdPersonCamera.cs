using UnityEngine;
using UnityEngine.InputSystem;

namespace FallGuyClone
{
    /// <summary>Mouse-orbit third person camera with scroll zoom and wall collision.</summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 pivotOffset = new Vector3(0f, 1.5f, 0f);
        public float distance = 7f;
        public float minDistance = 3f;
        public float maxDistance = 12f;
        public float mouseSensitivity = 0.12f;
        public float minPitch = -25f;
        public float maxPitch = 70f;
        public float yaw;
        public float pitch = 18f;

        public bool InputEnabled { get; set; }

        Vector3 smoothPivot;
        float currentDistance;
        int collisionMask;

        void Start()
        {
            // Only static course geometry blocks the camera.
            collisionMask = 1 << 0;
            currentDistance = distance;
            if (target != null) smoothPivot = target.position + pivotOffset;
        }

        public void SnapBehindTarget()
        {
            if (target == null) return;
            yaw = target.eulerAngles.y;
            smoothPivot = target.position + pivotOffset;
        }

        void LateUpdate()
        {
            if (target == null) return;
            float dt = Time.unscaledDeltaTime;

            var mouse = Mouse.current;
            if (InputEnabled && mouse != null)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * mouseSensitivity;
                pitch = Mathf.Clamp(pitch - delta.y * mouseSensitivity, minPitch, maxPitch);

                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                    distance = Mathf.Clamp(distance - Mathf.Sign(scroll) * 0.6f, minDistance, maxDistance);
            }

            Vector3 pivot = target.position + pivotOffset;
            smoothPivot = Vector3.Lerp(smoothPivot, pivot, 1f - Mathf.Exp(-14f * dt));

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 back = rot * Vector3.back;

            float wanted = distance;
            if (Physics.SphereCast(smoothPivot, 0.3f, back, out var hit, distance, collisionMask, QueryTriggerInteraction.Ignore))
                wanted = Mathf.Max(hit.distance - 0.1f, 0.8f);

            // Pull in instantly, ease back out.
            currentDistance = wanted < currentDistance ? wanted : Mathf.Lerp(currentDistance, wanted, 1f - Mathf.Exp(-5f * dt));

            transform.SetPositionAndRotation(smoothPivot + back * currentDistance, rot);
        }
    }
}
