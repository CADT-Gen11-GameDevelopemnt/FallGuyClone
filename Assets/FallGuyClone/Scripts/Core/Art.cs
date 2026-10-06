using System.Collections.Generic;
using UnityEngine;

namespace FallGuyClone
{
    /// <summary>
    /// Loads the free Kenney "Platformer Kit" models from FallGuyClone/Resources/Kenney and builds
    /// coloured materials. Every helper falls back to Unity primitives when a model is missing,
    /// so the game still runs without the asset pack.
    /// </summary>
    public static class Art
    {
        public const string KenneyFolder = "Kenney/";

        public static readonly Color Pink = new Color(1f, 0.45f, 0.72f);
        public static readonly Color Yellow = new Color(1f, 0.84f, 0.25f);
        public static readonly Color Cyan = new Color(0.3f, 0.82f, 1f);
        public static readonly Color Purple = new Color(0.62f, 0.45f, 1f);
        public static readonly Color Orange = new Color(1f, 0.58f, 0.22f);
        public static readonly Color Mint = new Color(0.45f, 0.95f, 0.7f);
        public static readonly Color White = new Color(0.97f, 0.97f, 1f);
        public static readonly Color Dark = new Color(0.15f, 0.12f, 0.25f);

        static Material baseMaterial;
        static Material kenneyMaterial;
        static readonly Dictionary<Color, Material> colorCache = new Dictionary<Color, Material>();
        static readonly Dictionary<string, GameObject> modelCache = new Dictionary<string, GameObject>();

        static Shader LitShader
        {
            get
            {
                var s = Shader.Find("Universal Render Pipeline/Lit");
                return s != null ? s : Shader.Find("Standard");
            }
        }

        public static Material BaseMaterial
        {
            get
            {
                if (baseMaterial == null)
                {
                    baseMaterial = Resources.Load<Material>("FallGuyBase");
                    if (baseMaterial == null) baseMaterial = new Material(LitShader);
                }
                return baseMaterial;
            }
        }

        public static Material KenneyMaterial
        {
            get
            {
                if (kenneyMaterial == null)
                {
                    kenneyMaterial = Resources.Load<Material>("FallGuyKenney");
                    if (kenneyMaterial == null)
                    {
                        kenneyMaterial = new Material(BaseMaterial) { name = "KenneyRuntime" };
                        kenneyMaterial.color = Color.white;
                        kenneyMaterial.mainTexture = Resources.Load<Texture2D>(KenneyFolder + "Textures/colormap");
                    }
                }
                return kenneyMaterial;
            }
        }

        public static Material Colored(Color c)
        {
            if (colorCache.TryGetValue(c, out var m) && m != null) return m;
            string name = "Color_" + ColorUtility.ToHtmlStringRGB(c);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Objects baked into the scene need real material assets, otherwise the
                // references are lost when the scene is saved.
                const string folder = "Assets/FallGuyClone/Materials";
                if (!UnityEditor.AssetDatabase.IsValidFolder(folder))
                    UnityEditor.AssetDatabase.CreateFolder("Assets/FallGuyClone", "Materials");
                string path = folder + "/" + name + ".mat";
                m = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null)
                {
                    m = new Material(BaseMaterial) { name = name };
                    m.color = c;
                    UnityEditor.AssetDatabase.CreateAsset(m, path);
                }
                colorCache[c] = m;
                return m;
            }
#endif
            m = new Material(BaseMaterial) { name = name };
            m.color = c;
            colorCache[c] = m;
            return m;
        }

        /// <summary>Destroy that also works in edit mode (when the editor bakes the course into the scene).</summary>
        public static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }

        public static GameObject LoadModel(string name)
        {
            if (modelCache.TryGetValue(name, out var go)) return go;
            go = Resources.Load<GameObject>(KenneyFolder + name);
            modelCache[name] = go;
            return go;
        }

        public static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform) SetLayerRecursive(child.gameObject, layer);
        }

        /// <summary>
        /// Instantiates a Kenney model and fits it inside a box of <paramref name="size"/>.
        /// A size component of 0 means "ignore this axis" (useful for uniform fitting by height).
        /// Returns the holder transform, or null when the model is not available.
        /// </summary>
        public static Transform FitModel(string model, Transform parent, Vector3 size, bool uniform = false,
            bool bottomAnchor = false, Vector3? localPos = null, Quaternion? localRot = null)
        {
            var prefab = LoadModel(model);
            if (prefab == null) return null;

            var pivot = new GameObject(model + "_Pivot").transform;
            var instance = Object.Instantiate(prefab, pivot, false);
            instance.name = model;
            foreach (var col in instance.GetComponentsInChildren<Collider>()) SafeDestroy(col);

            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                SafeDestroy(pivot.gameObject);
                return null;
            }

            var kenney = KenneyMaterial;
            foreach (var r in renderers)
            {
                var mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = kenney;
                r.sharedMaterials = mats;
                if (r is SkinnedMeshRenderer smr) smr.updateWhenOffscreen = true;
            }

            var b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            Vector3 s = Vector3.one;
            float uniformScale = float.MaxValue;
            for (int axis = 0; axis < 3; axis++)
            {
                if (size[axis] <= 0f || b.size[axis] < 0.0001f) continue;
                s[axis] = size[axis] / b.size[axis];
                uniformScale = Mathf.Min(uniformScale, s[axis]);
            }
            if (uniform)
            {
                if (uniformScale == float.MaxValue) uniformScale = 1f;
                s = Vector3.one * uniformScale;
            }

            var holder = new GameObject(model + "_Visual").transform;
            holder.SetParent(parent, false);
            holder.localPosition = localPos ?? Vector3.zero;
            holder.localRotation = localRot ?? Quaternion.identity;

            pivot.SetParent(holder, false);
            pivot.localScale = s;
            Vector3 offset = -Vector3.Scale(b.center, s);
            if (bottomAnchor) offset.y += b.extents.y * s.y;
            pivot.localPosition = offset;
            SetLayerRecursive(holder.gameObject, parent != null ? parent.gameObject.layer : 0);
            return holder;
        }

        /// <summary>Creates a coloured primitive. Colliders are kept only when requested.</summary>
        public static GameObject Prim(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Color color,
            string name = null, bool collider = false, Quaternion? localRot = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name ?? type.ToString();
            if (!collider) SafeDestroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot ?? Quaternion.identity;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Colored(color);
            if (parent != null) go.layer = parent.gameObject.layer;
            return go;
        }
    }
}
