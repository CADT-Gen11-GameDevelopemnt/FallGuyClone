using UnityEngine;

namespace FallGuyClone
{
    /// <summary>Spins a kinematic rigidbody at a constant rate (sweeper bars, spinning discs).</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Rotator : MonoBehaviour, IKinematicMover
    {
        public Vector3 axis = Vector3.up;
        public float degreesPerSecond = 90f;

        Rigidbody rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        void FixedUpdate()
        {
            rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(degreesPerSecond * Time.fixedDeltaTime, axis));
        }

        public Vector3 GetPointVelocity(Vector3 worldPoint)
        {
            Vector3 w = (rb.rotation * axis.normalized) * (degreesPerSecond * Mathf.Deg2Rad);
            return Vector3.Cross(w, worldPoint - rb.position);
        }
    }
}
