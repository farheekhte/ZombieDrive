using UnityEngine;

namespace AliZombieDrive
{
    public sealed class AliGameManager : MonoBehaviour
    {
        public static AliGameManager Instance { get; private set; }

        [Header("Run State")]
        [SerializeField] private int score;
        [SerializeField, Range(1, 7)] private int ramLevel = 1;
        [SerializeField] private int zombieKills;
        [SerializeField] private float distanceMeters;

        public int Score => score;
        public int RamLevel => ramLevel;
        public int ZombieKills => zombieKills;
        public float DistanceMeters => distanceMeters;
        public int Wave => Mathf.Clamp(1 + Mathf.FloorToInt(distanceMeters / 850f), 1, 7);

        private Transform player;
        private Vector3 lastPlayerPosition;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            var car = FindFirstObjectByType<ArcadeCarController>();
            if (car != null)
            {
                player = car.transform;
                lastPlayerPosition = player.position;
            }
        }

        private void Update()
        {
            if (player == null) return;
            Vector3 delta = player.position - lastPlayerPosition;
            delta.y = 0f;
            distanceMeters += delta.magnitude;
            lastPlayerPosition = player.position;
        }

        public bool CanCleanKill(int zombieStrength) => ramLevel >= zombieStrength;

        public void RegisterZombieKill(int strength, float impactSpeed)
        {
            zombieKills++;
            int speedBonus = Mathf.RoundToInt(Mathf.Clamp(impactSpeed, 0f, 55f) * 3f);
            score += strength * 100 + speedBonus;
        }

        public void AddRamUpgrade()
        {
            ramLevel = Mathf.Min(7, ramLevel + 1);
            score += 250;
        }
    }
}
