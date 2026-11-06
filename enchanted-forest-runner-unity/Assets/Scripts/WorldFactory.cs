using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EnchantedForest
{
    /// <summary>
    /// Builds all runtime materials and primitive shapes. Everything in the game
    /// is generated from code so the project stays asset-free.
    /// </summary>
    public class WorldFactory
    {
        public Material Ground, Trail, Bark, Leaf, LeafDark,
                       Obstacle, Crystal, Glow, Gold, TrailGlow;

        public void CreateMaterials()
        {
            Ground   = Lit(new Color(.025f, .095f, .065f));
            Trail    = Lit(new Color(.11f,  .19f,  .20f));
            Bark     = Lit(new Color(.12f, .035f, .06f));
            Leaf     = Lit(new Color(.04f, .26f,  .20f));
            LeafDark = Lit(new Color(.015f,.09f,  .12f));
            Obstacle = Lit(new Color(.26f, .07f,  .08f));
            Crystal  = MakeGlow(new Color(.44f, .18f, 1f),     2.8f);
            Glow     = MakeGlow(new Color(.12f, 1f,    .82f),   3.5f);
            Gold     = MakeGlow(new Color(1f,   .62f,  .14f),   1.5f);
            TrailGlow= MakeGlow(new Color(.10f, .55f, .55f),    1.1f);
        }

        public Material Lit(Color c)
        {
            var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var m = new Material(sh);
            m.color = c;
            return m;
        }

        public Material MakeGlow(Color c, float strength)
        {
            var m = Lit(c);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", c * strength);
            return m;
        }

        public GameObject Shape(PrimitiveType type, string name, Vector3 pos, Vector3 scale,
                                Material mat, Transform parent = null, bool keepCollider = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetPositionAndRotation(pos, Quaternion.identity);
            go.transform.localScale = scale;
            if (parent != null) go.transform.SetParent(parent, true);
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!keepCollider) GameObject.Destroy(go.GetComponent<Collider>());
            return go;
        }

        /// <summary>Creates a URP post-processing volume with bloom + vignette.</summary>
        public static Volume CreatePostFX()
        {
            var go = new GameObject("PostFX Volume");
            var volume = go.AddComponent<Volume>();
            // Global so bloom/vignette apply everywhere — no collider needed,
            // which matters because the world streams indefinitely in +Z.
            volume.isGlobal = true;

            var profile = ScriptableObject.CreateInstance<VolumeProfile>();

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.value = 1.0f;
            bloom.intensity.value = 1.4f;

            var vignette = profile.Add<Vignette>(true);
            vignette.intensity.value = 0.55f;
            vignette.smoothness.value = 0.45f;

            var colorAdjust = profile.Add<ColorAdjustments>(true);
            colorAdjust.contrast.value = 18f;
            colorAdjust.saturation.value = 14f;
            colorAdjust.colorFilter.value = new Color(0.96f, 1.0f, 1.04f);

            volume.profile = profile;
            return volume;
        }
    }
}
