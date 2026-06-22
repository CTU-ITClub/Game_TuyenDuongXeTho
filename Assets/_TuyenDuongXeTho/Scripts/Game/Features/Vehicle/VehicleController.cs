using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using UnityEngine.UI;

namespace Game.Features.Vehicle
{
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviourPun
    {
        #region SerializeField

        [Header("Motor")]
        [SerializeField] private float _motorForce = 1500f;
        [SerializeField] private float _baseMotorForce = 1500f;
        bool _isBoosting = false;

        [SerializeField] private float _brakeForce = 3000f;
        [SerializeField] private float _maxSpeed = 20f;
        [SerializeField] private float _baseMaxSpeed = 20f;

        [Header("Steering")]
        [SerializeField] private float _maxSteerAngle = 35f;

        [Header("Balance")]
        [SerializeField] private float _balanceForce = 600f;
        [SerializeField] private float _balanceDamping = 80f;
        [SerializeField] private float _maxLeanAngle = 12f;

        [Header("Wheels")]
        [SerializeField] private List<WheelCollider> _driveWheels = new();
        [SerializeField] private List<WheelCollider> _steerWheels = new();
        [SerializeField] private float _steerSmoothSpeed = 5f;

        private float _currentSteerAngle;

        [Header("Sound Settings")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _rideSound;
        private bool _isSoundPlayingState = false;

        #endregion

        private Rigidbody _rb;

        public float _motorInput;
        public float _steerInput;

        [Header("Stamina")]
        [SerializeField] private float _stamina = 100f;
        [SerializeField] private float _staminaConsumptionRate = 20f; // Stamina tiêu hao mỗi giây khi boost
        [SerializeField] private float _staminaRecoveryRate = 10f; // Stamina hồi phục mỗi giây khi không boost
        [SerializeField] private float _staminaRegenDelay = 2f; // Thời gian chờ sau khi ngừng boost trước khi bắt đầu hồi phục
        bool _canRegenStamina = true;
        private bool _regenTimerStarted;

        public Image staminaBar;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            _rb.linearDamping = 1.5f;
            _rb.angularDamping = 2f;
            _rb.centerOfMass = new Vector3(0f, -0.7f, 0f);

            _baseMaxSpeed = _maxSpeed;
            _baseMotorForce = _motorForce;
        }

        private void Update()
        {
            UpdateStaminaUI();
        }

        // =========================
        // STAMINA
        // =========================
        private void LateUpdate()
        {
            HandleStamina();
        }

        private void EnableStaminaRegen()
        {
            _canRegenStamina = true;
            _regenTimerStarted = false;
        }

        private void HandleStamina()
        {
            if (_isBoosting)
            {
                CancelInvoke(nameof(EnableStaminaRegen));

                _regenTimerStarted = false;
                _canRegenStamina = false;

                _stamina -= _staminaConsumptionRate * Time.deltaTime;

                if (_stamina <= 0f)
                {
                    _stamina = 0f;
                    SetBoost(false);
                    return;
                }
            }
            else
            {
                if (!_canRegenStamina && !_regenTimerStarted)
                {
                    _regenTimerStarted = true;
                    Invoke(nameof(EnableStaminaRegen), _staminaRegenDelay);
                }
                if (_canRegenStamina && _stamina < 100f)
                {
                    _stamina += _staminaRecoveryRate * Time.deltaTime;
                    _stamina = Mathf.Clamp(_stamina, 0f, 100f);
                }
            }
        }

        void UpdateStaminaUI()
        {
            if (staminaBar != null)
            {
                staminaBar.fillAmount = _stamina / 100f;
            }
        }
        // =========================
        // PUSH INPUT
        // =========================

        public void SetMotorInput(float input)
        {
            input = Mathf.Clamp(input, -1f, 1f);

            // Nếu là owner -> set trực tiếp
            if (photonView.IsMine)
            {
                _motorInput = input;
                SyncAudioState();
            }
            else
            {
                // Gửi input về owner
                photonView.RPC(
                    nameof(RPC_SetMotorInput),
                    photonView.Owner,
                    input
                );
            }
        }

        public void SetSteerInput(float input)
        {
            input = Mathf.Clamp(input, -1f, 1f);

            // Nếu là owner -> set trực tiếp
            if (photonView.IsMine)
            {
                _steerInput = input;
            }
            else
            {
                // Gửi input về owner
                photonView.RPC(
                    nameof(RPC_SetSteerInput),
                    photonView.Owner,
                    input
                );
            }
        }

        // =========================
        // BOOST
        // =========================
        public void SetBoost(bool boost)
        {
            if (_isBoosting == boost) return; // Tránh gửi RPC trùng lặp liên tục

            _isBoosting = boost;

             ApplyBoostState(boost);
             photonView.RPC(nameof(RPC_SetBoost), RpcTarget.Others, boost);
        }

        [PunRPC]
        private void RPC_SetBoost(bool boost)
        {
            _isBoosting = boost;
            ApplyBoostState(boost);
        }

        private void ApplyBoostState(bool boost)
        {
            if (boost)
            {
                _motorForce = _baseMotorForce * 6;
                _maxSpeed = _baseMaxSpeed * 4;
            }
            else
            {
                _motorForce = _baseMotorForce;
                _maxSpeed = _baseMaxSpeed;
            }
        }

        // =========================
        // SOUND
        // =========================

        private void SyncAudioState()
        {
            // Điều kiện để chạy xe (khác 0 hoặc tuyệt đối lớn hơn mức mồi ga)
            bool shouldPlay = Mathf.Abs(_motorInput) > 0.05f;

            if (shouldPlay != _isSoundPlayingState)
            {
                _isSoundPlayingState = shouldPlay;
                // Gửi trạng thái play/stop cho tất cả các máy trong phòng
                photonView.RPC(nameof(RPC_SetSoundState), RpcTarget.All, shouldPlay);
            }
        }

        [PunRPC]
        private void RPC_SetSoundState(bool play)
        {
            if (play)
            {
                PlayRideSound();
            }
            else
            {
                StopRideSound();
            }
        }

        public void PlayRideSound()
        {
            if (_audioSource != null && !_audioSource.isPlaying && _rideSound != null)
            {
                _audioSource.clip = _rideSound;
                _audioSource.Play();
            }
        }

        public void StopRideSound()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }

        // =========================
        // RPC RECEIVE
        // =========================

        [PunRPC]
        private void RPC_SetMotorInput(float input)
        {
            // Chỉ owner mới nhận
            if (!photonView.IsMine)
                return;

            _motorInput = input;
            SyncAudioState();
        }

        [PunRPC]
        private void RPC_SetSteerInput(float input)
        {
            // Chỉ owner mới nhận
            if (!photonView.IsMine)
                return;

            _steerInput = input;
        }

        // =========================
        // PHYSICS
        // =========================

        private void FixedUpdate()
        {
            // Chỉ owner simulate physics
            if (!photonView.IsMine)
                return;

            HandleMotor();
            HandleSteering();
            HandleBalance();

            Debug.Log(photonView.IsMine);
        }

        // =========================
        // MOTOR
        // =========================

        private void HandleMotor()
        {
            float speed = _rb.linearVelocity.magnitude;

            bool braking =
                Mathf.Abs(_motorInput) < 0.05f;

            float torque = _motorInput * _motorForce;

            if (speed > _maxSpeed)
            {
                torque = 0f;
            }

            foreach (var wheel in _driveWheels)
            {
                if (braking)
                {
                    wheel.motorTorque = 0f;
                    wheel.brakeTorque = _brakeForce;
                }
                else
                {
                    wheel.brakeTorque = 0f;
                    wheel.motorTorque = torque;
                }
            }

            // Chống trôi khi không input
            if (braking && speed < 0.5f)
            {
                _rb.linearVelocity *= 0.9f;
            }
        }

        // =========================
        // STEERING
        // =========================

        private void HandleSteering()
        {
            float targetAngle =
                _steerInput * _maxSteerAngle;

            _currentSteerAngle = Mathf.Lerp(
                _currentSteerAngle,
                targetAngle,
                Time.fixedDeltaTime * _steerSmoothSpeed
            );

            foreach (var wheel in _steerWheels)
            {
                wheel.steerAngle = _currentSteerAngle;
            }
        }

        // =========================
        // BALANCE
        // =========================

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
                Vector3.Dot(
                    _rb.angularVelocity,
                    transform.forward
                );

            float torque =
                ((targetTilt - tilt) * _balanceForce)
                - (angularVel * _balanceDamping);

            _rb.AddTorque(transform.forward * torque);
        }

        // =========================
        // STABILIZE
        // =========================

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

        // =========================
        // RESET VEHICLE RPC
        // =========================

        public void ResetVehicleRPC(Transform point)
        {
            if (point == null)
            {
                Debug.LogWarning("Reset point is null!");
                return;
            }

            photonView.RPC(
                nameof(RPC_ResetVehicle),
                RpcTarget.All,
                point.position,
                point.rotation
            );
        }

        [PunRPC]
        private void RPC_ResetVehicle(Vector3 pos, Quaternion rot)
        {
            ResetVehicle(pos, rot);
        }

        // =========================
        // LOCAL RESET
        // =========================

        public void ResetVehicle(Vector3 pos, Quaternion rot)
        {
            // Reset velocity
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;

            // Reset transform
            _rb.position = pos;
            _rb.rotation = rot;

            // Reset input
            _motorInput = 0f;
            _steerInput = 0f;

            // Reset wheels
            foreach (var wheel in _driveWheels)
            {
                wheel.motorTorque = 0f;
                wheel.brakeTorque = 0f;
            }

            foreach (var wheel in _steerWheels)
            {
                wheel.steerAngle = 0f;
            }

            // Force rigidbody sleep/wake để ổn định physics
            _rb.Sleep();
            _rb.WakeUp();
        }
    }
}