#if UNITY_EDITOR
using System.Collections.Generic;
using AliZombieDrive;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

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
            rootCol.height = 1.52f;
            rootCol.radius = 0.31f;
            rootCol.center = new Vector3(0f, 0.82f, 0f);

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

        private static void CreateLimb(Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat, float mass, List<Renderer> renderers, PrimitiveType type = PrimitiveType.Capsule)
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
            GameObject lampAsset = LoadModelAsset(FreeAssetBootstrap.LampDir, "lamp");
            GameObject rockAsset = LoadModelAsset(FreeAssetBootstrap.RockDir, "rock");

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

            if (lampAsset != null)
            {
                for (int i = 10; i < 210; i += 10)
                {
                    float z = i * 10f;
                    RoadFrame(road, z, out Vector3 center, out Vector3 forward, out Vector3 right);
                    int side = (i / 10) % 2 == 0 ? 1 : -1;
                    GameObject lamp = InstantiateAsset(lampAsset);
                    if (lamp == null) continue;
                    lamp.name = "CC0 Street Lamp";
                    lamp.transform.SetParent(environment.transform);
                    lamp.transform.position = center + right * side * 9.2f;
                    lamp.transform.rotation = Quaternion.LookRotation(forward * (side < 0 ? -1f : 1f), Vector3.up);
                    StripPhysics(lamp);

                    if (i % 20 == 0)
                    {
                        GameObject lightGo = new GameObject("Road Lamp Light");
                        lightGo.transform.SetParent(environment.transform);
                        lightGo.transform.position = lamp.transform.position + Vector3.up * 5.2f;
                        Light light = lightGo.AddComponent<Light>();
                        light.type = LightType.Point;
                        light.range = 21f;
                        light.intensity = 850f;
                        light.color = new Color(0.72f, 0.82f, 1f);
                        light.shadows = LightShadows.Soft;
                    }
                }
            }

            if (rockAsset != null)
            {
                for (int i = 15; i < 210; i += 11)
                {
                    float z = i * 10f;
                    RoadFrame(road, z, out Vector3 center, out Vector3 forward, out Vector3 right);
                    int side = ((i / 11) % 2 == 0) ? -1 : 1;
                    GameObject rock = InstantiateAsset(rockAsset);
                    if (rock == null) continue;
                    rock.name = "CC0 Rock Face";
                    rock.transform.SetParent(environment.transform);
                    rock.transform.position = center + right * side * Random.Range(15f, 24f) + Vector3.down * 1.1f;
                    rock.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    float s = Random.Range(1.8f, 3.8f);
                    rock.transform.localScale *= s;
                    StripPhysics(rock);
                }
            }
        }

    }
}
#endif
