using System.Collections.Generic;
using UnityEngine;

namespace FallGuyClone
{
    /// <summary>
    /// Builds the obstacle course in code. The course runs along +Z:
    ///  0. Start pad
    ///  1. Sweeper arena (spinning bars to jump over) + spring pads
    ///  2. Wrecking-ball bridge (pendulums)
    ///  3. Sliding platforms over a gap
    ///  4. Barrel ramp
    ///  5. Falling tiles
    ///  6. Punching walls and the finish line
    /// </summary>
    public class CourseBuilder
    {
        public Vector3 SpawnPoint { get; private set; }
        public float StartZ { get; private set; }
        public float FinishZ { get; private set; }
        public float KillY => -4f;
        public List<Checkpoint> Checkpoints { get; } = new List<Checkpoint>();

        Transform root;

        /// <summary>Builds the course and returns its root <see cref="Course"/> component.</summary>
        public static Course Build()
        {
            var b = new CourseBuilder();
            b.BuildAll();
            var course = b.root.gameObject.AddComponent<Course>();
            course.spawnPoint = b.SpawnPoint;
            course.startZ = b.StartZ;
            course.finishZ = b.FinishZ;
            course.killY = b.KillY;
            course.checkpoints = b.Checkpoints;
            return course;
        }

        void BuildAll()
        {
            root = new GameObject("Course").transform;
            StartZ = -3f;
            SpawnPoint = new Vector3(0f, 0.05f, StartZ);

            BuildStart();
            BuildSweepers();
            BuildPendulumBridge();
            BuildMovingPlatforms();
            BuildBarrelRamp();
            BuildFallingTiles();
            BuildFinish();
            BuildScenery();
        }

        // ---------- Sections ----------

        void BuildStart()
        {
            Block("Start Pad", new Vector3(0f, 0f, 2f), new Vector3(16f, 2f, 20f));
            Arch(new Vector3(0f, 0f, 10f), 16f, 5f, Art.Pink, Art.Yellow);
            Decor("tree", new Vector3(-6.5f, 0f, -6.5f), 2.6f);
            Decor("tree-pine", new Vector3(6.5f, 0f, -6.5f), 3f);
            Decor("flowers", new Vector3(-5f, 0f, -7f), 0.8f);
            Decor("sign", new Vector3(4.5f, 0f, -7f), 1.2f, 180f);
        }

        void BuildSweepers()
        {
            Block("Sweeper Arena", new Vector3(0f, 0f, 28f), new Vector3(16f, 2f, 32f), "block-grass-low-large");
            Sweeper(new Vector3(0f, 0f, 20f), 7.6f, 1, 75f, Art.Pink);
            Sweeper(new Vector3(0f, 0f, 35f), 7.6f, 2, -55f, Art.Purple);

            // Spring pads launch the player up onto the raised bridge.
            for (int i = -1; i <= 1; i++) Spring(new Vector3(i * 2.4f, 0f, 42.5f));
            AddCheckpoint(new Vector3(0f, 0f, 41f), 16f);
        }

        void BuildPendulumBridge()
        {
            const float h = 2.5f;
            Block("Bridge", new Vector3(0f, h, 61f), new Vector3(8f, 2f, 34f), "block-grass-large");
            for (int i = 0; i < 4; i++)
            {
                float z = 50f + i * 7f;
                Gantry(new Vector3(0f, h, z), 19f, 10f);
                WreckingBall(new Vector3(0f, h + 9.7f, z), 7.9f, 1.1f, 2.4f + i * 0.15f, i * 0.27f);
            }
            AddCheckpoint(new Vector3(0f, h, 81f), 12f);
        }

        void BuildMovingPlatforms()
        {
            const float h = 2.5f;
            Block("Ledge", new Vector3(0f, h, 81f), new Vector3(12f, 2f, 6f));
            float[] periods = { 3.2f, 2.6f, 3.6f, 2.8f };
            for (int i = 0; i < 4; i++)
            {
                float z = 87f + i * 6f;
                MovingPlatform(new Vector3(0f, h, z), new Vector3(4.5f, 0.8f, 4.5f), new Vector3(4f, 0f, 0f), periods[i], i * 0.31f);
            }
            Block("Landing", new Vector3(0f, 0f, 113f), new Vector3(12f, 2f, 10f));
            AddCheckpoint(new Vector3(0f, 0f, 112f), 12f);
        }

        void BuildBarrelRamp()
        {
            // Ramp from (z118, y0) up to (z148, y6).
            float rise = 6f, run = 30f;
            float angle = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            float length = Mathf.Sqrt(rise * rise + run * run);
            Block("Barrel Ramp", new Vector3(0f, rise * 0.5f, 118f + run * 0.5f), new Vector3(12f, 1f, length),
                "block-grass-large", Quaternion.Euler(-angle, 0f, 0f));
            Block("Ramp Top", new Vector3(0f, 6f, 154f), new Vector3(12f, 2f, 12f));

            var spawner = new GameObject("Barrel Spawner").AddComponent<BarrelSpawner>();
            spawner.transform.SetParent(root, false);
            spawner.transform.position = new Vector3(0f, 7.6f, 150.5f);
            spawner.width = 9f;

            // Barrel dispenser frame over the path (the player walks underneath).
            Art.Prim(PrimitiveType.Cube, root, new Vector3(-6.4f, 8f, 151.5f), new Vector3(0.8f, 4f, 0.8f), Art.Orange, "Dispenser Pillar", true);
            Art.Prim(PrimitiveType.Cube, root, new Vector3(6.4f, 8f, 151.5f), new Vector3(0.8f, 4f, 0.8f), Art.Orange, "Dispenser Pillar", true);
            Art.Prim(PrimitiveType.Cube, root, new Vector3(0f, 10.4f, 151.5f), new Vector3(13.6f, 0.8f, 1.6f), Art.Yellow, "Dispenser Beam", true);
            AddCheckpoint(new Vector3(0f, 6f, 157f), 12f);
        }

        void BuildFallingTiles()
        {
            const float h = 6f;
            int row = 0;
            for (float z = 161.5f; z <= 182.6f; z += 3f, row++)
            {
                for (int c = -2; c <= 2; c++)
                {
                    var color = ((row + c) & 1) == 0 ? Art.Pink : Art.White;
                    FallingTileAt(new Vector3(c * 3f, h, z), color);
                }
            }
        }

        void BuildFinish()
        {
            const float h = 6f;
            Block("Final Stretch", new Vector3(0f, h, 200f), new Vector3(14f, 2f, 32f));
            AddCheckpoint(new Vector3(0f, h, 186.5f), 14f);

            float[] zs = { 191f, 197f, 203f };
            for (int i = 0; i < zs.Length; i++)
            {
                bool left = (i & 1) == 0;
                Pusher(new Vector3(left ? -8.8f : 8.8f, h, zs[i]), left ? 1f : -1f, i * 0.33f);
                Pusher(new Vector3(left ? 8.8f : -8.8f, h, zs[i] + 2.6f), left ? -1f : 1f, i * 0.33f + 0.5f);
            }

            FinishZ = 210f;
            // Checkered finish strip.
            for (int x = 0; x < 14; x++)
            for (int z = 0; z < 2; z++)
                Art.Prim(PrimitiveType.Cube, root, new Vector3(-6.5f + x, h + 0.01f, FinishZ - 0.5f + z),
                    new Vector3(1f, 0.02f, 1f), ((x + z) & 1) == 0 ? Art.White : Art.Dark, "Checker");

            Arch(new Vector3(0f, h, FinishZ), 14f, 6f, Art.Yellow, Art.Pink);
            Decor("flag", new Vector3(-6.8f, h + 6.6f, FinishZ), 1.6f);
            Decor("flag", new Vector3(6.8f, h + 6.6f, FinishZ), 1.6f);
            for (int i = -2; i <= 2; i++)
            {
                var star = Decor("star", new Vector3(i * 2.4f, h + 7.6f, FinishZ), 1f);
                if (star != null) star.gameObject.AddComponent<Spinner>().bobHeight = 0.25f;
            }

            var finish = new GameObject("Finish Line");
            finish.transform.SetParent(root, false);
            finish.transform.position = new Vector3(0f, h + 3f, FinishZ + 0.5f);
            var trigger = finish.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(14f, 6f, 1f);
            finish.AddComponent<FinishLine>();

            Block("Podium", new Vector3(0f, h, 216f), new Vector3(14f, 2f, 8f));
            Decor("tree", new Vector3(-6f, h, 218.5f), 2.6f);
            Decor("tree", new Vector3(6f, h, 218.5f), 2.6f);
            for (int i = -2; i <= 2; i++)
            {
                var coin = Decor("coin-gold", new Vector3(i * 2f, h + 1.2f, 215f), 0.9f);
                if (coin != null) coin.gameObject.AddComponent<Spinner>().bobHeight = 0.2f;
            }
        }

        void BuildScenery()
        {
            // Pink goo far below: falling in sends you back to the last checkpoint.
            var goo = Art.Prim(PrimitiveType.Cube, root, new Vector3(0f, KillY - 1.5f, 105f), new Vector3(600f, 1f, 600f),
                new Color(0.9f, 0.55f, 0.9f), "Goo");
            goo.layer = Layers.Dynamic;

            // Floating islands & clouds for atmosphere.
            var rng = new System.Random(7);
            for (int i = 0; i < 18; i++)
            {
                float side = (i & 1) == 0 ? -1f : 1f;
                float x = side * (22f + (float)rng.NextDouble() * 30f);
                float z = -20f + i * 14f;
                float y = 4f + (float)rng.NextDouble() * 18f;
                var cloudRoot = new GameObject("Cloud").transform;
                cloudRoot.SetParent(root, false);
                cloudRoot.position = new Vector3(x, y, z);
                cloudRoot.gameObject.layer = Layers.Dynamic;
                int puffs = 3 + rng.Next(3);
                for (int p = 0; p < puffs; p++)
                {
                    float s = 3f + (float)rng.NextDouble() * 3f;
                    Art.Prim(PrimitiveType.Sphere, cloudRoot, new Vector3(p * 2.6f - puffs, (float)rng.NextDouble(), (float)rng.NextDouble() * 2f),
                        new Vector3(s, s * 0.7f, s), Art.White, "Puff");
                }
                cloudRoot.gameObject.AddComponent<Spinner>().degreesPerSecond = Vector3.zero;
                cloudRoot.GetComponent<Spinner>().bobHeight = 0.6f;
                cloudRoot.GetComponent<Spinner>().bobSpeed = 0.4f;
            }

            for (int i = 0; i < 8; i++)
            {
                float side = (i & 1) == 0 ? -1f : 1f;
                var island = Block("Island", new Vector3(side * (16f + i * 1.5f), -1f + (i % 3), 10f + i * 27f),
                    new Vector3(5f, 3f, 5f));
                island.layer = Layers.Dynamic;
                Decor(i % 2 == 0 ? "tree-pine" : "tree", island.transform.position + Vector3.up * 0f, 2.8f);
            }
        }

        // ---------- Building blocks ----------

        GameObject Block(string name, Vector3 topCenter, Vector3 size, string model = "block-grass-large", Quaternion? rotation = null)
        {
            var rot = rotation ?? Quaternion.identity;
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.SetPositionAndRotation(topCenter - rot * Vector3.up * (size.y * 0.5f), rot);
            var col = go.AddComponent<BoxCollider>();
            col.size = size;
            if (Art.FitModel(model, go.transform, size) == null)
                Art.Prim(PrimitiveType.Cube, go.transform, Vector3.zero, size, Art.Mint, "Visual");
            return go;
        }

        Transform Decor(string model, Vector3 position, float height, float yaw = 0f)
        {
            var holder = Art.FitModel(model, root, new Vector3(0f, height, 0f), uniform: true, bottomAnchor: true,
                localPos: position, localRot: Quaternion.Euler(0f, yaw, 0f));
            return holder;
        }

        void Arch(Vector3 basePos, float width, float height, Color pillar, Color beam)
        {
            float half = width * 0.5f - 0.6f;
            Art.Prim(PrimitiveType.Cube, root, basePos + new Vector3(-half, height * 0.5f, 0f), new Vector3(1.2f, height, 1.2f), pillar, "Arch Pillar", true);
            Art.Prim(PrimitiveType.Cube, root, basePos + new Vector3(half, height * 0.5f, 0f), new Vector3(1.2f, height, 1.2f), pillar, "Arch Pillar", true);
            Art.Prim(PrimitiveType.Cube, root, basePos + new Vector3(0f, height + 0.5f, 0f), new Vector3(width + 2.4f, 1f, 1.4f), beam, "Arch Beam", true);
            for (int i = 0; i < 9; i++)
            {
                float x = -width * 0.5f + i * (width / 8f);
                Art.Prim(PrimitiveType.Sphere, root, basePos + new Vector3(x, height - 0.1f, 0f), Vector3.one * 0.5f,
                    (i & 1) == 0 ? Art.Cyan : Art.White, "Bulb");
            }
        }

        void Sweeper(Vector3 basePos, float armLength, int bars, float speed, Color color)
        {
            var go = new GameObject("Sweeper");
            go.layer = Layers.Dynamic;
            go.transform.SetParent(root, false);
            go.transform.position = basePos;
            go.AddComponent<Rigidbody>();
            var rot = go.AddComponent<Rotator>();
            rot.degreesPerSecond = speed;
            var hazard = go.AddComponent<Hazard>();
            hazard.force = 13f;
            hazard.upForce = 6f;

            Art.Prim(PrimitiveType.Cylinder, go.transform, new Vector3(0f, 0.9f, 0f), new Vector3(1.4f, 0.9f, 1.4f), Art.Yellow, "Hub", true);
            Art.Prim(PrimitiveType.Sphere, go.transform, new Vector3(0f, 1.8f, 0f), Vector3.one * 1.4f, color, "Cap");
            for (int i = 0; i < bars; i++)
            {
                var q = Quaternion.Euler(0f, i * 180f / bars, 0f);
                Art.Prim(PrimitiveType.Cube, go.transform, new Vector3(0f, 0.6f, 0f), new Vector3(armLength * 2f, 0.45f, 0.45f), color, "Bar", true, q);
                for (int end = -1; end <= 1; end += 2)
                {
                    Vector3 p = q * new Vector3(end * armLength, 0.6f, 0f);
                    var saw = Art.FitModel("saw", go.transform, new Vector3(0.9f, 0.9f, 0f), uniform: true, localPos: p, localRot: q);
                    if (saw == null)
                        Art.Prim(PrimitiveType.Sphere, go.transform, p, Vector3.one * 0.8f, Art.White, "BarEnd");
                }
            }
        }

        void Spring(Vector3 pos)
        {
            var go = new GameObject("Spring Pad");
            go.transform.SetParent(root, false);
            go.transform.position = pos;
            if (Art.FitModel("spring", go.transform, new Vector3(1.8f, 0f, 1.8f), uniform: true, bottomAnchor: true) == null)
                Art.Prim(PrimitiveType.Cylinder, go.transform, new Vector3(0f, 0.1f, 0f), new Vector3(1.8f, 0.1f, 1.8f), Art.Cyan, "Pad");
            var trigger = go.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.6f, 0f);
            trigger.size = new Vector3(1.9f, 1f, 1.9f);
            go.AddComponent<BouncePad>().launchSpeed = 15f;
        }

        void Gantry(Vector3 basePos, float width, float height)
        {
            float half = width * 0.5f;
            Art.Prim(PrimitiveType.Cube, root, basePos + new Vector3(-half, height * 0.5f - 4f, 0f), new Vector3(0.8f, height + 8f, 0.8f), Art.Purple, "Gantry Leg", true);
            Art.Prim(PrimitiveType.Cube, root, basePos + new Vector3(half, height * 0.5f - 4f, 0f), new Vector3(0.8f, height + 8f, 0.8f), Art.Purple, "Gantry Leg", true);
            Art.Prim(PrimitiveType.Cube, root, basePos + new Vector3(0f, height + 0.3f, 0f), new Vector3(width + 0.8f, 0.8f, 0.8f), Art.Purple, "Gantry Beam", true);
        }

        void WreckingBall(Vector3 pivot, float ropeLength, float radius, float period, float phase)
        {
            var go = new GameObject("Wrecking Ball");
            go.layer = Layers.Dynamic;
            go.transform.SetParent(root, false);
            go.transform.position = pivot;
            go.AddComponent<Rigidbody>();
            var pendulum = go.AddComponent<Pendulum>();
            pendulum.axis = Vector3.forward;
            pendulum.amplitude = 58f;
            pendulum.period = period;
            pendulum.phase = phase;
            var hazard = go.AddComponent<Hazard>();
            hazard.force = 15f;
            hazard.upForce = 6f;
            hazard.stunTime = 0.8f;

            Art.Prim(PrimitiveType.Cylinder, go.transform, new Vector3(0f, -ropeLength * 0.5f, 0f), new Vector3(0.18f, ropeLength * 0.5f, 0.18f), Art.Dark, "Rope");
            Art.Prim(PrimitiveType.Sphere, go.transform, new Vector3(0f, -ropeLength - radius * 0.6f, 0f), Vector3.one * radius * 2f, Art.Orange, "Ball", true);
            Art.Prim(PrimitiveType.Sphere, go.transform, new Vector3(0f, -ropeLength - radius * 0.6f, 0f), new Vector3(radius * 2.1f, 0.35f, radius * 2.1f), Art.Yellow, "Stripe");
        }

        void MovingPlatform(Vector3 topCenter, Vector3 size, Vector3 offset, float period, float phase)
        {
            var go = new GameObject("Moving Platform");
            go.layer = Layers.Dynamic;
            go.transform.SetParent(root, false);
            go.transform.position = topCenter - Vector3.up * (size.y * 0.5f);
            var col = go.AddComponent<BoxCollider>();
            col.size = size;
            go.AddComponent<Rigidbody>();
            var osc = go.AddComponent<Oscillator>();
            osc.mode = Oscillator.Mode.Sine;
            osc.offset = offset;
            osc.period = period;
            osc.phase = phase;
            if (Art.FitModel("block-moving-large", go.transform, size) == null)
                Art.Prim(PrimitiveType.Cube, go.transform, Vector3.zero, size, Art.Cyan, "Visual");
        }

        void FallingTileAt(Vector3 topCenter, Color color)
        {
            var size = new Vector3(2.8f, 0.6f, 2.8f);
            var go = new GameObject("Falling Tile");
            go.layer = Layers.Dynamic;
            go.transform.SetParent(root, false);
            go.transform.position = topCenter - Vector3.up * (size.y * 0.5f);
            var col = go.AddComponent<BoxCollider>();
            col.size = size;
            go.AddComponent<Rigidbody>();
            Art.Prim(PrimitiveType.Cube, go.transform, Vector3.zero, size, color, "Visual");
            go.AddComponent<FallingTile>();
        }

        void Pusher(Vector3 basePos, float direction, float phase)
        {
            var go = new GameObject("Pusher");
            go.layer = Layers.Dynamic;
            go.transform.SetParent(root, false);
            go.transform.position = basePos + Vector3.up * 1.1f;
            go.AddComponent<Rigidbody>();
            var osc = go.AddComponent<Oscillator>();
            osc.mode = Oscillator.Mode.Punch;
            osc.offset = new Vector3(direction * 6.5f, 0f, 0f);
            osc.period = 2.6f;
            osc.phase = phase;
            var hazard = go.AddComponent<Hazard>();
            hazard.force = 15f;
            hazard.upForce = 5f;

            Art.Prim(PrimitiveType.Cube, go.transform, Vector3.zero, new Vector3(3.6f, 2.2f, 2.2f), Art.Cyan, "Block", true);
            Art.Prim(PrimitiveType.Cube, go.transform, new Vector3(direction * 1.85f, 0f, 0f), new Vector3(0.2f, 1.6f, 1.6f), Art.Yellow, "Face", true);
            // Rail it slides on (outside the platform).
            Art.Prim(PrimitiveType.Cube, root, basePos + new Vector3(-direction * 1.5f, -0.2f, 0f), new Vector3(4f, 0.4f, 2.6f), Art.Dark, "Rail", true);
        }

        void AddCheckpoint(Vector3 floorCenter, float width)
        {
            var go = new GameObject("Checkpoint " + (Checkpoints.Count + 1));
            go.transform.SetParent(root, false);
            go.transform.position = floorCenter + Vector3.up * 2f;
            var trigger = go.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(width, 4f, 2f);
            var cp = go.AddComponent<Checkpoint>();
            cp.index = Checkpoints.Count + 1;
            cp.respawnPoint = floorCenter + Vector3.up * 0.05f;
            Checkpoints.Add(cp);

            Art.Prim(PrimitiveType.Cube, root, floorCenter + Vector3.up * 0.02f, new Vector3(width, 0.04f, 0.6f), Art.Cyan, "Checkpoint Stripe");
            for (int s = -1; s <= 1; s += 2)
            {
                var pole = new Vector3(s * (width * 0.5f - 0.4f), 0f, 0f);
                Art.Prim(PrimitiveType.Cylinder, root, floorCenter + pole + Vector3.up * 1.2f, new Vector3(0.15f, 1.2f, 0.15f), Art.White, "Checkpoint Pole");
                Art.Prim(PrimitiveType.Sphere, root, floorCenter + pole + Vector3.up * 2.5f, Vector3.one * 0.45f, Art.Cyan, "Checkpoint Light");
            }
        }
    }
}
