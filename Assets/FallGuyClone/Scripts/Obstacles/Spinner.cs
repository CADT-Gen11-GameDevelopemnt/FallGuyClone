using UnityEngine;

namespace FallGuyClone
{
    /// <summary>Simple visual spin/bob for decorations (no physics).</summary>
    public class Spinner : MonoBehaviour
    {
        public Vector3 degreesPerSecond = new Vector3(0f, 90f, 0f);
        public float bobHeight;
        public float bobSpeed = 2f;

        Vector3 startLocal;
        float seed;

        void Start()
        {
            startLocal = transform.localPosition;
            seed = Random.value * 10f;
        }

        void Update()
        {
            transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
            if (bobHeight > 0f)
                transform.localPosition = startLocal + Vector3.up * (Mathf.Sin((Time.time + seed) * bobSpeed) * bobHeight);
        }
    }
}
