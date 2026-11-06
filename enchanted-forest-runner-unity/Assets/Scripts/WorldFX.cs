using UnityEngine;

namespace EnchantedForest
{
    /// <summary>
    /// One-shot particle bursts for collect / hit plus a looping ambient spore drift.
    /// FX are cloned whole-gameobject so the particle systems play reliably.
    /// </summary>
    public class WorldFX : MonoBehaviour
    {
        public static WorldFX Instance { get; private set; }

        Transform collectRoot, impactRoot, ambientRoot;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            collectRoot = MakeBurst("CollectSpark", new Color(.5f, 1f, .85f, 1f),
                                    26, .5f, 3.5f, loop: false, overTime: 0f).transform;
            impactRoot = MakeBurst("ImpactShards", new Color(.9f, .3f, .3f, 1f),
                                   34, .4f, 4.5f, loop: false, overTime: 0f).transform;
            ambientRoot = MakeBurst("DriftingSpores", new Color(.7f, 1f, .9f, .5f),
                                    1, 6f, .8f, loop: true, overTime: 24f).transform;
        }

        GameObject MakeBurst(string name, Color color, int count, float life, float speed,
                             bool loop, float overTime)
        {
            var go = new GameObject(name + "Template");
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = loop;
            main.duration = Mathf.Max(1f, life);
            main.startLifetime = life;
            main.startSize = .5f;
            main.startSpeed = speed;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var e = ps.emission;
            e.SetBursts(new[] { new ParticleSystem.Burst(0, count) });
            if (overTime > 0f) e.rateOverTime = overTime;

            var sh = ps.shape;
            sh.shapeType = ParticleSystemShapeType.Sphere;
            sh.radius = loop ? 6f : .4f;

            go.SetActive(false);
            return go;
        }

        public void SpawnCollect(Vector3 pos)  => Fire(collectRoot, pos);
        public void SpawnImpact(Vector3 pos)    => Fire(impactRoot, pos);

        void Fire(Transform template, Vector3 pos)
        {
            var clone = Instantiate(template.gameObject, pos, Quaternion.identity);
            var ps = clone.GetComponent<ParticleSystem>();
            if (ps.main.loop)
            {
                clone.name = clone.name + " (active)";
                ps.Play();
            }
            else
            {
                clone.SetActive(true);
                ps.Play();
                Destroy(clone, ps.main.duration + .5f);
            }
        }

        public void StartAmbient()
        {
            if (ambientRoot == null) return;
            var go = Instantiate(ambientRoot.gameObject);
            go.name = "Ambient Spores";
            go.transform.position = Vector3.zero;
            go.SetActive(true);                       // template was built inactive
            go.GetComponent<ParticleSystem>().Play();
        }
    }
}
