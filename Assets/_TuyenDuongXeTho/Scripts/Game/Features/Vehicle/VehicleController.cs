using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Game.Features.Vehicle
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviourPun
    {
        #region SerializeField

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

        private Rigidbody _rb;

        public  float _motorInput;
        public float _steerInput;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        // =====================================================
        // PUSH INPUT
        // =====================================================

        public void SetMotorInput(float input)
        {
            input = Mathf.Clamp(input, -1f, 1f);

            // nếu là owner -> set trực tiếp
            if (photonView.IsMine)
            {
                _motorInput = input;
            }
            else
            {
                // gửi input về owner
                photonView.RPC(
                    nameof(RPC_SetMotorInput),
                    photonView.Owner,
                    input
                );
            }

            Debug.Log($"IsMine: {photonView.IsMine} | Owner: {photonView.Owner}");
        }

        // =====================================================
        // STEER INPUT
        // =====================================================

        public void SetSteerInput(float input)
        {
            input = Mathf.Clamp(input, -1f, 1f);

            // nếu là owner -> set trực tiếp
            if (photonView.IsMine)
            {
                _steerInput = input;
            }
            else
            {
                // gửi input về owner
                photonView.RPC(
                    nameof(RPC_SetSteerInput),
                    photonView.Owner,
                    input
                );
            }
        }

        // =====================================================
        // RPC RECEIVE
        // =====================================================

        [PunRPC]
        private void RPC_SetMotorInput(float input)
        {
            // chỉ owner mới nhận
            if (!photonView.IsMine)
                return;

            _motorInput = input;
        }

        [PunRPC]
        private void RPC_SetSteerInput(float input)
        {
            // chỉ owner mới nhận
            if (!photonView.IsMine)
                return;

            _steerInput = input;
        }

        // =====================================================
        // PHYSICS
        // =====================================================

        private void FixedUpdate()
        {
            // chỉ owner simulate physics
            if (!photonView.IsMine)
                return;

            HandleMotor();
            HandleSteering();
            HandleBalance();
            Debug.Log(photonView.IsMine);
        }

        // =====================================================
        // MOTOR
        // =====================================================

        private void HandleMotor()
        {
            float speed = _rb.linearVelocity.magnitude;

            float torque = _motorInput * _motorForce;

            if (speed > _maxSpeed)
            {
                torque = 0f;
            }

            bool braking = Mathf.Abs(_motorInput) < 0.01f;

            foreach (var wheel in _driveWheels)
            {
                wheel.motorTorque =
                    braking ? 0f : torque;

                wheel.brakeTorque =
                    braking ? _brakeForce : 0f;
            }

            Debug.Log(_driveWheels.Count);
            Debug.Log(torque);
            Debug.Log($"Motor: {_motorInput}");
        }

        // =====================================================
        // STEERING
        // =====================================================

        private void HandleSteering()
        {
            float steerAngle =
                _steerInput * _maxSteerAngle;

            foreach (var wheel in _steerWheels)
            {
                wheel.steerAngle = steerAngle;
            }
        }

        // =====================================================
        // BALANCE
        // =====================================================

        private void HandleBalance()
        {
            float speed = _rb.linearVelocity.magnitude;

            if (speed < 0.5f)
            {
                StabilizeUpright();
                return;
            }

            float tilt = transform.eulerAngles.z;

            if (tilt > 180f)
                tilt -= 360f;

            float targetTilt =
                -_steerInput * _maxLeanAngle;

            float angularVel =
                _rb.angularVelocity.z;

            float torque =
                ((targetTilt - tilt) * _balanceForce)
                - (angularVel * _balanceDamping);

            _rb.AddTorque(transform.forward * torque);
        }

        // =====================================================
        // STABILIZE
        // =====================================================

        private void StabilizeUpright()
        {
            float tilt = transform.eulerAngles.z;

            if (tilt > 180f)
                tilt -= 360f;

            float angularVel =
                _rb.angularVelocity.z;

            float torque =
                (-tilt * _balanceForce * 2f)
                - (angularVel * _balanceDamping);

            _rb.AddTorque(transform.forward * torque);
        }
    }
}