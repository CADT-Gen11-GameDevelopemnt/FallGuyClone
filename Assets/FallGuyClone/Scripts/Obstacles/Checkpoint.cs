using UnityEngine;

namespace FallGuyClone
{
    /// <summary>Saves the respawn point when the player walks through it.</summary>
    public class Checkpoint : MonoBehaviour
    {
        public int index;
        public Vector3 respawnPoint;

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            if (GameManager.Instance != null) GameManager.Instance.ReachCheckpoint(this);
        }
    }
}
