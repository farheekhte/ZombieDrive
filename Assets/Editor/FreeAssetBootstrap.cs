#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class FreeAssetBootstrap
{
    public const string Root = "Assets/AliZombieDrive/GeneratedAssets";
    public const string AsphaltDir = Root + "/PolyHaven/asphalt_01";
    public const string LampDir = Root + "/PolyHaven/street_lamp_01";
    public const string RockDir = Root + "/PolyHaven/rock_face_01";
    public const string BarrierDir = Root + "/PolyHaven/concrete_road_barrier";
    public const string HdriDir = Root + "/PolyHaven/dikhololo_night";
    public const string GltfDir = Root + "/3DAssets";

    public const string SportsCarPath = GltfDir + "/toycar_cc0.glb";
    public const string InfectedRunnerPath = GltfDir + "/infected_runner.glb";
    public const string BruteInfectedPath = GltfDir + "/brute_infected.glb";

    private const string UserAgent = "AliZombieDrive/0.5 (+https://github.com/farheekhte/ZombieDrive)";

    [MenuItem("Tools/Ali Zombie Drive/Download CC0 Visual Assets")]
    public static void EnsureFreeAssets()
    {
        Directory.CreateDirectory(Root);
        var failures = new List<string>();

        // High-resolution environment art. Poly Haven assets are CC0.
        Try("Poly Haven asphalt_01", () => DownloadPolyHavenTexture("asphalt_01", AsphaltDir));
        Try("Poly Haven street_lamp_01", () => DownloadPolyHavenModel("street_lamp_01", LampDir));
        Try("Poly Haven rock_face_01", () => DownloadPolyHavenModel("rock_face_01", RockDir));
        Try("Poly Haven concrete_road_barrier", () => DownloadPolyHavenModel("concrete_road_barrier", BarrierDir));
        Try("Poly Haven dikhololo_night", () => DownloadPolyHavenHdri("dikhololo_night", HdriDir));

        // Stable CC0 showcase car from Khronos + CC0 infected GLBs.
        Try("Khronos CC0 ToyCar", () => DownloadNamed(
            "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Assets/main/Models/ToyCar/glTF-Binary/ToyCar.glb", SportsCarPath));
        Try("3DAssets infected runner", () => DownloadNamed(
            "https://cdn.3dassets.dev/assets/32707/v1/model.glb", InfectedRunnerPath));
        Try("3DAssets brute infected", () => DownloadNamed(
            "https://cdn.3dassets.dev/assets/12007/v1/model.glb", BruteInfectedPath));

        // Import synchronously so the scene builder can immediately load downloaded models.
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigureTextureImporters();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        void Try(string label, Action action)
        {
            try { action(); }
            catch (Exception ex)
            {
                failures.Add(label + ": " + ex.Message);
                Debug.LogWarning("[AliZombieDrive] Optional CC0 asset failed; generated fallback art will be used. " + label + " -> " + ex.Message);
            }
        }

        if (failures.Count == 0)
            Debug.Log("[AliZombieDrive] CC0 visual asset bootstrap completed.");
        else
            Debug.LogWarning("[AliZombieDrive] CC0 asset bootstrap finished with " + failures.Count + " optional failure(s). Build can continue with fallbacks.");
    }

    public static string FindFirstModel(string folder, params string[] preferredTokens)
    {
        if (!AssetDatabase.IsValidFolder(folder)) return null;
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { folder });
        if (guids == null || guids.Length == 0) return null;
        var paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToList();
        foreach (string token in preferredTokens ?? Array.Empty<string>())
        {
            string hit = paths.FirstOrDefault(p => p.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!string.IsNullOrEmpty(hit)) return hit;
        }
        return paths.FirstOrDefault(p => p.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            ?? paths.FirstOrDefault(p => p.EndsWith(".obj", StringComparison.OrdinalIgnoreCase))
            ?? paths.FirstOrDefault();
    }

    public static GameObject LoadGameObject(string assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath)) return null;
        return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
    }

    public static Texture2D LoadTextureByToken(string folder, string token)
    {
        if (!AssetDatabase.IsValidFolder(folder)) return null;
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        return null;
    }

    public static Cubemap LoadHdriCubemap()
    {
        if (!AssetDatabase.IsValidFolder(HdriDir)) return null;
        foreach (string guid in AssetDatabase.FindAssets("t:Cubemap", new[] { HdriDir }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Cubemap cube = AssetDatabase.LoadAssetAtPath<Cubemap>(path);
            if (cube != null) return cube;
        }
        return null;
    }

    private static void DownloadPolyHavenTexture(string slug, string folder)
    {
        Directory.CreateDirectory(folder);
        JObject root = JObject.Parse(GetText("https://api.polyhaven.com/files/" + slug));
        string[] urls = CollectFileUrls(root)
            .Where(u => IsResolution(u, "1k") && IsImage(u))
            .Where(u => u.Contains("_diff_") || u.Contains("_nor_gl_") || u.Contains("_rough_") || u.Contains("_arm_"))
            .Distinct().ToArray();
        DownloadUrls(urls, folder);
    }

    private static void DownloadPolyHavenModel(string slug, string folder)
    {
        Directory.CreateDirectory(folder);
        JObject root = JObject.Parse(GetText("https://api.polyhaven.com/files/" + slug));
        string[] all = CollectFileUrls(root).Distinct().ToArray();
        string model = all.FirstOrDefault(u => IsResolution(u, "1k") && u.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                    ?? all.FirstOrDefault(u => u.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(model))
            throw new InvalidOperationException("No FBX download found for " + slug);
        DownloadUrl(model, folder);

        IEnumerable<string> textures = all.Where(u => IsResolution(u, "1k") && IsImage(u))
            .Where(u => u.Contains("_diff_") || u.Contains("_nor_gl_") || u.Contains("_rough_") || u.Contains("_arm_") || u.Contains("_metal_") || u.Contains("_opacity_"))
            .Distinct();
        DownloadUrls(textures, folder);
    }

    private static void DownloadPolyHavenHdri(string slug, string folder)
    {
        Directory.CreateDirectory(folder);
        JObject root = JObject.Parse(GetText("https://api.polyhaven.com/files/" + slug));
        string[] all = CollectFileUrls(root).Distinct().ToArray();
        string hdr = all.FirstOrDefault(u => IsResolution(u, "1k") && u.EndsWith(".hdr", StringComparison.OrdinalIgnoreCase))
                  ?? all.FirstOrDefault(u => IsResolution(u, "2k") && u.EndsWith(".hdr", StringComparison.OrdinalIgnoreCase))
                  ?? all.FirstOrDefault(u => u.EndsWith(".hdr", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(hdr)) throw new InvalidOperationException("No HDR file found for " + slug);
        DownloadUrl(hdr, folder);
    }

    private static IEnumerable<string> CollectFileUrls(JToken token)
    {
        if (token == null) yield break;
        if (token is JObject obj)
        {
            if (obj.TryGetValue("url", out JToken urlToken))
            {
                string value = urlToken?.ToString();
                if (!string.IsNullOrWhiteSpace(value)) yield return value;
            }
            foreach (JProperty prop in obj.Properties())
                foreach (string url in CollectFileUrls(prop.Value)) yield return url;
        }
        else if (token is JArray arr)
        {
            foreach (JToken child in arr)
                foreach (string url in CollectFileUrls(child)) yield return url;
        }
    }

    private static void DownloadUrls(IEnumerable<string> urls, string folder)
    {
        foreach (string url in urls) DownloadUrl(url, folder);
    }

    private static void DownloadNamed(string url, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? Root);
        if (File.Exists(destination) && new FileInfo(destination).Length > 1024) return;
        File.WriteAllBytes(destination, GetBytes(url));
        Debug.Log("[AliZombieDrive] Downloaded " + Path.GetFileName(destination));
    }

    private static void DownloadUrl(string url, string folder)
    {
        Directory.CreateDirectory(folder);
        string clean = url.Split('?')[0];
        string name = Uri.UnescapeDataString(Path.GetFileName(clean));
        string destination = Path.Combine(folder, name).Replace('\\', '/');
        if (File.Exists(destination) && new FileInfo(destination).Length > 256) return;
        File.WriteAllBytes(destination, GetBytes(url));
        Debug.Log("[AliZombieDrive] Downloaded " + name);
    }

    private static string GetText(string url) => System.Text.Encoding.UTF8.GetString(GetBytes(url));

    private static byte[] GetBytes(string url)
    {
        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 90;
        request.SetRequestHeader("User-Agent", UserAgent);
        UnityWebRequestAsyncOperation op = request.SendWebRequest();
        while (!op.isDone) Thread.Sleep(20);
        if (request.result != UnityWebRequest.Result.Success)
            throw new IOException(request.error + " [" + url + "]");
        return request.downloadHandler.data;
    }

    private static bool IsResolution(string url, string resolution) =>
        url.IndexOf("/" + resolution + "/", StringComparison.OrdinalIgnoreCase) >= 0 ||
        url.IndexOf("_" + resolution + ".", StringComparison.OrdinalIgnoreCase) >= 0;

    private static bool IsImage(string url) =>
        url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
        url.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
        url.EndsWith(".tif", StringComparison.OrdinalIgnoreCase) ||
        url.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase);

    private static void ConfigureTextureImporters()
    {
        string[] roots = { AsphaltDir, LampDir, RockDir, BarrierDir };
        foreach (string root in roots)
        {
            if (!AssetDatabase.IsValidFolder(root)) continue;
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { root }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer) continue;
                bool normal = path.IndexOf("_nor_", StringComparison.OrdinalIgnoreCase) >= 0;
                importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = !normal &&
                    path.IndexOf("rough", StringComparison.OrdinalIgnoreCase) < 0 &&
                    path.IndexOf("arm", StringComparison.OrdinalIgnoreCase) < 0 &&
                    path.IndexOf("metal", StringComparison.OrdinalIgnoreCase) < 0;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }
        }

        if (!AssetDatabase.IsValidFolder(HdriDir)) return;
        foreach (string guid in AssetDatabase.FindAssets("t:Texture", new[] { HdriDir }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".hdr", StringComparison.OrdinalIgnoreCase)) continue;
            if (AssetImporter.GetAtPath(path) is TextureImporter importer)
            {
                importer.textureShape = TextureImporterShape.TextureCube;
                importer.generateCubemap = TextureImporterGenerateCubemap.AutoCubemap;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
