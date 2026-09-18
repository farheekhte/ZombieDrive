using UnityEngine;

namespace AliZombieDrive
{
    public sealed class ZombieRagdoll : MonoBehaviour
    {
        [SerializeField, Range(1, 7)] private int strength = 1;
        [SerializeField] private Rigidbody mainBody;
        [SerializeField] private Rigidbody[] ragdollBodies;
        [SerializeField] private Collider[] ragdollColliders;
        [SerializeField] private GameObject aliveVisual;
        [SerializeField] private GameObject strongVisual;
        [SerializeField] private Renderer[] fallbackRagdollRenderers;
        [SerializeField] private bool keepAliveVisualAfterImpact;
        [SerializeField] private float cleanupDelay = 8f;
        private bool dead;
        private Vector3 aliveVisualBaseScale = Vector3.one;
        private Vector3 strongVisualBaseScale = Vector3.one;

        public int Strength => strength;

        public void SetStrength(int value)
        {
            strength = Mathf.Clamp(value, 1, 7);
            UpdateVisualForStrength();
        }

        public void ConfigureVisual(GameObject visual, GameObject heavyVisual, Renderer[] fallbackRenderers, bool keepVisualOnDeath)
        {
            aliveVisual = visual;
            strongVisual = heavyVisual;
            fallbackRagdollRenderers = fallbackRenderers;
            keepAliveVisualAfterImpact = keepVisualOnDeath;

            if (aliveVisual != null) aliveVisualBaseScale = aliveVisual.transform.localScale;
            if (strongVisual != null) strongVisualBaseScale = strongVisual.transform.localScale;
            UpdateVisualForStrength();
            SetFallbackRenderers(aliveVisual == null && strongVisual == null);
        }

        private void Reset()
        {
            mainBody = GetComponent<Rigidbody>();
            ragdollBodies = GetComponentsInChildren<Rigidbody>(true);
            ragdollColliders = GetComponentsInChildren<Collider>(true);
        }

        private void Awake()
        {
            if (mainBody == null) mainBody = GetComponent<Rigidbody>();
            if (ragdollBodies == null || ragdollBodies.Length == 0) ragdollBodies = GetComponentsInChildren<Rigidbody>(true);
            if (ragdollColliders == null || ragdollColliders.Length == 0) ragdollColliders = GetComponentsInChildren<Collider>(true);
            if (aliveVisual != null) aliveVisualBaseScale = aliveVisual.transform.localScale;
            if (strongVisual != null) strongVisualBaseScale = strongVisual.transform.localScale;
            UpdateVisualForStrength();
            SetFallbackRenderers(aliveVisual == null && strongVisual == null);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (dead) return;
            ArcadeCarController car = collision.collider.GetComponentInParent<ArcadeCarController>();
            if (car == null) return;

            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < 4.5f) return;

            bool clean = AliGameManager.Instance == null || AliGameManager.Instance.CanCleanKill(strength);
            Die(collision, car.transform.forward, impactSpeed, clean);
        }

        private void Die(Collision collision, Vector3 carForward, float impactSpeed, bool clean)
        {
            dead = true;
            GameObject currentVisual = CurrentVisual();

            // Keep the downloaded body on screen while the physical root is thrown and spins.
            // If external art was unavailable, reveal the articulated capsule fallback ragdoll.
            if (currentVisual != null && !keepAliveVisualAfterImpact) currentVisual.SetActive(false);
            SetFallbackRenderers(currentVisual == null || !keepAliveVisualAfterImpact);

            if (AliGameManager.Instance != null)
                AliGameManager.Instance.RegisterZombieKill(strength, impactSpeed);

            ContactPoint contact = collision.contactCount > 0 ? collision.GetContact(0) : default;
            Vector3 hitPoint = collision.contactCount > 0 ? contact.point : transform.position;

            foreach (Rigidbody body in ragdollBodies)
            {
                if (body == null) continue;
                body.isKinematic = false;
                body.useGravity = true;
                body.collisionDetectionMode = CollisionDetectionMode.Continuous;

                Vector3 away = body.worldCenterOfMass - hitPoint;
                if (away.sqrMagnitude < 0.001f) away = Vector3.up;
                away.Normalize();
                float velocity = Mathf.Clamp(impactSpeed, 8f, 34f);
                Vector3 impulse = (carForward * velocity * (clean ? 0.72f : 0.92f) + away * 4.2f + Vector3.up * 3.8f) * body.mass;
                body.AddForce(impulse, ForceMode.Impulse);
                body.AddTorque(Random.onUnitSphere * velocity * body.mass * 0.42f, ForceMode.Impulse);
            }

            foreach (Collider col in ragdollColliders)
                if (col != null) col.enabled = true;

            Destroy(gameObject, cleanupDelay);
        }

        private void UpdateVisualForStrength()
        {
            bool useStrong = strength >= 5 && strongVisual != null;
            if (aliveVisual != null)
            {
                aliveVisual.SetActive(!useStrong);
                float scale = Mathf.Lerp(0.98f, 1.10f, Mathf.Clamp01((strength - 1) / 4f));
                aliveVisual.transform.localScale = aliveVisualBaseScale * scale;
            }
            if (strongVisual != null)
            {
                strongVisual.SetActive(useStrong);
                float scale = Mathf.Lerp(0.92f, 1.08f, Mathf.Clamp01((strength - 5) / 2f));
                strongVisual.transform.localScale = strongVisualBaseScale * scale;
            }
        }

        private GameObject CurrentVisual()
        {
            if (strength >= 5 && strongVisual != null) return strongVisual;
            return aliveVisual;
        }

        private void SetFallbackRenderers(bool visible)
        {
            if (fallbackRagdollRenderers == null) return;
            foreach (Renderer renderer in fallbackRagdollRenderers)
                if (renderer != null) renderer.enabled = visible;
        }
    }
}
