using UnityEngine;

namespace FallGuyClone
{
    /// <summary>Destroys an object after a lifetime or when it falls below a height.</summary>
    public class Despawner : MonoBehaviour
    {
        float lifetime;
        float minY;

        public void Init(float life, float killY)
        {
            lifetime = life;
            minY = killY;
        }

        void Update()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f || transform.position.y < minY) Destroy(gameObject);
        }
    }
}
