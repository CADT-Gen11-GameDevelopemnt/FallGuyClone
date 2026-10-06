using UnityEngine;

namespace FallGuyClone
{
    /// <summary>Ends the round as "Qualified" when the player crosses it.</summary>
    public class FinishLine : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null) return;
            if (GameManager.Instance != null) GameManager.Instance.Finish();
        }
    }
}
