using UnityEngine;
using UnityEngine.Rendering;

namespace FallGuyClone
{
    /// <summary>
    /// Entry point. Uses the Course, Player, Main Camera and Sun already in the scene
    /// (the editor menu "Fall Guy Clone > Create Game Scene" bakes them in).
    /// Anything missing is created at runtime, so this also works in an empty scene.
    /// </summary>
    public class FallGuyGame : MonoBehaviour
    {
        [Tooltip("Seconds the player has to reach the finish line.")]
        public float timeLimit = 150f;

        [Tooltip("Rotate the Kenney character if it does not face forward.")]
        public float characterYawOffset;

        void Awake()
        {
            Application.targetFrameRate = 120;
            Physics.gravity = new Vector3(0f, -22f, 0f);
            SetupEnvironment();

            var course = FindAnyObjectByType<Course>();
            if (course == null) course = CourseBuilder.Build();

            var player = FindAnyObjectByType<PlayerController>();
            if (player == null) player = CreatePlayer(course.spawnPoint);
            player.transform.SetPositionAndRotation(course.spawnPoint, Quaternion.identity);
            var avatar = player.GetComponent<PlayerAvatar>();
            if (avatar == null) avatar = player.gameObject.AddComponent<PlayerAvatar>();
            avatar.modelYawOffset = characterYawOffset;

            var orbit = SetupCamera(player.transform);
            player.cameraTransform = orbit.transform;

            var ui = GameUI.Create();
            var gm = gameObject.AddComponent<GameManager>();
            ui.Bind(gm);
            gm.Init(player, avatar, orbit, ui, course, timeLimit);
        }

        public static PlayerController CreatePlayer(Vector3 position)
        {
            var playerGo = new GameObject("Player");
            playerGo.transform.position = position;
            playerGo.AddComponent<Rigidbody>();
            playerGo.AddComponent<CapsuleCollider>();
            var player = playerGo.AddComponent<PlayerController>();
            playerGo.AddComponent<PlayerAvatar>();
            return player;
        }

        public static ThirdPersonCamera SetupCamera(Transform target)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 600f;
            if (RenderSettings.skybox != null)
            {
                cam.clearFlags = CameraClearFlags.Skybox;
            }
            else
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.55f, 0.8f, 1f);
            }
            var orbit = cam.GetComponent<ThirdPersonCamera>();
            if (orbit == null) orbit = cam.gameObject.AddComponent<ThirdPersonCamera>();
            orbit.target = target;
            orbit.SnapBehindTarget();

            // Start directly behind the player (also used for the editor preview).
            var rot = Quaternion.Euler(orbit.pitch, orbit.yaw, 0f);
            cam.transform.SetPositionAndRotation(target.position + orbit.pivotOffset + rot * Vector3.back * orbit.distance, rot);
            return orbit;
        }

        public static void SetupEnvironment()
        {
            Light sun = null;
            foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { sun = l; break; }
            if (sun == null)
            {
                sun = new GameObject("Sun").AddComponent<Light>();
                sun.type = LightType.Directional;
            }
            sun.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            sun.color = new Color(1f, 0.96f, 0.88f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.6f;
            RenderSettings.sun = sun;

            // Bright, cartoony ambient light (set directly so no lighting bake is needed).
            var sky = new Color(0.62f, 0.7f, 0.9f);
            var ground = new Color(0.55f, 0.45f, 0.6f);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = sky;
            RenderSettings.ambientEquatorColor = new Color(0.6f, 0.6f, 0.7f);
            RenderSettings.ambientGroundColor = ground;
            var sh = new SphericalHarmonicsL2();
            sh.AddAmbientLight(new Color(0.5f, 0.52f, 0.62f));
            sh.AddDirectionalLight(Vector3.up, sky * 0.35f, 1f);
            RenderSettings.ambientProbe = sh;

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.75f, 0.85f, 1f);
            RenderSettings.fogStartDistance = 90f;
            RenderSettings.fogEndDistance = 320f;
        }
    }
}
