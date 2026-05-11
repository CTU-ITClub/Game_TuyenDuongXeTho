using UnityEngine;
using Core.Interfaces;
using Game.Features.Player;

namespace Game.Features.CameraSystem
{
    public class CameraController : MonoBehaviour
    {
        // Chuyển Enum thành private để đóng gói logic bên trong class này
        private enum CameraState { FirstPerson, ThirdPerson }

        [Header("--- Core References ---")]
        [Tooltip("Transform của Player (Nhân vật chính)")]
        [SerializeField] private Transform _playerTarget;
        [Tooltip("Vị trí đặt Camera khi ở FPS (thường là transform ở vị trí mắt/đầu)")]
        [SerializeField] private Transform _headPosition;
        [SerializeField] private PlayerController _player;
        [Header("--- Rotation Settings ---")]
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private float _minYAngle = -80f;
        [SerializeField] private float _maxYAngle = 80f;

        [Header("--- Third Person Settings (TPS) ---")]
        [SerializeField] private float _tpsDistance = 4f;
        [SerializeField] private Vector3 _tpsOffset = new Vector3(0, 1.5f, 0);
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private float _collisionRadius = 0.2f;

        [Header("--- Smoothing Settings ---")]
        [Tooltip("Độ mượt khi camera lùi ra xa")]
        [SerializeField] private float _distanceSmoothTime = 0.1f;

        [Header("--- Input Provider (Plug & Play) ---")]
        [Tooltip("Kéo Object chứa script Input (PC, Mobile, AI) vào đây. Không cần quan tâm nó là script gì, miễn có ICameraInput là chạy!")]
        [SerializeField] private GameObject _inputProvider;

        // Interface Input
        private ICameraInput _cameraInput;

        // States
        private CameraState _currentState = CameraState.ThirdPerson;
        private float _currentRotX = 0f;
        private float _currentRotY = 0f;
        private float _currentDistance;
        private float _distanceVelocity;

        // Getter public để PlayerController biết mà không làm lộ Enum nội bộ (Encapsulation)
        public bool IsFirstPersonView => _currentState == CameraState.FirstPerson;

        private void Awake()
        {
            // Ưu tiên 1: Lấy Interface từ GameObject được gán ở ngoài Inspector
            if (_inputProvider != null)
            {
                _cameraInput = _inputProvider.GetComponent<ICameraInput>();
            }

            // Ưu tiên 2 (Fallback): Nếu quên không gán _inputProvider, tự động tìm trong chính GameObject này hoặc các object cha
            if (_cameraInput == null)
            {
                _cameraInput = GetComponentInParent<ICameraInput>();
            }

            if (_cameraInput == null)
            {
                Debug.LogError("[CameraController] Cảnh báo: Không tìm thấy bất kỳ Component nào kế thừa ICameraInput! Hãy gán Input Provider.");
            }

            _currentDistance = _tpsDistance;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (_playerTarget != null)
            {
                _currentRotX = _playerTarget.eulerAngles.y;
            }
        }

        private void Update()
        {
            if (_cameraInput == null) return;
            HandleInput();
        }

        private void LateUpdate()
        {
            if (_playerTarget == null) return;

            if (_currentState == CameraState.FirstPerson)
            {
                HandleFirstPersonView();
            }
            else
            {
                HandleThirdPersonView();
            }
        }

        private void HandleInput()
        {
            Vector2 cameraLook = _cameraInput.GetCameraLook();

            _currentRotX += cameraLook.x * _mouseSensitivity;
            _currentRotY -= cameraLook.y * _mouseSensitivity;
            _currentRotY = Mathf.Clamp(_currentRotY, _minYAngle, _maxYAngle);

            if (_cameraInput.SwitchViewKeyPressed())
            {
                ToggleCameraState();
            }
        }

        private void ToggleCameraState()
        {
            _currentState = _currentState == CameraState.FirstPerson
                ? CameraState.ThirdPerson
                : CameraState.FirstPerson;
        }

        private void HandleFirstPersonView()
        {
            if (_headPosition == null) return;
            transform.position = _headPosition.position;
            if (_player.CurrentPlayerState == _player.PushState) return;

            if (_player.CurrentPlayerState == _player.SteerState)
            {
                _currentRotX = Mathf.Clamp(_currentRotX, -70f, 70f);
            }
            transform.rotation = Quaternion.Euler(_currentRotY, _currentRotX, 0);
        }

        private void HandleThirdPersonView()
        {
            Quaternion rotation = Quaternion.Euler(_currentRotY, _currentRotX, 0);
            Vector3 focusPoint = _playerTarget.position + _tpsOffset;
            Vector3 direction = rotation * Vector3.back;

            float targetDistance = _tpsDistance;

            if (Physics.SphereCast(focusPoint, _collisionRadius, direction, out RaycastHit hit, _tpsDistance, _collisionMask))
            {
                targetDistance = hit.distance;
            }

            // Logic tránh Camera xuyên tường hoàn chỉnh
            if (_currentDistance > targetDistance)
            {
                _currentDistance = targetDistance; // Va chạm -> Kéo camera lại gần ngay
            }
            else
            {
                _currentDistance = Mathf.SmoothDamp(_currentDistance, targetDistance, ref _distanceVelocity, _distanceSmoothTime);
            }

            transform.position = focusPoint + (direction * _currentDistance);
            transform.rotation = rotation;
        }
    }
}