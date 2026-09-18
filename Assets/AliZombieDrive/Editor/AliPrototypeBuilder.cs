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
        private const string ScenePath = "Assets/AliZombieDrive/AliZombieDrive_Prototype.unity";

        [MenuItem("Tools/Ali Zombie Drive/Build HDRP Prototype Scene")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Random.InitState(20260918);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.025f, 0.038f, 0.07f);
            RenderSettings.ambientEquatorColor = new Color(0.014f, 0.022f, 0.04f);
            RenderSettings.ambientGroundColor = new Color(0.004f, 0.006f, 0.012f);

            GameObject manager = new GameObject("GameManager");
            manager.AddComponent<AliGameManager>();
            manager.AddComponent<HudOverlay>();

            Material asphalt = CreateAsphaltMaterial();
            Material carMat = CreateLitMaterial("Ali_CarPaint", new Color(0.12f, 0.008f, 0.02f), 0.92f, 0.94f);
            Material glassMat = CreateLitMaterial("Ali_Glass", new Color(0.025f, 0.08f, 0.12f), 0.12f, 0.98f);
            Material tireMat = CreateLitMaterial("Ali_Tire", new Color(0.008f, 0.008f, 0.01f), 0.02f, 0.16f);
            Material zombieMat = CreateLitMaterial("Ali_Zombie", new Color(0.12f, 0.22f, 0.11f), 0f, 0.3f);
            Material skinMat = CreateLitMaterial("Ali_ZombieSkin", new Color(0.32f, 0.42f, 0.24f), 0f, 0.22f);
            Material emissiveBlue = CreateEmissiveMaterial("Ali_NeonBlue", new Color(0.02f, 0.24f, 1f), 9f);
            Material emissiveOrange = CreateEmissiveMaterial("Ali_NeonOrange", new Color(1f, 0.08f, 0.012f), 8f);

            GameObject roadGo = new GameObject("Procedural Road");
            ProceduralRoad road = roadGo.AddComponent<ProceduralRoad>();
            roadGo.GetComponent<MeshRenderer>().sharedMaterial = asphalt;
            road.Build();

            CreateRoadsideEnvironment(road, emissiveBlue, emissiveOrange);

            GameObject car = CreateCar(carMat, glassMat, tireMat);
            car.transform.position = road.CenterAt(12f) + Vector3.up * 0.9f;
            Vector3 carForward = (road.CenterAt(17f) - road.CenterAt(9f)).normalized;
            car.transform.rotation = Quaternion.LookRotation(carForward, Vector3.up);

            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.allowHDR = true;
            cam.nearClipPlane = 0.08f;
            cam.farClipPlane = 3200f;
            cam.fieldOfView = 62f;
            ChaseCamera chase = camGo.AddComponent<ChaseCamera>();
            chase.SetTarget(car.transform);
            camGo.transform.position = car.transform.TransformPoint(new Vector3(0f, 3.15f, -7.8f));
            camGo.transform.LookAt(car.transform.position + car.transform.forward * 6f);

            CreateLightingAndPost();

            ZombieRagdoll zombiePrefab = CreateZombiePrefab(zombieMat, skinMat);
            GameObject spawnerGo = new GameObject("Zombie Spawner");
            ZombieSpawner spawner = spawnerGo.AddComponent<ZombieSpawner>();
            spawner.Configure(zombiePrefab, car.transform, road);

            CreateGameplayObjects(road, emissiveOrange, emissiveBlue);

            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = car;
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath));
            Debug.Log("[AliZombieDrive] HDRP scene generated: " + ScenePath);
        }

    }
}
#endif
