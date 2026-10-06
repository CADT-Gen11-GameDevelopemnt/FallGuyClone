using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FallGuyClone.EditorTools
{
    /// <summary>
    /// Creates the materials and the game scene the first time the project compiles
    /// (only when the scene is missing), and adds the scene to Build Settings.
    /// Use the "Fall Guy Clone" menu to rebuild the scene from code.
    /// </summary>
    [InitializeOnLoad]
    public static class FallGuySetup
    {
        public const string ScenePath = "Assets/FallGuyClone/Scenes/FallGuyCourse.unity";
        const string BaseMaterialPath = "Assets/FallGuyClone/Resources/FallGuyBase.mat";
        const string KenneyMaterialPath = "Assets/FallGuyClone/Resources/FallGuyKenney.mat";
        const string SkyMaterialPath = "Assets/FallGuyClone/Materials/FallGuySky.mat";
        const string ColormapPath = "Assets/FallGuyClone/Resources/Kenney/Textures/colormap.png";

        static FallGuySetup()
        {
            EditorApplication.delayCall += AutoSetup;
        }

        static void AutoSetup()
        {
            if (Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EnsureAssets();
            if (File.Exists(ScenePath)) return;
            if (SceneManager.GetActiveScene().isDirty)
            {
                Debug.Log("[Fall Guy Clone] Save your scene, then use menu 'Fall Guy Clone > Create Game Scene'.");
                return;
            }
            CreateScene();
        }

        [MenuItem("Fall Guy Clone/Create Game Scene")]
        public static void CreateSceneMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureAssets();
            CreateScene();
        }

        [MenuItem("Fall Guy Clone/Open Game Scene")]
        static void OpenScene()
        {
            if (!File.Exists(ScenePath))
            {
                CreateSceneMenu();
                return;
            }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
        }

        public static void EnsureAssets()
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) lit = Shader.Find("Standard");

            if (!File.Exists(BaseMaterialPath))
            {
                var m = new Material(lit) { name = "FallGuyBase" };
                m.SetFloat("_Smoothness", 0.35f);
                AssetDatabase.CreateAsset(m, BaseMaterialPath);
            }

            if (!File.Exists(KenneyMaterialPath))
            {
                var m = new Material(lit) { name = "FallGuyKenney" };
                m.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(ColormapPath);
                m.color = Color.white;
                m.SetFloat("_Smoothness", 0.2f);
                AssetDatabase.CreateAsset(m, KenneyMaterialPath);
            }

            if (!File.Exists(SkyMaterialPath))
            {
                var skyShader = Shader.Find("Skybox/Procedural");
                if (skyShader != null)
                {
                    var sky = new Material(skyShader) { name = "FallGuySky" };
                    sky.SetColor("_SkyTint", new Color(0.45f, 0.65f, 1f));
                    sky.SetColor("_GroundColor", new Color(0.85f, 0.55f, 0.9f));
                    sky.SetFloat("_AtmosphereThickness", 0.7f);
                    sky.SetFloat("_Exposure", 1.3f);
                    sky.SetFloat("_SunSize", 0.05f);
                    AssetDatabase.CreateAsset(sky, SkyMaterialPath);
                }
            }
            AssetDatabase.SaveAssets();
        }

        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sky = AssetDatabase.LoadAssetAtPath<Material>(SkyMaterialPath);
            if (sky != null) RenderSettings.skybox = sky;

            // Bake everything into the scene so it is visible and editable before pressing Play.
            new GameObject("FallGuyGame").AddComponent<FallGuyGame>();
            FallGuyGame.SetupEnvironment();
            var course = CourseBuilder.Build();
            var player = FallGuyGame.CreatePlayer(course.spawnPoint);
            player.GetComponent<PlayerAvatar>().Build(0);
            FallGuyGame.SetupCamera(player.transform);
            AssetDatabase.SaveAssets();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = EditorBuildSettings.scenes.Where(s => s.path != ScenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();

            Debug.Log("[Fall Guy Clone] Game scene created at " + ScenePath + ". Press Play!");
        }
    }
}
