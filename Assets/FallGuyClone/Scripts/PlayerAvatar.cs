using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace FallGuyClone
{
    /// <summary>
    /// Builds the player's look (a Kenney character, or a "bean" made of primitives as fallback)
    /// and drives its animations with the Playables API, so no Animator Controller asset is needed.
    /// </summary>
    public class PlayerAvatar : MonoBehaviour
    {
        public static readonly string[] Skins =
            { "character-oobi", "character-oodi", "character-ooli", "character-oopi", "character-oozi" };

        public enum Pose { Idle, Walk, Run, Jump, Fall, Win, Lose }

        static readonly string[] ClipNames = { "idle", "walk", "sprint", "jump", "fall", "emote-yes", "die" };
        static readonly bool[] Loops = { true, true, true, false, true, true, false };

        /// <summary>Extra yaw applied to the model if it does not face +Z.</summary>
        public float modelYawOffset;

        public Pose? ForcedPose { get; set; }

        PlayerController controller;
        Transform visualRoot;
        PlayableGraph graph;
        AnimationMixerPlayable mixer;
        AnimationClipPlayable[] playables;
        Animation legacyAnimation;
        float[] weights;
        int current = -1;
        float squash;
        float tilt;

        void Awake()
        {
            if (!Application.isPlaying) return;
            controller = GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.Jumped += () => SetPose(Pose.Jump, true);
                controller.Landed += () => squash = 0.18f;
            }
        }

        public void Build(int skinIndex)
        {
            Cleanup();
            visualRoot = new GameObject("Visual").transform;
            visualRoot.SetParent(transform, false);
            visualRoot.gameObject.layer = gameObject.layer;

            string skin = Skins[Mathf.Abs(skinIndex) % Skins.Length];
            var holder = Art.FitModel(skin, visualRoot, new Vector3(0f, 1.55f, 0f), uniform: true, bottomAnchor: true,
                localRot: Quaternion.Euler(0f, modelYawOffset, 0f));

            if (holder == null)
            {
                BuildBean(skinIndex);
                return;
            }
            if (Application.isPlaying) SetupAnimation(holder, skin);
        }

        void BuildBean(int skinIndex)
        {
            Color[] colors = { Art.Pink, Art.Cyan, Art.Yellow, Art.Mint, Art.Purple };
            var body = colors[Mathf.Abs(skinIndex) % colors.Length];
            Art.Prim(PrimitiveType.Capsule, visualRoot, new Vector3(0f, 0.8f, 0f), new Vector3(0.85f, 0.8f, 0.85f), body, "Body");
            Art.Prim(PrimitiveType.Sphere, visualRoot, new Vector3(0f, 1.15f, 0.32f), new Vector3(0.6f, 0.38f, 0.25f), Art.White, "Visor");
            Art.Prim(PrimitiveType.Sphere, visualRoot, new Vector3(-0.12f, 1.17f, 0.43f), Vector3.one * 0.1f, Art.Dark, "EyeL");
            Art.Prim(PrimitiveType.Sphere, visualRoot, new Vector3(0.12f, 1.17f, 0.43f), Vector3.one * 0.1f, Art.Dark, "EyeR");
            Art.Prim(PrimitiveType.Capsule, visualRoot, new Vector3(-0.2f, 0.12f, 0f), new Vector3(0.25f, 0.15f, 0.3f), body, "FootL");
            Art.Prim(PrimitiveType.Capsule, visualRoot, new Vector3(0.2f, 0.12f, 0f), new Vector3(0.25f, 0.15f, 0.3f), body, "FootR");
        }

        void SetupAnimation(Transform holder, string skin)
        {
            var clips = Resources.LoadAll<AnimationClip>(Art.KenneyFolder + skin);
            var found = new AnimationClip[ClipNames.Length];
            foreach (var clip in clips)
            {
                if (clip == null || clip.name.StartsWith("__")) continue;
                string n = clip.name.ToLowerInvariant();
                for (int i = 0; i < ClipNames.Length; i++)
                    if (found[i] == null && (n == ClipNames[i] || n.EndsWith("|" + ClipNames[i])))
                        found[i] = clip;
            }
            if (found[0] == null) return; // no idle clip: leave the model static

            legacyAnimation = holder.GetComponentInChildren<Animation>();
            if (legacyAnimation != null)
            {
                for (int i = 0; i < found.Length; i++)
                {
                    if (found[i] == null) continue;
                    if (legacyAnimation.GetClip(found[i].name) == null) legacyAnimation.AddClip(found[i], found[i].name);
                    legacyAnimation[found[i].name].wrapMode = Loops[i] ? WrapMode.Loop : WrapMode.ClampForever;
                }
                ClipNamesFound = found;
                return;
            }

            var animator = holder.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                var modelRoot = holder.GetChild(0).GetChild(0).gameObject;
                animator = modelRoot.AddComponent<Animator>();
            }
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            graph = PlayableGraph.Create("PlayerAvatar");
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var output = AnimationPlayableOutput.Create(graph, "Animation", animator);
            mixer = AnimationMixerPlayable.Create(graph, ClipNames.Length);
            output.SetSourcePlayable(mixer);

            playables = new AnimationClipPlayable[ClipNames.Length];
            weights = new float[ClipNames.Length];
            for (int i = 0; i < ClipNames.Length; i++)
            {
                var clip = found[i] != null ? found[i] : found[0];
                playables[i] = AnimationClipPlayable.Create(graph, clip);
                graph.Connect(playables[i], 0, mixer, i);
                mixer.SetInputWeight(i, 0f);
            }
            ClipNamesFound = found;
            current = -1;
            SetPose(Pose.Idle, true);
            weights[(int)Pose.Idle] = 1f;
            mixer.SetInputWeight((int)Pose.Idle, 1f);
            graph.Play();
        }

        AnimationClip[] ClipNamesFound;

        public void SetPose(Pose pose, bool restart = false)
        {
            int i = (int)pose;
            if (i == current && !restart) return;
            current = i;

            if (legacyAnimation != null && ClipNamesFound != null)
            {
                var clip = ClipNamesFound[i] != null ? ClipNamesFound[i] : ClipNamesFound[0];
                if (restart) legacyAnimation.Stop(clip.name);
                legacyAnimation.CrossFade(clip.name, 0.15f);
                return;
            }
            if (playables != null && (restart || !Loops[i])) playables[i].SetTime(0);
        }

        void Update()
        {
            if (controller == null || visualRoot == null) return;

            // Choose the pose from the controller state.
            if (ForcedPose.HasValue) SetPose(ForcedPose.Value);
            else if (!controller.IsGrounded)
            {
                if (controller.VerticalSpeed < -1f || controller.IsDiving || controller.IsStunned) SetPose(Pose.Fall);
                else if (current != (int)Pose.Jump) SetPose(Pose.Fall);
            }
            else if (controller.PlanarSpeed > 4.5f) SetPose(Pose.Run);
            else if (controller.PlanarSpeed > 0.4f) SetPose(Pose.Walk);
            else SetPose(Pose.Idle);

            if (playables != null)
            {
                float dt = Time.deltaTime;
                for (int i = 0; i < playables.Length; i++)
                {
                    weights[i] = Mathf.MoveTowards(weights[i], i == current ? 1f : 0f, dt / 0.15f);
                    mixer.SetInputWeight(i, weights[i]);
                    var clip = playables[i].GetAnimationClip();
                    if (Loops[i] && clip != null && clip.length > 0f)
                    {
                        double t = playables[i].GetTime();
                        if (t > clip.length) playables[i].SetTime(t % clip.length);
                    }
                }
            }

            // Procedural extras: lean forward when diving, wobble when stunned, squash on landing.
            float targetTilt = controller.IsDiving ? 70f : 0f;
            tilt = Mathf.Lerp(tilt, targetTilt, 1f - Mathf.Exp(-12f * Time.deltaTime));
            float wobble = controller.IsStunned ? Mathf.Sin(Time.time * 30f) * 12f : 0f;
            visualRoot.localRotation = Quaternion.Euler(tilt, 0f, wobble);

            squash = Mathf.MoveTowards(squash, 0f, Time.deltaTime);
            float s = squash;
            visualRoot.localScale = new Vector3(1f + s, 1f - s, 1f + s);
            if (playables == null && legacyAnimation == null && controller.IsGrounded)
            {
                // Bean fallback: bob while walking.
                float bob = controller.PlanarSpeed > 0.4f ? Mathf.Abs(Mathf.Sin(Time.time * 14f)) * 0.08f : 0f;
                visualRoot.localPosition = Vector3.up * bob;
            }
        }

        void Cleanup()
        {
            if (graph.IsValid()) graph.Destroy();
            playables = null;
            legacyAnimation = null;
            ClipNamesFound = null;
            current = -1;
            if (visualRoot != null) Art.SafeDestroy(visualRoot.gameObject);
            visualRoot = null;
            // Remove a preview model baked into the scene by the editor.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.name != "Visual") continue;
                child.gameObject.SetActive(false);
                Art.SafeDestroy(child.gameObject);
            }
        }

        void OnDestroy()
        {
            if (graph.IsValid()) graph.Destroy();
        }
    }
}
