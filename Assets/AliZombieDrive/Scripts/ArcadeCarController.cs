using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AliZombieDrive
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ArcadeCarController : MonoBehaviour
    {
        [Header("Arcade Handling")]
        [SerializeField] private float acceleration = 48f;
        [SerializeField] private float reverseAcceleration = 20f;
        [SerializeField] private float maxForwardSpeed = 64f;
        [SerializeField] private float maxReverseSpeed = 16f;
        [SerializeField] private float steeringTorque = 14f;
        [SerializeField] private float highSpeedSteeringFactor = 0.42f;
        [SerializeField] private float lateralGrip = 7.8f;
        [SerializeField] private float downforce = 2.5f;
        [SerializeField] private float airControl = 2.2f;
        [SerializeField] private float brakeStrength = 18f;

        [Header("Grounding")]
        [SerializeField] private float suspensionRayLength = 1.65f;
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
            rb.centerOfMass = new Vector3(0f, -0.42f, 0.12f);
            rb.linearDamping = 0.12f;
            rb.angularDamping = 1.45f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void Update() => ReadInput();

        private void FixedUpdate()
        {
            grounded = CheckGrounded();
            Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
            float forwardSpeed = localVelocity.z;

            if (grounded)
            {
                if (throttle > 0.01f && forwardSpeed < maxForwardSpeed)
                    rb.AddForce(transform.forward * throttle * acceleration, ForceMode.Acceleration);

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
                // Keep a little control in the air and allow recovery if the road ray momentarily misses.
                rb.AddTorque(transform.up * steering * airControl, ForceMode.Acceleration);
                if (throttle > 0.01f && Mathf.Abs(forwardSpeed) < 4f)
                    rb.AddForce(transform.forward * throttle * 7f, ForceMode.Acceleration);
            }
        }

        private bool CheckGrounded()
        {
            if (suspensionPoints == null || suspensionPoints.Length == 0)
            {
                Vector3 origin = transform.position + Vector3.up * 0.65f;
                return Physics.SphereCast(
                    origin,
                    0.28f,
                    Vector3.down,
                    out _,
                    suspensionRayLength + 0.85f,
                    groundMask,
                    QueryTriggerInteraction.Ignore);
            }

            int contacts = 0;
            foreach (Transform point in suspensionPoints)
            {
                if (point != null && Physics.SphereCast(
                        point.position + transform.up * 0.15f,
                        0.15f,
                        -transform.up,
                        out _,
                        suspensionRayLength,
                        groundMask,
                        QueryTriggerInteraction.Ignore))
                    contacts++;
            }
            return contacts >= Mathf.Max(1, suspensionPoints.Length / 2);
        }

        private void ReadInput()
        {
            steering = 0f;
            throttle = 0f;
            brake = 0f;

            // New Input System: Xbox/PlayStation controller, including analog triggers.
            Gamepad pad = Gamepad.current;
            if (pad != null)
            {
                steering = pad.leftStick.x.ReadValue();
                throttle = Mathf.Max(throttle, pad.rightTrigger.ReadValue());
                brake = Mathf.Max(brake, pad.leftTrigger.ReadValue());

                // Useful fallback for pads/drivers that do not report triggers correctly.
                float stickDrive = pad.leftStick.y.ReadValue();
                if (stickDrive > 0.12f) throttle = Mathf.Max(throttle, stickDrive);
                if (stickDrive < -0.12f) brake = Mathf.Max(brake, -stickDrive);
                if (pad.buttonSouth.isPressed) throttle = Mathf.Max(throttle, 1f);
            }

            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) steering -= 1f;
                if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) steering += 1f;
                if (kb.upArrowKey.isPressed || kb.wKey.isPressed) throttle = Mathf.Max(throttle, 1f);
                if (kb.downArrowKey.isPressed || kb.sKey.isPressed) brake = Mathf.Max(brake, 1f);
            }

            // Legacy input fallback. This also covers many generic USB pads without
            // requiring an Input Actions asset.
            try
            {
                float legacySteer = UnityEngine.Input.GetAxisRaw("Horizontal");
                float legacyDrive = UnityEngine.Input.GetAxisRaw("Vertical");
                if (Mathf.Abs(legacySteer) > Mathf.Abs(steering)) steering = legacySteer;
                if (legacyDrive > 0.08f) throttle = Mathf.Max(throttle, legacyDrive);
                if (legacyDrive < -0.08f) brake = Mathf.Max(brake, -legacyDrive);
            }
            catch (InvalidOperationException)
            {
                // Old backend disabled: the new Input System above remains active.
            }

            steering = Mathf.Clamp(steering, -1f, 1f);
            throttle = Mathf.Clamp01(throttle);
            brake = Mathf.Clamp01(brake);
        }
    }
}
