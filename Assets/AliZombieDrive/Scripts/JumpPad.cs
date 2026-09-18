using UnityEngine;

namespace AliZombieDrive
{
    public sealed class JumpPad : MonoBehaviour
    {
        [SerializeField] private float forwardImpulse = 7f;
        [SerializeField] private float upwardImpulse = 10f;

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null || other.GetComponentInParent<ArcadeCarController>() == null) return;
            body.AddForce(transform.forward * forwardImpulse + Vector3.up * upwardImpulse, ForceMode.VelocityChange);
        }
    }
}
