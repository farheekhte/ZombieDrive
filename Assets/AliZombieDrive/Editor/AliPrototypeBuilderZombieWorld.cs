#if UNITY_EDITOR
using System.Collections.Generic;
using AliZombieDrive;
using UnityEditor;
using UnityEngine;

namespace AliZombieDriveEditor
{
    public static partial class AliPrototypeBuilder
    {
        private static ZombieRagdoll CreateZombiePrefab(Material bodyMat, Material skinMat)
        {
            GameObject root = new GameObject("Zombie_Ragdoll_RuntimePrefab");
            root.transform.position = new Vector3(10000f, -10000f, 10000f);

            Rigidbody pelvis = root.AddComponent<Rigidbody>();
            pelvis.mass = 42f;
            pelvis.isKinematic = true;
            pelvis.collisionDetectionMode = CollisionDetectionMode.Continuous;

            CapsuleCollider rootCol = root.AddComponent<CapsuleCollider>();
            rootCol.height = 1.72f;
            rootCol.radius = 0.34f;
            rootCol.center = new Vector3(0f, 0.86f, 0f);

            ZombieRagdoll zr = root.AddComponent<ZombieRagdoll>();
            var fallbackRenderers = new List<Renderer>();

            CreateLimb(root.transform, "Torso", new Vector3(0f, 1.18f, 0f), new Vector3(0.68f, 0.9f, 0.38f), bodyMat, 12f, fallbackRenderers);
            CreateLimb(root.transform, "Head", new Vector3(0f, 1.84f, 0f), new Vector3(0.38f, 0.38f, 0.38f), skinMat, 4f, fallbackRenderers, PrimitiveType.Sphere);
            CreateLimb(root.transform, "ArmL", new Vector3(-0.47f, 1.22f, 0f), new Vector3(0.22f, 0.78f, 0.22f), skinMat, 3f, fallbackRenderers);
            CreateLimb(root.transform, "ArmR", new Vector3(0.47f, 1.22f, 0f), new Vector3(0.22f, 0.78f, 0.22f), skinMat, 3f, fallbackRenderers);
            CreateLimb(root.transform, "LegL", new Vector3(-0.19f, 0.34f, 0f), new Vector3(0.25f, 0.9f, 0.27f), bodyMat, 5f, fallbackRenderers);
            CreateLimb(root.transform, "LegR", new Vector3(0.19f, 0.34f, 0f), new Vector3(0.25f, 0.9f, 0.27f), bodyMat, 5f, fallbackRenderers);

            GameObject importedZombie = InstantiateAsset(FreeAssetBootstrap.LoadGameObject(FreeAssetBootstrap.InfectedRunnerPath));
            if (importedZombie != null)
            {
                importedZombie.name = "CC0 Infected Runner Visual";
                importedZombie.transform.SetParent(root.transform, false);
                importedZombie.transform.localPosition = Vector3.zero;
                importedZombie.transform.localRotation = Quaternion.identity;
                importedZombie.transform.localScale = Vector3.one;
                StripPhysics(importedZombie);
                DisableImportedLightsAndCameras(importedZombie);
                FitVisualToBounds(importedZombie, 1.88f, true);
                ApplyMaterial(importedZombie, bodyMat);

                GameObject bruteZombie = InstantiateAsset(FreeAssetBootstrap.LoadGameObject(FreeAssetBootstrap.BruteInfectedPath));
                if (bruteZombie != null)
                {
                    bruteZombie.name = "CC0 Brute Infected Visual";
                    bruteZombie.transform.SetParent(root.transform, false);
                    bruteZombie.transform.localPosition = Vector3.zero;
                    bruteZombie.transform.localRotation = Quaternion.identity;
                    bruteZombie.transform.localScale = Vector3.one;
                    StripPhysics(bruteZombie);
                    DisableImportedLightsAndCameras(bruteZombie);
                    FitVisualToBounds(bruteZombie, 2.28f, true);
                    ApplyMaterial(bruteZombie, bodyMat);
                }

                zr.ConfigureVisual(importedZombie, bruteZombie, fallbackRenderers.ToArray(), true);
            }
            else
            {
                zr.ConfigureVisual(null, null, fallbackRenderers.ToArray(), false);
            }

            string path = "Assets/AliZombieDrive/Zombie_Ragdoll.prefab";
            AssetDatabase.DeleteAsset(path);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<ZombieRagdoll>();
        }

        private static void CreateLimb(
            Transform parent,
            string name,
            Vector3 localPos,
            Vector3 localScale,
            Material mat,
            float mass,
            List<Renderer> renderers,
            PrimitiveType type = PrimitiveType.Capsule)
        {
            GameObject limb = GameObject.CreatePrimitive(type);
            limb.name = name;
            limb.transform.SetParent(parent, false);
            limb.transform.localPosition = localPos;
            limb.transform.localScale = localScale;

            Renderer renderer = limb.GetComponent<Renderer>();
            renderer.sharedMaterial = mat;
            renderers.Add(renderer);

            Rigidbody body = limb.AddComponent<Rigidbody>();
            body.mass = mass;
            body.isKinematic = true;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;

            CharacterJoint joint = limb.AddComponent<CharacterJoint>();
            joint.connectedBody = parent.GetComponent<Rigidbody>();

            SoftJointLimit low = joint.lowTwistLimit;
            low.limit = -35f;
            joint.lowTwistLimit = low;

            SoftJointLimit high = joint.highTwistLimit;
            high.limit = 35f;
            joint.highTwistLimit = high;

            SoftJointLimit swing = joint.swing1Limit;
            swing.limit = 48f;
            joint.swing1Limit = swing;
        }

        private static void CreateRoadsideEnvironment(ProceduralRoad road, Material blue, Material orange)
        {
            GameObject environment = new GameObject("Modern Roadside Environment");

            Material groundMat = CreateLitMaterial("Ali_RoadsideGround", new Color(0.018f, 0.025f, 0.02f), 0f, 0.08f);
            Material guardMat = CreateLitMaterial("Ali_GuardRail", new Color(0.20f, 0.23f, 0.26f), 0.72f, 0.56f);
            Material whiteLine = CreateLitMaterial("Ali_RoadLineWhite", new Color(0.72f, 0.75f, 0.78f), 0.05f, 0.42f);
            Material yellowLine = CreateLitMaterial("Ali_RoadLineYellow", new Color(0.72f, 0.52f, 0.08f), 0.04f, 0.45f);
            Material rockMat = CreateLitMaterial("Ali_Rock", new Color(0.075f, 0.085f, 0.09f), 0f, 0.18f);
            Material lampMat = CreateLitMaterial("Ali_LampMetal", new Color(0.12f, 0.14f, 0.16f), 0.78f, 0.42f);

            GameObject lampAsset = LoadModelAsset(FreeAssetBootstrap.LampDir, "lamp");
            GameObject rockAsset = LoadModelAsset(FreeAssetBootstrap.RockDir, "rock");

            // Build continuous roadside shoulders and visual lane guidance.
            for (float z = 10f; z < road.Length - 10f; z += 20f)
            {
                RoadFrame(road, z, out Vector3 center, out Vector3 forward, out Vector3 right);
                Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

                for (int side = -1; side <= 1; side += 2)
                {
                    GameObject shoulder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    shoulder.name = "Roadside Ground";
                    shoulder.transform.SetParent(environment.transform);
                    shoulder.transform.position = center + right * side * 15.2f + Vector3.down * 0.24f;
                    shoulder.transform.rotation = rotation;
                    shoulder.transform.localScale = new Vector3(18f, 0.42f, 21f);
                    shoulder.GetComponent<Renderer>().sharedMaterial = groundMat;
                    Object.DestroyImmediate(shoulder.GetComponent<Collider>());

                    GameObject edgeLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    edgeLine.name = "Road Edge Line";
                    edgeLine.transform.SetParent(environment.transform);
                    edgeLine.transform.position = center + right * side * 5.88f + Vector3.up * 0.035f;
                    edgeLine.transform.rotation = rotation;
                    edgeLine.transform.localScale = new Vector3(0.11f, 0.018f, 20f);
                    edgeLine.GetComponent<Renderer>().sharedMaterial = whiteLine;
                    Object.DestroyImmediate(edgeLine.GetComponent<Collider>());

                    GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    rail.name = "Guard Rail";
                    rail.transform.SetParent(environment.transform);
                    rail.transform.position = center + right * side * 7.15f + Vector3.up * 0.52f;
                    rail.transform.rotation = rotation;
                    rail.transform.localScale = new Vector3(0.13f, 0.42f, 18.8f);
                    rail.GetComponent<Renderer>().sharedMaterial = guardMat;
                    Object.DestroyImmediate(rail.GetComponent<Collider>());
                }

                if (((int)z / 20) % 2 == 0)
                {
                    GameObject laneDash = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    laneDash.name = "Center Lane Dash";
                    laneDash.transform.SetParent(environment.transform);
                    laneDash.transform.position = center + Vector3.up * 0.04f;
                    laneDash.transform.rotation = rotation;
                    laneDash.transform.localScale = new Vector3(0.12f, 0.02f, 6.8f);
                    laneDash.GetComponent<Renderer>().sharedMaterial = yellowLine;
                    Object.DestroyImmediate(laneDash.GetComponent<Collider>());
                }
            }

            // Reflective marker posts.
            for (int i = 5; i < 214; i += 5)
            {
                float z = i * 10f;
                RoadFrame(road, z, out Vector3 center, out Vector3 forward, out Vector3 right);
                for (int side = -1; side <= 1; side += 2)
                {
                    GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    post.name = "Reflective Road Marker";
                    post.transform.SetParent(environment.transform);
                    post.transform.position = center + right * side * 7.5f + Vector3.up * 0.56f;
                    post.transform.localScale = new Vector3(0.075f, 1.12f, 0.075f);
                    post.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                    Object.DestroyImmediate(post.GetComponent<Collider>());
                    post.GetComponent<Renderer>().sharedMaterial = side < 0 ? blue : orange;
                }
            }

            // Sparse practical lighting, not a dense city street.
            if (lampAsset != null)
            {
                for (int i = 12; i < 210; i += 16)
                {
                    float z = i * 10f;
                    RoadFrame(road, z, out Vector3 center, out Vector3 forward, out Vector3 right);
                    int side = (i / 16) % 2 == 0 ? 1 : -1;

                    GameObject lamp = InstantiateAsset(lampAsset);
                    if (lamp == null) continue;
                    lamp.name = "CC0 Street Lamp";
                    lamp.transform.SetParent(environment.transform);
                    lamp.transform.position = center + right * side * 9.2f;
                    lamp.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                    StripPhysics(lamp);
                    FitVisualToBounds(lamp, 4.0f, true);
                    ApplyMaterial(lamp, lampMat);

                    GameObject lightGo = new GameObject("Road Lamp Light");
                    lightGo.transform.SetParent(environment.transform);
                    lightGo.transform.position = lamp.transform.position + Vector3.up * 3.25f;
                    Light light = lightGo.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.range = 24f;
                    light.intensity = 1150f;
                    light.color = new Color(1f, 0.64f, 0.32f);
                    light.shadows = LightShadows.Soft;
                }
            }

            if (rockAsset != null)
            {
                for (int i = 18; i < 210; i += 13)
                {
                    float z = i * 10f;
                    RoadFrame(road, z, out Vector3 center, out Vector3 forward, out Vector3 right);
                    int side = ((i / 13) % 2 == 0) ? -1 : 1;

                    GameObject rock = InstantiateAsset(rockAsset);
                    if (rock == null) continue;
                    rock.name = "CC0 Rock Face";
                    rock.transform.SetParent(environment.transform);
                    rock.transform.position = center + right * side * Random.Range(16f, 22f);
                    rock.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    StripPhysics(rock);
                    FitVisualToBounds(rock, Random.Range(5.5f, 8.5f), true);
                    ApplyMaterial(rock, rockMat);
                }
            }
        }
    }
}
#endif
