#if UNITY_EDITOR
using System.Collections.Generic;
using AliZombieDrive;
using UnityEditor;
using UnityEngine;

namespace AliZombieDriveEditor
{
    public static partial class AliPrototypeBuilder
    {
        private static Material CreateAsphaltMaterial()
        {
            Material mat = CreateLitMaterial("Ali_Asphalt", new Color(0.035f, 0.04f, 0.045f), 0.04f, 0.84f);
            Texture2D diffuse = FreeAssetBootstrap.LoadTextureByToken(FreeAssetBootstrap.AsphaltDir, "_diff_");
            Texture2D normal = FreeAssetBootstrap.LoadTextureByToken(FreeAssetBootstrap.AsphaltDir, "_nor_gl_");

            if (diffuse != null)
            {
                if (mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", diffuse);
                else if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", diffuse);
                if (mat.HasProperty("_BaseColorMap")) mat.SetTextureScale("_BaseColorMap", new Vector2(1.15f, 7.5f));
            }

            if (normal != null && mat.HasProperty("_NormalMap"))
            {
                mat.SetTexture("_NormalMap", normal);
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTextureScale("_NormalMap", new Vector2(1.15f, 7.5f));
                if (mat.HasProperty("_NormalScale")) mat.SetFloat("_NormalScale", 0.7f);
            }

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.86f);
            if (mat.HasProperty("_CoatMask")) mat.SetFloat("_CoatMask", 0.5f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreateLitMaterial(string name, Color color, float metallic, float smoothness)
        {
            Shader shader = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
            Material mat = new Material(shader) { name = name, color = color };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            string path = "Assets/AliZombieDrive/" + name + ".mat";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static Material CreateEmissiveMaterial(string name, Color color, float intensity)
        {
            Material mat = CreateLitMaterial(name, Color.black, 0.15f, 0.72f);
            if (mat.HasProperty("_EmissiveColor"))
            {
                mat.EnableKeyword("_EMISSIVE_COLOR_MAP");
                mat.SetColor("_EmissiveColor", color * intensity);
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static GameObject CreateCar(Material paint, Material glass, Material tire)
        {
            GameObject car = new GameObject("Player Car");
            Rigidbody rb = car.AddComponent<Rigidbody>();
            rb.mass = 1420f;

            BoxCollider bodyCol = car.AddComponent<BoxCollider>();
            bodyCol.center = new Vector3(0f, 0.62f, 0f);
            bodyCol.size = new Vector3(2.05f, 1.05f, 4.45f);
            car.AddComponent<ArcadeCarController>();

            GameObject externalCar = InstantiateAsset(FreeAssetBootstrap.LoadGameObject(FreeAssetBootstrap.SportsCarPath));
            if (externalCar != null)
            {
                externalCar.name = "CC0 Sports Car Visual";
                externalCar.transform.SetParent(car.transform, false);
                externalCar.transform.localPosition = Vector3.zero;
                externalCar.transform.localRotation = Quaternion.identity;
                externalCar.transform.localScale = Vector3.one;

                StripPhysics(externalCar);
                DisableImportedLightsAndCameras(externalCar);
                FitVisualToBounds(externalCar, 4.45f, true);
                ApplyCarMaterials(externalCar, paint, glass, tire);
            }
            else
            {
                CreateFallbackCarVisual(car.transform, paint, glass, tire);
            }

            CreateCarLights(car.transform);
            CreateUnderbodyGlow(car.transform);
            return car;
        }

        private static void ApplyCarMaterials(GameObject root, Material paint, Material glass, Material tire)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] source = renderer.sharedMaterials;
                Material[] replacement = new Material[source.Length];

                for (int i = 0; i < source.Length; i++)
                {
                    string materialName = source[i] == null ? string.Empty : source[i].name;
                    string key = (renderer.name + " " + materialName).ToLowerInvariant();

                    if (key.Contains("glass") || key.Contains("window") || key.Contains("windshield"))
                        replacement[i] = glass;
                    else if (key.Contains("tire") || key.Contains("tyre") || key.Contains("rubber"))
                        replacement[i] = tire;
                    else
                        replacement[i] = paint;
                }

                renderer.sharedMaterials = replacement;
            }
        }

        private static void ApplyMaterial(GameObject root, Material material)
        {
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Material[] mats = new Material[Mathf.Max(1, renderer.sharedMaterials.Length)];
                for (int i = 0; i < mats.Length; i++) mats[i] = material;
                renderer.sharedMaterials = mats;
            }
        }

        private static void FitVisualToBounds(GameObject root, float targetLongestDimension, bool alignBottom)
        {
            if (root == null) return;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            float longest = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
            if (longest > 0.0001f)
            {
                float factor = targetLongestDimension / longest;
                root.transform.localScale *= factor;
            }

            renderers = root.GetComponentsInChildren<Renderer>(true);
            b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            Vector3 parentOrigin = root.transform.parent != null ? root.transform.parent.position : root.transform.position;
            Vector3 target = parentOrigin;
            if (!alignBottom) target.y = b.center.y;

            Vector3 currentAnchor = alignBottom
                ? new Vector3(b.center.x, b.min.y, b.center.z)
                : b.center;

            root.transform.position += target - currentAnchor;
        }

        private static void CreateFallbackCarVisual(Transform parent, Material paint, Material glass, Material tire)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Fallback Body";
            body.transform.SetParent(parent, false);
            body.transform.localPosition = new Vector3(0f, 0.58f, 0f);
            body.transform.localScale = new Vector3(2.0f, 0.48f, 4.2f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial = paint;

            GameObject hood = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hood.name = "Fallback Hood";
            hood.transform.SetParent(parent, false);
            hood.transform.localPosition = new Vector3(0f, 0.83f, 1.22f);
            hood.transform.localScale = new Vector3(1.82f, 0.18f, 1.6f);
            Object.DestroyImmediate(hood.GetComponent<Collider>());
            hood.GetComponent<Renderer>().sharedMaterial = paint;

            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Fallback Cabin";
            cabin.transform.SetParent(parent, false);
            cabin.transform.localPosition = new Vector3(0f, 1.05f, -0.28f);
            cabin.transform.localScale = new Vector3(1.48f, 0.5f, 1.72f);
            Object.DestroyImmediate(cabin.GetComponent<Collider>());
            cabin.GetComponent<Renderer>().sharedMaterial = glass;

            for (int side = -1; side <= 1; side += 2)
            for (int axle = 0; axle < 2; axle++)
            {
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"Fallback Wheel {side} {axle}";
                wheel.transform.SetParent(parent, false);
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                wheel.transform.localScale = new Vector3(0.45f, 0.19f, 0.45f);
                wheel.transform.localPosition = new Vector3(side * 1.0f, 0.4f, axle == 0 ? 1.38f : -1.38f);
                Object.DestroyImmediate(wheel.GetComponent<Collider>());
                wheel.GetComponent<Renderer>().sharedMaterial = tire;
            }
        }

        private static void CreateCarLights(Transform car)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject lightGo = new GameObject(side < 0 ? "Headlight L" : "Headlight R");
                lightGo.transform.SetParent(car, false);
                lightGo.transform.localPosition = new Vector3(side * 0.62f, 0.62f, 2.18f);
                lightGo.transform.localRotation = Quaternion.Euler(5f, 0f, 0f);
                Light light = lightGo.AddComponent<Light>();
                light.type = LightType.Spot;
                light.range = 100f;
                light.spotAngle = 46f;
                light.innerSpotAngle = 28f;
                light.intensity = 6800f;
                light.color = new Color(0.78f, 0.88f, 1f);
                light.shadows = LightShadows.Soft;
            }
        }

        private static void CreateUnderbodyGlow(Transform car)
        {
            GameObject glow = new GameObject("Subtle Underbody Light");
            glow.transform.SetParent(car, false);
            glow.transform.localPosition = new Vector3(0f, 0.2f, -0.25f);
            Light light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 4.5f;
            light.intensity = 210f;
            light.color = new Color(0.05f, 0.12f, 0.55f);
            light.shadows = LightShadows.None;
        }
    }
}
#endif
