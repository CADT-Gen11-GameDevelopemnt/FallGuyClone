using UnityEngine;

namespace FallGuyClone
{
    /// <summary>Anything that moves under script control and can tell how fast a point on it travels.</summary>
    public interface IKinematicMover
    {
        Vector3 GetPointVelocity(Vector3 worldPoint);
    }
}
