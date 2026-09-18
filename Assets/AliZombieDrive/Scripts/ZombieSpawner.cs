using System.Collections.Generic;
using UnityEngine;

namespace AliZombieDrive
{
    public sealed class ZombieSpawner : MonoBehaviour
    {
        [SerializeField] private ZombieRagdoll zombiePrefab;
        [SerializeField] private Transform player;
        [SerializeField] private ProceduralRoad road;
        [SerializeField] private int maxAlive = 18;
        [SerializeField] private float spawnAheadMin = 65f;
        [SerializeField] private float spawnAheadMax = 180f;
        [SerializeField] private float minSpacing = 28f;
        [SerializeField] private float roadHalfWidth = 4.6f;

        private readonly List<ZombieRagdoll> alive = new();
        private float nextSpawnZ = 65f;

        public void Configure(ZombieRagdoll prefab, Transform playerTransform, ProceduralRoad proceduralRoad)
        {
            zombiePrefab = prefab;
            player = playerTransform;
            road = proceduralRoad;
        }

        private void Start()
        {
            if (player == null)
            {
                var car = FindFirstObjectByType<ArcadeCarController>();
                if (car != null) player = car.transform;
            }
            if (road == null) road = FindFirstObjectByType<ProceduralRoad>();
        }

        private void Update()
        {
            alive.RemoveAll(z => z == null);
            if (player == null || road == null || zombiePrefab == null || alive.Count >= maxAlive) return;

            float playerZ = player.position.z;
            if (nextSpawnZ < playerZ + spawnAheadMin) nextSpawnZ = playerZ + spawnAheadMin;

            while (alive.Count < maxAlive && nextSpawnZ < playerZ + spawnAheadMax && nextSpawnZ < road.Length - 80f)
            {
                Spawn(nextSpawnZ);
                int wave = AliGameManager.Instance == null ? 1 : AliGameManager.Instance.Wave;
                float waveTightening = Mathf.Lerp(1f, 0.72f, (wave - 1) / 6f);
                nextSpawnZ += Random.Range(minSpacing, minSpacing + 28f) * waveTightening;
            }
        }

        private void Spawn(float z)
        {
            Vector3 center = road.CenterAt(z);
            Vector3 forward = (road.CenterAt(z + 2f) - road.CenterAt(z - 2f)).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            float lane = Random.Range(-roadHalfWidth, roadHalfWidth);
            Vector3 position = center + right * lane + Vector3.up * 1.1f;
            ZombieRagdoll zombie = Instantiate(zombiePrefab, position, Quaternion.LookRotation(-forward, Vector3.up));
            int wave = AliGameManager.Instance == null ? 1 : AliGameManager.Instance.Wave;
            int strength = Mathf.Clamp(wave + (Random.value < 0.18f ? 1 : 0) - (Random.value < 0.28f ? 1 : 0), 1, 7);
            zombie.SetStrength(strength);
            alive.Add(zombie);
        }
    }
}
