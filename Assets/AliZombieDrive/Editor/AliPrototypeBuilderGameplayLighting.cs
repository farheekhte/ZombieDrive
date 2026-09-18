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
        private static void CreateGameplayObjects(ProceduralRoad road, Material orange, Material blue)
        {
            GameObject barrierAsset = LoadModelAsset(FreeAssetBootstrap.BarrierDir, "barrier");
            float[] jumpZ = { 420f, 980f, 1510f };
            foreach (float z in jumpZ)
            {
                RoadFrame(road, z, out Vector3 center, out Vector3 forward, out Vector3 right);
                GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ramp.name = "Jump Ramp";
                ramp.transform.position = center + Vector3.up * 0.38f;
                ramp.transform.rotation = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(-11f, 0f, 0f);
                ramp.transform.localScale = new Vector3(4.8f, 0.38f, 5.8f);
                ramp.GetComponent<Renderer>().sharedMaterial = orange;
                BoxCollider trigger = ramp.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.size = new Vector3(1.0f, 4f, 1.2f);
                ramp.AddComponent<JumpPad>();

                float obstacleZ = z + 24f;
                RoadFrame(road, obstacleZ, out Vector3 obstacleCenter, out Vector3 obstacleForward, out Vector3 obstacleRight);
                for (int lane = -1; lane <= 1; lane++)
                {
                    Vector3 p = obstacleCenter + obstacleRight * lane * 2.1f;
                    GameObject obstacle = barrierAsset != null ? InstantiateAsset(barrierAsset) : null;
                    if (obstacle != null)
                    {
                        obstacle.name = "CC0 Jump Barrier";
                        obstacle.transform.position = p;
                        obstacle.transform.rotation = Quaternion.LookRotation(obstacleRight, Vector3.up);
                        obstacle.transform.localScale *= 1.2f;
                    }
                    else
                    {
                        obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        obstacle.name = "Fallback Jump Barrier";
                        obstacle.transform.position = p + Vector3.up * 0.5f;
                        obstacle.transform.rotation = Quaternion.LookRotation(obstacleRight, Vector3.up);
                        obstacle.transform.localScale = new Vector3(2f, 1f, 0.65f);
                        obstacle.GetComponent<Renderer>().sharedMaterial = blue;
                    }
                }
            }

            float[] upgradeZ = { 260f, 760f, 1260f, 1830f };
            foreach (float z in upgradeZ)
            {
                RoadFrame(road, z, out Vector3 center, out Vector3 forward, out Vector3 right);
                GameObject pickup = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pickup.name = "Ram Upgrade";
                pickup.transform.position = center + right * Random.Range(-3.4f, 3.4f) + Vector3.up * 1.1f;
                pickup.transform.localScale = Vector3.one * 0.82f;
                pickup.GetComponent<Renderer>().sharedMaterial = blue;
                pickup.GetComponent<BoxCollider>().isTrigger = true;
                pickup.AddComponent<RamUpgradePickup>();
            }
        }

        private static void CreateLightingAndPost()
        {
            GameObject moon = new GameObject("Moon Light");
            Light sun = moon.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.45f;
            sun.color = new Color(0.38f, 0.49f, 0.76f);
            sun.shadows = LightShadows.Soft;
            moon.transform.rotation = Quaternion.Euler(42f, -34f, 0f);

            GameObject volumeGo = new GameObject("HDRP Global Volume");
            Volume volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10;
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            const string volumePath = "Assets/AliZombieDrive/Ali_NightDrive_Volume.asset";
            AssetDatabase.DeleteAsset(volumePath);
            AssetDatabase.CreateAsset(profile, volumePath);
            volume.sharedProfile = profile;

            Cubemap hdri = FreeAssetBootstrap.LoadHdriCubemap();
            if (hdri != null)
            {
                VisualEnvironment environment = profile.Add<VisualEnvironment>();
                environment.active = true;
                environment.skyType.Override(1);

                HDRISky sky = profile.Add<HDRISky>();
                sky.active = true;
                sky.hdriSky.Override(hdri);
                sky.exposure.Override(-1.15f);
                sky.multiplier.Override(0.9f);
                sky.rotation.Override(18f);
            }

            Bloom bloom = profile.Add<Bloom>();
            bloom.active = true;
            bloom.intensity.Override(0.42f);
            bloom.threshold.Override(0.86f);

            Tonemapping tone = profile.Add<Tonemapping>();
            tone.active = true;
            tone.mode.Override(TonemappingMode.ACES);

            ColorAdjustments color = profile.Add<ColorAdjustments>();
            color.active = true;
            color.postExposure.Override(-0.28f);
            color.contrast.Override(16f);
            color.saturation.Override(-3f);

            Vignette vignette = profile.Add<Vignette>();
            vignette.active = true;
            vignette.intensity.Override(0.2f);
            vignette.smoothness.Override(0.58f);

            Fog fog = profile.Add<Fog>();
            fog.active = true;
            fog.enabled.Override(true);
            fog.meanFreePath.Override(280f);
            fog.baseHeight.Override(-1f);
            fog.maximumHeight.Override(85f);
        }

        private static GameObject LoadModelAsset(string folder, params string[] tokens)
        {
            string path = FreeAssetBootstrap.FindFirstModel(folder, tokens);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static GameObject InstantiateAsset(GameObject asset)
        {
            if (asset == null) return null;
            GameObject instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null) instance = Object.Instantiate(asset);
            return instance;
        }

        private static void StripPhysics(GameObject root)
        {
            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(collider);
            foreach (Rigidbody body in root.GetComponentsInChildren<Rigidbody>(true))
                Object.DestroyImmediate(body);
        }

        private static void DisableImportedLightsAndCameras(GameObject root)
        {
            foreach (Camera camera in root.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
            foreach (Light light in root.GetComponentsInChildren<Light>(true)) light.enabled = false;
        }

        private static void RoadFrame(ProceduralRoad road, float z, out Vector3 center, out Vector3 forward, out Vector3 right)
        {
            center = road.CenterAt(z);
            forward = (road.CenterAt(z + 2f) - road.CenterAt(Mathf.Max(0f, z - 2f))).normalized;
            right = Vector3.Cross(Vector3.up, forward).normalized;
        }
    }
}
#endif
