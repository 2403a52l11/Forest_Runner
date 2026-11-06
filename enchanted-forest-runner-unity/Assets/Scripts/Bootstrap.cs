using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

namespace EnchantedForest
{
    /// <summary>
    /// The single component you add to ONE empty GameObject in the scene.
    /// It builds the whole world (materials, runner, ground, trees, obstacles,
    /// fireflies, post-FX, HUD, audio, FX) and drives the run loop.
    ///
    /// Requirements: URP project, a Main Camera tagged MainCamera,
    /// and the new Input System package enabled.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        WorldFactory factory;
        RunnerController runner;
        CameraRig camRig;
        Light sunLight, moonLight;

        // recycle pools for the endless world
        readonly System.Collections.Generic.List<GameObject> scenery = new();
        float nextSpawn = 60f;

        void Awake()
        {
            factory = new WorldFactory();
            factory.CreateMaterials();

            SetupLighting();
            WorldFactory.CreatePostFX();
            gameObject.AddComponent<GameManager>();
            gameObject.AddComponent<GameHUD>();
            gameObject.AddComponent<ProceduralAudio>();
            gameObject.AddComponent<WorldFX>();

            CreateRunner();
            var rig = CreateCameraRig();
            rig.target = runner.transform;

            // initial world
            for (int z = 0; z < 150; z += 12) CreateGround(z);
            for (int z = 14; z < 150; z += 9) CreateForestRow(z);
            for (int z = 30; z < 150; z += 20) CreateObstacle(z);
            for (int z = 16; z < 150; z += 11) CreateFirefly(z);
        }

        void Update()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.UpdateDistance(Time.deltaTime);

            // only stream the world while actually running
            if (GameManager.Instance.State != GameManager.RunState.Playing) return;

            var pz = runner.transform.position.z;
            if (pz + 90f > nextSpawn)
            {
                for (float z = nextSpawn; z < nextSpawn + 60; z += 12) CreateGround(z);
                for (float z = nextSpawn; z < nextSpawn + 60; z += 9) CreateForestRow(z);
                for (float z = nextSpawn + 10; z < nextSpawn + 60; z += 20) CreateObstacle(z);
                for (float z = nextSpawn + 8; z < nextSpawn + 60; z += 11) CreateFirefly(z);
                nextSpawn += 60;
            }
            RecycleWorld(pz - 45f);
        }

        // ---------- setup ----------

        void SetupLighting()
        {
            sunLight = FindFirstObjectByType<Light>();
            if (sunLight != null)
            {
                sunLight.color = new Color(.37f, .55f, 1f);
                sunLight.intensity = 1.15f;
                sunLight.transform.rotation = Quaternion.Euler(38, -35, 0);
            }
            else
            {
                var g = new GameObject("Sun");
                sunLight = g.AddComponent<Light>();
                sunLight.type = LightType.Directional;
                sunLight.color = new Color(.37f, .55f, 1f);
                sunLight.intensity = 1.15f;
                sunLight.transform.rotation = Quaternion.Euler(38, -35, 0);
            }

            var moon = factory.Shape(PrimitiveType.Sphere, "Emerald Moon",
                                     new Vector3(-15, 16, 75), Vector3.one * 9, factory.Crystal);
            var ml = moon.AddComponent<Light>();
            ml.color = new Color(.28f, .38f, 1f);
            ml.range = 55;
            ml.intensity = 4;
            moonLight = ml;
            scenery.Add(moon);
        }

        CameraRig CreateCameraRig()
        {
            var cam = Camera.main ?? new GameObject("Runner Camera").AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.transform.position = new Vector3(0, 5.2f, -9.5f);
            return cam.gameObject.AddComponent<CameraRig>();
        }

        void CreateRunner()
        {
            var hero = new GameObject("Forest Runner");
            hero.transform.position = Vector3.zero;
            runner = hero.AddComponent<RunnerController>();

            var body = factory.Shape(PrimitiveType.Capsule, "Mooncloak Body",
                                     new Vector3(0, 1, 0), new Vector3(.7f, 1f, .7f),
                                     factory.Lit(new Color(.82f, .76f, 1f)), hero.transform);
            var head = factory.Shape(PrimitiveType.Sphere, "Forest Spirit Mask",
                                     new Vector3(0, 1.9f, 0), new Vector3(.62f, .62f, .62f),
                                     factory.Gold, hero.transform);
            for (int side = -1; side <= 1; side += 2)
            {
                var ear = factory.Shape(PrimitiveType.Capsule, "Spirit Ear",
                    new Vector3(.38f * side, 2.3f, 0), new Vector3(.16f, .42f, .16f),
                    factory.Crystal, hero.transform);
                ear.transform.localRotation = Quaternion.Euler(0, 0, -28 * side);
            }
            runner.Body = hero;

            // trail + blinker
            var blink = hero.AddComponent<FlashLight>();
            blink.target = hero;
            runner.Blinker = blink;

            var trailGo = new GameObject("Rune Trail");
            trailGo.transform.SetParent(hero.transform, false);
            trailGo.transform.localPosition = new Vector3(0, 1f, 0);
            var trail = trailGo.AddComponent<TrailRenderer>();
            trail.material = factory.TrailGlow;
            trail.startWidth = .35f;
            trail.endWidth = 0f;
            trail.minDistance = .3f;
            trail.time = .5f;
            trail.autoGenerateMesh = true;
            trail.alphaFromToA = 1f;
            runner.RuneTrail = trail;
        }

        void CreateGround(float z)
        {
            var tile = factory.Shape(PrimitiveType.Cube, "Mossy Trail",
                                     new Vector3(0, -.35f, z + 6), new Vector3(12, .45f, 12),
                                     factory.Ground, null, keepCollider: true);
            scenery.Add(tile);
            for (int lane = -1; lane <= 1; lane++)
                for (int step = 1; step < 12; step += 3)
                {
                    var stone = factory.Shape(PrimitiveType.Cube, "Rune Pathstone",
                        new Vector3(lane * 3, .02f, z + step),
                        new Vector3(1.65f, .18f, 1.8f), factory.Trail);
                    stone.transform.localRotation = Quaternion.Euler(0, Random.Range(-8f, 8f), 0);
                    scenery.Add(stone);
                }
        }

        void CreateForestRow(float z)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * Random.Range(7f, 10f);
                var trunk = factory.Shape(PrimitiveType.Cylinder, "Ancient Tree",
                    new Vector3(x, 3f, z), new Vector3(1.25f, 3f, 1.25f), factory.Bark);
                scenery.Add(trunk);
                for (int i = 0; i < 5; i++)
                {
                    var crown = factory.Shape(PrimitiveType.Sphere, "Moonlit Canopy",
                        trunk.transform.position + new Vector3(Random.Range(-2f, 2f), 4.2f + i * .55f, Random.Range(-1.7f, 1.7f)),
                        Vector3.one * Random.Range(2.4f, 3.6f),
                        i % 2 == 0 ? factory.Leaf : factory.LeafDark);
                    scenery.Add(crown);
                    if (Random.value > .35f) CreateCrystalCluster(trunk.transform.position - new Vector3(side * 1.8f, -0.5f, 0));
                }
            }
        }

        void CreateCrystalCluster(Vector3 p)
        {
            for (int i = 0; i < 3; i++)
            {
                var c = factory.Shape(PrimitiveType.Cylinder, "Amethyst Crystal",
                    p + new Vector3(Random.Range(-.6f, .6f), Random.Range(.25f, .75f), Random.Range(-.6f, .6f)),
                    new Vector3(.18f, Random.Range(.45f, 1.1f), .18f),
                    i == 0 ? factory.Crystal : factory.Glow);
                c.transform.localRotation = Quaternion.Euler(Random.Range(-12f, 12f), 0, Random.Range(-12f, 12f));
                scenery.Add(c);
            }
        }

        void CreateObstacle(float z)
        {
            var root = factory.Shape(PrimitiveType.Cylinder, "Fallen Enchanted Root",
                new Vector3(Random.Range(-2.6f, 2.6f), .65f, z),
                new Vector3(.65f, 1.45f, .65f), factory.Obstacle, null, keepCollider: true);
            root.transform.localRotation = Quaternion.Euler(90, Random.Range(-35f, 35f), 0);
            root.AddComponent<Obstacle>();
            scenery.Add(root);
        }

        void CreateFirefly(float z)
        {
            var orb = new GameObject("Spirit Firefly");
            orb.transform.position = new Vector3(Random.Range(-3f, 3f), 1.6f, z);
            var mesh = factory.Shape(PrimitiveType.Sphere, "Core", Vector3.zero, Vector3.one * .32f,
                                     factory.Glow, orb.transform);
            var col = orb.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = .35f;
            orb.AddComponent<FireflyPickup>();
            scenery.Add(orb);
        }

        void RecycleWorld(float behindZ)
        {
            for (int i = scenery.Count - 1; i >= 0; i--)
            {
                if (scenery[i] == null || scenery[i].transform.position.z < behindZ)
                {
                    Destroy(scenery[i]);
                    scenery.RemoveAt(i);
                }
            }
            if (scenery.Count > 900) // hard cap so memory stays bounded
            {
                Destroy(scenery[0]);
                scenery.RemoveAt(0);
            }
        }

    }
}
