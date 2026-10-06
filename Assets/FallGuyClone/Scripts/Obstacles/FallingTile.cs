using UnityEngine;

namespace FallGuyClone
{
    /// <summary>Tile that shakes when the player lands on it, drops away, then comes back.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class FallingTile : MonoBehaviour, IKinematicMover
    {
        enum State { Idle, Shaking, Falling, Hidden }

        public float shakeTime = 0.55f;
        public float respawnDelay = 3.5f;
        public Color warnColor = new Color(1f, 0.25f, 0.25f);

        Rigidbody rb;
        Collider col;
        Renderer[] renderers;
        Transform visual;
        Material[] baseMaterials;
        Vector3 home;
        Vector3 velocity;
        State state;
        float timer;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            col = GetComponent<Collider>();
            renderers = GetComponentsInChildren<Renderer>();
            baseMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) baseMaterials[i] = renderers[i].sharedMaterial;
            visual = transform.childCount > 0 ? transform.GetChild(0) : null;
            home = transform.position;
        }

        void OnCollisionEnter(Collision c) => TryTrigger(c);
        void OnCollisionStay(Collision c) => TryTrigger(c);

        void TryTrigger(Collision c)
        {
            if (state != State.Idle) return;
            if (c.rigidbody == null || c.rigidbody.GetComponent<PlayerController>() == null) return;
            state = State.Shaking;
            timer = 0f;
            var warn = Art.Colored(warnColor);
            foreach (var r in renderers) r.sharedMaterial = warn;
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            switch (state)
            {
                case State.Shaking:
                    timer += dt;
                    if (visual != null) visual.localPosition = Random.insideUnitSphere * 0.06f;
                    if (timer >= shakeTime)
                    {
                        if (visual != null) visual.localPosition = Vector3.zero;
                        state = State.Falling;
                        velocity = Vector3.zero;
                    }
                    break;

                case State.Falling:
                    velocity += Vector3.down * 18f * dt;
                    rb.MovePosition(rb.position + velocity * dt);
                    if (home.y - rb.position.y > 14f)
                    {
                        state = State.Hidden;
                        timer = 0f;
                        col.enabled = false;
                        foreach (var r in renderers) r.enabled = false;
                    }
                    break;

                case State.Hidden:
                    timer += dt;
                    if (timer >= respawnDelay) ResetTile();
                    break;
            }
        }

        void ResetTile()
        {
            velocity = Vector3.zero;
            rb.position = home;
            transform.position = home;
            col.enabled = true;
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = true;
                renderers[i].sharedMaterial = baseMaterials[i];
            }
            state = State.Idle;
        }

        public Vector3 GetPointVelocity(Vector3 worldPoint) => velocity;
    }
}
