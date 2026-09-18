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
        private static Material CreateAsphaltMaterial()
        {
            Material mat = CreateLitMaterial("Ali_Asphalt", new Color(0.065f, 0.07f, 0.075f), 0.02f, 0.78f);
            Texture2D diffuse = FreeAssetBootstrap.LoadTextureByToken(FreeAssetBootstrap.AsphaltDir, "_diff_");
            Texture2D normal = FreeAssetBootstrap.LoadTextureByToken(FreeAssetBootstrap.AsphaltDir, "_nor_gl_");

            if (diffuse != null)
            {
                if (mat.HasProperty("_BaseColorMap")) mat.SetTexture("_BaseColorMap", diffuse);
                else if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", diffuse);
                if (mat.HasProperty("_BaseColorMap")) mat.SetTextureScale("_BaseColorMap", new Vector2(2.2f, 2.2f));
            }

            if (normal != null && mat.HasProperty("_NormalMap"))
            {
                mat.SetTexture("_NormalMap", normal);
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTextureScale("_NormalMap", new Vector2(2.2f, 2.2f));
            }

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.82f);
            if (mat.HasProperty("_CoatMask")) mat.SetFloat("_CoatMask", 0.42f);
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
            bodyCol.center = new Vector3(0f, 0.66f, 0f);
            bodyCol.size = new Vector3(2.08f, 1.06f, 4.48f);
            car.AddComponent<ArcadeCarController>();

            GameObject externalCar = InstantiateAsset(FreeAssetBootstrap.LoadGameObject(FreeAssetBootstrap.SportsCarPath));
            if (externalCar != null)
            {
                externalCar.name = "Modern CC0 Sports Car Visual";
                externalCar.transform.SetParent(car.transform, false);
                externalCar.transform.localPosition = Vector3.zero;
                externalCar.transform.localRotation = Quaternion.identity;
                externalCar.transform.localScale = Vector3.one;
                StripPhysics(externalCar);
                DisableImportedLightsAndCameras(externalCar);
            }
            else
            {
                CreateFallbackCarVisual(car.transform, paint, glass, tire);
            }

            CreateCarLights(car.transform);
            CreateUnderbodyGlow(car.transform);
            return car;
        }

        private static void CreateFallbackCarVisual(Transform parent, Material paint, Material glass, Material tire)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Fallback Body";
            body.transform.SetParent(parent, false);
            body.transform.localPosition = new Vector3(0f, 0.66f, 0f);
            body.transform.localScale = new Vector3(2.02f, 0.55f, 4.25f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial = paint;

            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Fallback Cabin";
            cabin.transform.SetParent(parent, false);
            cabin.transform.localPosition = new Vector3(0f, 1.13f, -0.18f);
            cabin.transform.localScale = new Vector3(1.52f, 0.52f, 1.92f);
            Object.DestroyImmediate(cabin.GetComponent<Collider>());
            cabin.GetComponent<Renderer>().sharedMaterial = glass;

            for (int side = -1; side <= 1; side += 2)
            for (int axle = 0; axle < 2; axle++)
            {
                GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"Fallback Wheel {side} {axle}";
                wheel.transform.SetParent(parent, false);
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                wheel.transform.localScale = new Vector3(0.48f, 0.22f, 0.48f);
                wheel.transform.localPosition = new Vector3(side * 1.02f, 0.43f, axle == 0 ? 1.38f : -1.38f);
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
                lightGo.transform.localPosition = new Vector3(side * 0.67f, 0.62f, 2.24f);
                lightGo.transform.localRotation = Quaternion.Euler(6f, 0f, 0f);
                Light light = lightGo.AddComponent<Light>();
                light.type = LightType.Spot;
                light.range = 82f;
                light.spotAngle = 52f;
                light.innerSpotAngle = 30f;
                light.intensity = 4200f;
                light.color = new Color(0.78f, 0.88f, 1f);
                light.shadows = LightShadows.Soft;
            }
        }

        private static void CreateUnderbodyGlow(Transform car)
        {
            GameObject glow = new GameObject("Subtle Underbody Light");
            glow.transform.SetParent(car, false);
            glow.transform.localPosition = new Vector3(0f, 0.24f, -0.25f);
            Light light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 5f;
            light.intensity = 320f;
            light.color = new Color(0.08f, 0.18f, 1f);
            light.shadows = LightShadows.None;
        }

    }
}
#endif
