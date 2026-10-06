using UnityEngine;

namespace FallGuyClone
{
    /// <summary>
    /// Moves a kinematic rigidbody along an offset. Sine mode is used for sliding platforms,
    /// Punch mode for the side pushers (fast punch, hold, slow retract).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Oscillator : MonoBehaviour, IKinematicMover
    {
        public enum Mode { Sine, Punch }

        public Mode mode = Mode.Sine;
        public Vector3 offset = new Vector3(4f, 0f, 0f);
        public float period = 3f;
        [Range(0f, 1f)] public float phase;

        Rigidbody rb;
        Vector3 start;
        Vector3 velocity;
        float clock;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            start = transform.position;
            transform.position = Evaluate(0f);
        }

        Vector3 Evaluate(float t)
        {
            float cycle = Mathf.Repeat(t / period + phase, 1f);
            float f;
            if (mode == Mode.Sine)
            {
                f = Mathf.Sin(cycle * 2f * Mathf.PI);
            }
            else
            {
                if (cycle < 0.35f) f = 0f;
                else if (cycle < 0.45f) f = Mathf.SmoothStep(0f, 1f, (cycle - 0.35f) / 0.1f);
                else if (cycle < 0.65f) f = 1f;
                else if (cycle < 0.95f) f = 1f - (cycle - 0.65f) / 0.3f;
                else f = 0f;
            }
            return start + offset * f;
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            clock += dt;
            Vector3 next = Evaluate(clock);
            velocity = (next - rb.position) / dt;
            rb.MovePosition(next);
        }

        public Vector3 GetPointVelocity(Vector3 worldPoint) => velocity;
    }
}
