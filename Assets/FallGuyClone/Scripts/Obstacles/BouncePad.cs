using UnityEngine;

namespace FallGuyClone
{
    /// <summary>Launches the player upward (spring pad).</summary>
    public class BouncePad : MonoBehaviour
    {
        public float launchSpeed = 15f;

        void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null) player.Launch(launchSpeed);
        }
    }
}
