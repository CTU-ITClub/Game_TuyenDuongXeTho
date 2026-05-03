using System.Collections.Generic;
using UnityEngine;

namespace Game.Features.Vehicle
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {

        #region SerializeField Property
        [Header("Motor")]
        [SerializeField] private float _motorForce = 1500f;
        [SerializeField] private float _brakeForce = 3000f;
        [SerializeField] private float _maxSpeed = 20f;

        [Header("Steering")]
        [SerializeField] private float _maxSteerAngle = 35f;

        [Header("Balance")]
        [SerializeField] private float _balanceForce = 600f;
        [SerializeField] private float _balanceDamping = 80f;
        [SerializeField] private float _maxLeanAngle = 12f;

        [Header("Wheels")]
        [SerializeField] private List<WheelCollider> _driveWheels = new();
        [SerializeField] private List<WheelCollider> _steerWheels = new();

        #endregion

        #region Private Property
        private Rigidbody _rb;
        private float _motorInput;
        private float _steerInput;
        #endregion

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        //Nhận input player ở push state
        public void SetMotorInput(float input)
        {
            _motorInput = Mathf.Clamp(input, -1f, 1f);
        }

        //Nhận input player ở steer state
        public void SetSteerInput(float input)
        {
            _steerInput = Mathf.Clamp(input, -1f, 1f);
        }

        private void FixedUpdate()
        {
            HandleMotor();
            HandleSteering();
            HandleBalance();
        }

        //xử lý bánh lái và thắng
        private void HandleMotor()
        {
            float speed = _rb.linearVelocity.magnitude;

            float torque = _motorInput * _motorForce;

            // giới hạn tốc độ
            if (speed > _maxSpeed)
            {
                torque = 0f;
            }

            bool braking = Mathf.Abs(_motorInput) < 0.01f;

            foreach (var wheel in _driveWheels)
            {
                wheel.motorTorque = braking ? 0f : torque;
                wheel.brakeTorque = braking ? _brakeForce : 0f;
            }
        }

        //xử lý bánh điều khiển 
        private void HandleSteering()
        {
            float steerAngle = _steerInput * _maxSteerAngle;

            foreach (var wheel in _steerWheels)
            {
                wheel.steerAngle = steerAngle;
            }
        }

        //cân bằng khi xe đó là 2 bánh
        private void HandleBalance()
        {
            float speed = _rb.linearVelocity.magnitude;

            // nếu đứng yên thì cho nó ko nghiêng
            if (speed < 0.5f)
            {
                StabilizeUpright();
                return;
            }

            float tilt = transform.eulerAngles.z;
            if (tilt > 180f) tilt -= 360f;

            // nghiêng theo hướng rẽ
            float targetTilt = -_steerInput * _maxLeanAngle;

            float angularVel = _rb.angularVelocity.z;

            float torque =
                ((targetTilt - tilt) * _balanceForce)
                - (angularVel * _balanceDamping);

            _rb.AddTorque(transform.forward * torque);
        }

        // hàm xử lý xe khi đứng yên 
        private void StabilizeUpright()
        {
            float tilt = transform.eulerAngles.z;
            if (tilt > 180f) tilt -= 360f;

            float angularVel = _rb.angularVelocity.z;

            float torque =
                (-tilt * _balanceForce * 2f)
                - (angularVel * _balanceDamping);

            _rb.AddTorque(transform.forward * torque);
        }
    }
}