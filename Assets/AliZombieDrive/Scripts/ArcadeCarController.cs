using UnityEngine;
using UnityEngine.InputSystem;

namespace AliZombieDrive
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ArcadeCarController : MonoBehaviour
    {
        [Header("Arcade Handling")]
        [SerializeField] private float acceleration = 42f;
        [SerializeField] private float reverseAcceleration = 18f;
        [SerializeField] private float maxForwardSpeed = 62f;
        [SerializeField] private float maxReverseSpeed = 16f;
        [SerializeField] private float steeringTorque = 13f;
        [SerializeField] private float highSpeedSteeringFactor = 0.42f;
        [SerializeField] private float lateralGrip = 7.5f;
        [SerializeField] private float downforce = 2.4f;
        [SerializeField] private float airControl = 2.2f;
        [SerializeField] private float brakeStrength = 16f;

        [Header("Grounding")]
        [SerializeField] private float suspensionRayLength = 0.75f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private Transform[] suspensionPoints;

        private Rigidbody rb;
        private float throttle;
        private float brake;
        private float steering;
        private bool grounded;

        public float SpeedKph => rb == null ? 0f : rb.linearVelocity.magnitude * 3.6f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = 1420f;
            rb.centerOfMass = new Vector3(0f, -0.45f, 0.1f);
            rb.linearDamping = 0.16f;
            rb.angularDamping = 1.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void Update()
        {
            ReadInput();
        }

        private void FixedUpdate()
        {
            grounded = CheckGrounded();
            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            float forwardSpeed = localVelocity.z;

            if (grounded)
            {
                float driveInput = throttle;
                if (driveInput > 0.01f && forwardSpeed < maxForwardSpeed)
                    rb.AddForce(transform.forward * driveInput * acceleration, ForceMode.Acceleration);

                if (brake > 0.01f)
                {
                    if (forwardSpeed > 1f)
                        rb.AddForce(-transform.forward * brake * brakeStrength, ForceMode.Acceleration);
                    else if (forwardSpeed > -maxReverseSpeed)
                        rb.AddForce(-transform.forward * brake * reverseAcceleration, ForceMode.Acceleration);
                }

                float speed01 = Mathf.InverseLerp(0f, maxForwardSpeed, Mathf.Abs(forwardSpeed));
                float steeringScale = Mathf.Lerp(1f, highSpeedSteeringFactor, speed01);
                float directionSign = Mathf.Abs(forwardSpeed) < 0.6f ? 1f : Mathf.Sign(forwardSpeed);
                rb.AddTorque(Vector3.up * steering * steeringTorque * steeringScale * directionSign, ForceMode.Acceleration);

                Vector3 sideVelocity = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
                rb.AddForce(-sideVelocity * lateralGrip, ForceMode.Acceleration);
                rb.AddForce(-transform.up * rb.linearVelocity.magnitude * downforce, ForceMode.Acceleration);
            }
            else
            {
                rb.AddTorque(transform.up * steering * airControl, ForceMode.Acceleration);
            }
        }

        private bool CheckGrounded()
        {
            if (suspensionPoints == null || suspensionPoints.Length == 0)
                return Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, suspensionRayLength + 0.4f, groundMask, QueryTriggerInteraction.Ignore);

            int contacts = 0;
            foreach (Transform point in suspensionPoints)
            {
                if (point != null && Physics.Raycast(point.position, -transform.up, suspensionRayLength, groundMask, QueryTriggerInteraction.Ignore))
                    contacts++;
            }
            return contacts >= Mathf.Max(1, suspensionPoints.Length / 2);
        }

        private void ReadInput()
        {
            steering = 0f;
            throttle = 0f;
            brake = 0f;

            Gamepad pad = Gamepad.current;
            if (pad != null)
            {
                steering = pad.leftStick.x.ReadValue();
                throttle = pad.rightTrigger.ReadValue();
                brake = pad.leftTrigger.ReadValue();
            }

            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) steering -= 1f;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) steering += 1f;
                if (kb.upArrowKey.isPressed || kb.wKey.isPressed) throttle = Mathf.Max(throttle, 1f);
                if (kb.downArrowKey.isPressed || kb.sKey.isPressed) brake = Mathf.Max(brake, 1f);
            }

            steering = Mathf.Clamp(steering, -1f, 1f);
        }
    }
}
