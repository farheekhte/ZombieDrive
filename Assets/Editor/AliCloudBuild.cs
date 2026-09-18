#if UNITY_EDITOR
using AliZombieDriveEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AliCloudBuild
{
    private const string ScenePath = "Assets/AliZombieDrive/AliZombieDrive_Prototype.unity";

    // Configure this method in Unity Build Automation as the Pre-export method:
    // AliCloudBuild.PreExport
    public static void PreExport()
    {
        Debug.Log("[AliZombieDrive] Preparing project for cloud build...");

        PlayerSettings.companyName = "Farheekhte";
        PlayerSettings.productName = "Ali Zombie Drive";
        PlayerSettings.bundleVersion = "0.5.0";
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.runInBackground = true;

        // A package reference alone does not activate HDRP. Ensure a pipeline asset,
        // global settings/resources and Linear color space exist before creating HDRP materials.
        HdrpProjectBootstrap.Ensure();

        // Pull optional CC0 art first. Any failed download is non-fatal and the scene builder has fallbacks.
        FreeAssetBootstrap.EnsureFreeAssets();

        // Build the gameplay scene automatically. This removes the need to open Unity locally
        // and press Tools > Ali Zombie Drive > Build HDRP Prototype Scene.
        AliPrototypeBuilder.Build();

        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (sceneAsset == null)
            throw new System.InvalidOperationException("Cloud build scene was not generated: " + ScenePath);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[AliZombieDrive] Cloud build preparation complete. Scene: " + ScenePath);
    }
}
#endif
