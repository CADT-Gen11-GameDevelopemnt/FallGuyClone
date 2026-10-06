using UnityEngine;

namespace FallGuyClone
{
    /// <summary>Spawns barrels that roll down the ramp toward the player.</summary>
    public class BarrelSpawner : MonoBehaviour
    {
        public float interval = 1.3f;
        public float width = 10f;
        public float rollSpeed = 5f;
        public float lifetime = 16f;

        float timer = 0.5f;

        void FixedUpdate()
        {
            timer -= Time.fixedDeltaTime;
            if (timer > 0f) return;
            timer = interval * Random.Range(0.75f, 1.25f);
            Spawn();
        }

        void Spawn()
        {
            var go = new GameObject("Barrel");
            go.layer = Layers.Dynamic;
            go.transform.position = transform.position + transform.right * Random.Range(-width * 0.5f, width * 0.5f);
            go.transform.rotation = transform.rotation;

            var col = go.AddComponent<CapsuleCollider>();
            col.direction = 0; // along X so the barrel rolls forward/back
            col.radius = 0.6f;
            col.height = 1.7f;

            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 3f;
            rb.linearDamping = 0.15f;
            rb.angularDamping = 0.05f;
            rb.maxAngularVelocity = 40f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = -transform.forward * rollSpeed;
            rb.angularVelocity = -transform.right * (rollSpeed / col.radius);

            var hazard = go.AddComponent<Hazard>();
            hazard.force = 11f;
            hazard.upForce = 5f;

            if (Art.FitModel("barrel", go.transform, new Vector3(1.2f, 1.7f, 1.2f), localRot: Quaternion.Euler(0f, 0f, 90f)) == null)
            {
                Art.Prim(PrimitiveType.Cylinder, go.transform, Vector3.zero, new Vector3(1.2f, 0.85f, 1.2f), Art.Orange,
                    "BarrelVisual", localRot: Quaternion.Euler(0f, 0f, 90f));
            }

            go.AddComponent<Despawner>().Init(lifetime, -8f);
        }
    }
}
