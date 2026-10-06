using UnityEngine;

namespace FallGuyClone
{
    /// <summary>Marks an obstacle that knocks the player back on contact.</summary>
    public class Hazard : MonoBehaviour
    {
        public float force = 12f;
        public float upForce = 5f;
        public float stunTime = 0.6f;

        /// <summary>Computes the knock-back velocity for a player touching this hazard at <paramref name="point"/>.</summary>
        public Vector3 ComputeKnock(Vector3 point, Vector3 playerPosition, Rigidbody otherBody)
        {
            Vector3 source = Vector3.zero;
            var mover = GetComponentInParent<IKinematicMover>();
            if (mover != null) source = mover.GetPointVelocity(point);
            else if (otherBody != null && !otherBody.isKinematic) source = otherBody.GetPointVelocity(point);

            Vector3 dir = new Vector3(source.x, 0f, source.z);
            Vector3 away = playerPosition - point;
            away.y = 0f;
            if (dir.sqrMagnitude < 0.25f) dir = away;
            if (dir.sqrMagnitude < 0.0001f) dir = -Vector3.forward;
            dir.Normalize();

            // Blend in a little "away from the obstacle" so the player never gets pushed into it.
            if (away.sqrMagnitude > 0.0001f) dir = (dir + away.normalized * 0.35f).normalized;
            return dir * force + Vector3.up * upForce;
        }
    }
}
