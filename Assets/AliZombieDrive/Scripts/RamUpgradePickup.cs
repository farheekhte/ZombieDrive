using UnityEngine;

namespace AliZombieDrive
{
    public sealed class RamUpgradePickup : MonoBehaviour
    {
        [SerializeField] private float spinSpeed = 55f;
        private void Update() => transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<ArcadeCarController>() == null) return;
            AliGameManager.Instance?.AddRamUpgrade();
            Destroy(gameObject);
        }
    }
}
