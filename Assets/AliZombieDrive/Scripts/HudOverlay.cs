using UnityEngine;

namespace AliZombieDrive
{
    public sealed class HudOverlay : MonoBehaviour
    {
        private ArcadeCarController car;
        private GUIStyle big;
        private GUIStyle small;

        private void Start() => car = FindFirstObjectByType<ArcadeCarController>();

        private void OnGUI()
        {
            big ??= new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            small ??= new GUIStyle(GUI.skin.label) { fontSize = 16, normal = { textColor = new Color(.85f,.9f,1f,1f) } };
            AliGameManager gm = AliGameManager.Instance;
            if (gm == null) return;

            GUI.Box(new Rect(18, 18, 285, 135), GUIContent.none);
            GUI.Label(new Rect(34, 28, 240, 38), $"{(car == null ? 0f : car.SpeedKph):0} km/h", big);
            GUI.Label(new Rect(34, 72, 250, 28), $"Score  {gm.Score:N0}", small);
            GUI.Label(new Rect(34, 98, 250, 28), $"Ram  {gm.RamLevel}     Wave  {gm.Wave}", small);
            GUI.Label(new Rect(34, 124, 250, 28), $"Kills  {gm.ZombieKills}", small);
            GUI.Label(new Rect(18, Screen.height - 30, 430, 24), "CC0 visuals: Poly Haven + 3DAssets.dev", small);
        }
    }
}
