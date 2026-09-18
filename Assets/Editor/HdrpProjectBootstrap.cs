#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public static class HdrpProjectBootstrap
{
    private const string PipelineAssetPath = "Assets/AliZombieDrive/Ali_HDRP.asset";

    public static void Ensure()
    {
        PlayerSettings.colorSpace = ColorSpace.Linear;
        EnsureInputHandlingBoth();

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

    // Unity Cloud creates this project from source, so there is no local editor prompt
    // to enable the Input System. Force "Both" so keyboard and modern gamepads work
    // in the generated Windows player.
    private static void EnsureInputHandlingBoth()
    {
        try
        {
            System.Type buildProfileType = typeof(BuildProfile);
            FieldInfo globalPlayerSettingsField =
                buildProfileType.GetField("s_GlobalPlayerSettings", BindingFlags.Static | BindingFlags.NonPublic);

            if (globalPlayerSettingsField == null)
            {
                Debug.LogWarning("[AliZombieDrive] Could not locate global PlayerSettings to enable input backends.");
                return;
            }

            PlayerSettings playerSettings =
                (PlayerSettings)globalPlayerSettingsField.GetValue(null);

            BuildProfile activeBuildProfile = BuildProfile.GetActiveBuildProfile();
            if (activeBuildProfile != null)
            {
                FieldInfo playerSettingsOverrideField =
                    buildProfileType.GetField("m_PlayerSettings", BindingFlags.Instance | BindingFlags.NonPublic);

                if (playerSettingsOverrideField != null)
                {
                    PlayerSettings overrideSettings =
                        (PlayerSettings)playerSettingsOverrideField.GetValue(activeBuildProfile);
                    if (overrideSettings != null)
                        playerSettings = overrideSettings;
                }
            }

            if (playerSettings == null)
            {
                Debug.LogWarning("[AliZombieDrive] PlayerSettings instance unavailable; input backend unchanged.");
                return;
            }

            SerializedObject settingsObject = new SerializedObject(playerSettings);
            SerializedProperty activeInput = settingsObject.FindProperty("activeInputHandler");
            if (activeInput != null)
            {
                activeInput.intValue = 2; // 0=Old, 1=New, 2=Both
                settingsObject.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[AliZombieDrive] Active Input Handling forced to Both.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[AliZombieDrive] Could not force Input Handling to Both: " + ex.Message);
        }
    }
}
#endif
