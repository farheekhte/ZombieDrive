using UnityEngine;

namespace AliZombieDrive
{
    public sealed class ChaseCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 3.3f, -7.4f);
        [SerializeField] private float positionSharpness = 8f;
        [SerializeField] private float rotationSharpness = 10f;
        [SerializeField] private float lookAhead = 5f;
        [SerializeField] private float baseFov = 61f;
        [SerializeField] private float maxFov = 76f;

        private Camera cam;
        private ArcadeCarController car;

        private void Awake() => cam = GetComponent<Camera>();

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            car = target == null ? null : target.GetComponent<ArcadeCarController>();
        }

        private void Start()
        {
            if (target == null)
            {
                car = FindFirstObjectByType<ArcadeCarController>();
                if (car != null) target = car.transform;
            }
            else car = target.GetComponent<ArcadeCarController>();
        }

        private void LateUpdate()
        {
            if (target == null) return;
            float p = 1f - Mathf.Exp(-positionSharpness * Time.deltaTime);
            float r = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);

            Vector3 desired = target.TransformPoint(localOffset);
            transform.position = Vector3.Lerp(transform.position, desired, p);
            Vector3 look = target.position + target.forward * lookAhead + Vector3.up * 0.8f;
            Quaternion desiredRotation = Quaternion.LookRotation(look - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, r);

            if (cam != null && car != null)
            {
                float speed01 = Mathf.InverseLerp(20f, 190f, car.SpeedKph);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, Mathf.Lerp(baseFov, maxFov, speed01), p);
            }
        }
    }
}
