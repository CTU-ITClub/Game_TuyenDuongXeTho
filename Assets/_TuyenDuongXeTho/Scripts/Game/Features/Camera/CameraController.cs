using UnityEngine;
using Core.Interfaces;
using Game.Features.Player;
using Photon.Pun;
using System.Linq;

namespace Game.Features.CameraSystem
{
    public class CameraController : MonoBehaviour
    {
        // Chuyển Enum thành private để đóng gói logic bên trong class này
        private enum CameraState { FirstPerson, ThirdPerson }

        [Header("--- Core References ---")]
        [Tooltip("Transform của Player (Nhân vật chính)")]
        [SerializeField] private Transform _playerTarget;
        [SerializeField] private Transform _playerTarget1;

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

        [Header("Change Cam")]
        public Transform[] targets = new Transform[2];
        bool canChange = false;
        private int currentTargetIndex = 0;
        public GameObject guide;

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
            FindTargets();
            if (_cameraInput == null) return;
            HandleInput();

            ActiveChangeTarget();

            if (Input.GetKeyDown(KeyCode.Tab) && canChange)
            {
                ChangeTarget();
            }
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

        private void FindTargets()
        {
            if (_player != null && _headPosition != null) return;
            // 1. Ưu tiên lấy từ LocalPlayer.TagObject nếu bạn đã gán trước đó
            GameObject localPlayer = PhotonNetwork.LocalPlayer.TagObject as GameObject;

            Transform player = localPlayer != null ? localPlayer.transform : null;
            if (player != null)
            {
                _playerTarget = player;
                _playerTarget1 = player.GetComponentsInChildren<Transform>(true)
                                    .FirstOrDefault(t => t.name == "mixamorig:Hips");
            }

            // 3. Sau khi đã có _playerTarget, tìm vị trí Camera
            if (player != null && _headPosition == null)
            {
                // Tìm trong tất cả các object con ở mọi cấp độ
                Transform found = player.GetComponentsInChildren<Transform>(true)
                                    .FirstOrDefault(t => t.name == "FirstCam");

                if (found != null)
                {
                    _headPosition = found;
                }
            }

            if (_player == null && player != null)
            {
                _player = player.GetComponent<PlayerController>();
            }
        }

        //Change Target
        private void ActiveChangeTarget()
        {
            if (_player == null) return;
            if (canChange) return;

            if (_player.isDead)
            {
                if (guide != null) guide.SetActive(true);

                _playerTarget = _playerTarget1;
                canChange = true;
                FindAllTargets();
            }
        }

        private void FindAllTargets()
        {
            //Tìm kiểm tất cả các object có tag "Player" trong scene
            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
            //Lưu transform của chúng vào mảng targets
            for (int i = 0; i < playerObjects.Length && i < targets.Length; i++)
            {
                targets[i] = playerObjects[i].transform;
            }
            //Đổi target hiện tại thành con của nó theo tên "mixamorig:Hips"
            foreach(Transform target in targets)
            {
                if (target != null)
                {
                    Transform hips = target.GetComponentsInChildren<Transform>(true)
                                    .FirstOrDefault(t => t.name == "mixamorig:Hips");
                    if (hips != null)
                    {
                        _playerTarget = hips;
                        break; 
                    }
                }
            }
        }
        private void ChangeTarget()
        {
            if(targets.Length < 2) return;
            currentTargetIndex = (currentTargetIndex + 1) % targets.Length;

            _playerTarget = targets[currentTargetIndex];
        }
    }
}