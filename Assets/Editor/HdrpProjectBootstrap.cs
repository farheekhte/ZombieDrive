#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public static class HdrpProjectBootstrap
{
    private const string PipelineAssetPath = "Assets/AliZombieDrive/Ali_HDRP.asset";

    public static void Ensure()
    {
        PlayerSettings.colorSpace = ColorSpace.Linear;
        HDRenderPipelineAsset pipeline =
            AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(PipelineAssetPath);

        if (pipeline == null)
        {
            pipeline = ScriptableObject.CreateInstance<HDRenderPipelineAsset>();
            pipeline.name = "Ali HDRP";
            AssetDatabase.CreateAsset(pipeline, PipelineAssetPath);
            Debug.Log("[AliZombieDrive] Created HDRP asset: " + PipelineAssetPath);
        }

        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;

        var hdrpAssembly = typeof(HDRenderPipelineAsset).Assembly;
        System.Type globalSettingsType =
            hdrpAssembly.GetType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineGlobalSettings");

        if (globalSettingsType != null)
        {
            MethodInfo ensureMethod = globalSettingsType.GetMethod(
                "Ensure",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            if (ensureMethod != null)
            {
                ParameterInfo[] parameters = ensureMethod.GetParameters();
                object[] args = parameters.Length == 1 ? new object[] { true } : null;
                ensureMethod.Invoke(null, args);
                Debug.Log("[AliZombieDrive] HDRP Global Settings ensured.");
            }
            else
            {
                Debug.LogWarning("[AliZombieDrive] HDRP Global Settings Ensure method was not found. HDRP build preprocessing may create defaults automatically.");
            }
        }

        EditorUtility.SetDirty(pipeline);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        if (GraphicsSettings.currentRenderPipelineAssetType != typeof(HDRenderPipelineAsset))
            Debug.LogWarning("[AliZombieDrive] HDRP asset assigned, but current pipeline type has not refreshed yet.");
        else
            Debug.Log("[AliZombieDrive] HDRP is active.");
    }

}
#endif
