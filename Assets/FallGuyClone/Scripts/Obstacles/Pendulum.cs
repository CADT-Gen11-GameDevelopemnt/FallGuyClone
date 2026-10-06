using UnityEngine;

namespace FallGuyClone
{
    /// <summary>Swings a kinematic rigidbody back and forth around a local axis (wrecking balls).</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Pendulum : MonoBehaviour, IKinematicMover
    {
        public Vector3 axis = Vector3.forward;
        public float amplitude = 60f;
        public float period = 2.5f;
        [Range(0f, 1f)] public float phase;

        Rigidbody rb;
        Quaternion baseRotation;
        float clock;
        float angularSpeedDeg;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            baseRotation = transform.rotation;
            transform.rotation = Evaluate(0f);
        }

        Quaternion Evaluate(float t)
        {
            float w = 2f * Mathf.PI / period;
            float arg = w * t + phase * 2f * Mathf.PI;
            angularSpeedDeg = amplitude * w * Mathf.Cos(arg);
            return baseRotation * Quaternion.AngleAxis(amplitude * Mathf.Sin(arg), axis);
        }

        void FixedUpdate()
        {
            clock += Time.fixedDeltaTime;
            rb.MoveRotation(Evaluate(clock));
        }

        public Vector3 GetPointVelocity(Vector3 worldPoint)
        {
            Vector3 w = (baseRotation * axis.normalized) * (angularSpeedDeg * Mathf.Deg2Rad);
            return Vector3.Cross(w, worldPoint - rb.position);
        }
    }
}
