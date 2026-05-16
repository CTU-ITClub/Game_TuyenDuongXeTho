using UnityEngine;
using Core.Interfaces;
using Game.Features.CameraSystem;
using Photon.Pun;
using VGDSystem.Animation;
using Core.Constains;
namespace Game.Features.Player
{
    [RequireComponent(typeof(CharacterController),typeof(AnimatorHandler))]
    public class PlayerController : MonoBehaviour
    {

        private const float GRAVITY_STICK_FORCE = -2f;
        private const float TERMINAL_VELOCITY = -50f;

        #region SerializeField Property
        [Header("--- Core References ---")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private CameraController _cameraController;
        [SerializeField] private PlayerRigging _playerRig;


        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;

        [Header("Jump & Gravity Settings")]
        [SerializeField] private float _jumpForce = 8f;
        [SerializeField] private float _fallMultiplier = 2.5f;
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField] private float _coyoteTime = 0.15f;
        [SerializeField] private float _jumpBufferTime = 0.1f;

        [Header("Ground Check Settings")]
        [SerializeField] private float _groundCheckRadius = 0.3f;
        [SerializeField] private Vector3 _groundCheckOffset = new Vector3(0, 0.1f, 0);
        [SerializeField] private LayerMask _groundLayer;

        [Header("--- Input Provider (Plug & Play) ---")]
        [Tooltip("Kéo Object chứa script input tùy hệ điều hành")]
        [SerializeField] private GameObject _inputProvider;

        #endregion

        #region Private Property
        private Vector2 _currentInput;
        private Vector3 _currentMovement;
        private float _verticalVelocity;
        private float _coyoteCounter;
        private float _jumpBufferCounter;
        private bool _hasJumped;
        private bool IsGrounded => Physics.CheckSphere(
    transform.position + _groundCheckOffset,
    _groundCheckRadius,
    _groundLayer
);
        #endregion

        #region Components & Interfaces
        private AnimatorHandler _animator;
        private CharacterController _controller;
        private IMovementInput _movementInput;
        private IInteractInput _interactInput;
        private IPlayerState _currentState;
        private IVehicleable _targetVehicleSlot;
        #endregion

        #region Player State Machine
        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerPushState PushState { get; private set; }
        public PlayerSteerState SteerState { get; private set; }

        #endregion

        #region Getter Function
        public bool HasMovementInput() => _currentInput.sqrMagnitude > 0.01f;
        public Vector2 GetMovementInput() => _currentInput;
        public bool ExitPressed() => _interactInput.ExitPressed();
        public IVehicleable CurrentVehicleSlot => _targetVehicleSlot;
        public AnimatorHandler Animator => _animator;
        public PlayerRigging PlayerRig() => _playerRig;
        public IPlayerState CurrentPlayerState => _currentState;
        #endregion

        PhotonView pv;

        private void Awake()
        {
            pv = GetComponent<PhotonView>(); 
            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<AnimatorHandler>();
            if (_inputProvider != null)
            {
                _movementInput = _inputProvider.GetComponent<IMovementInput>();
                _interactInput = _inputProvider.GetComponent<IInteractInput>();
            }

            if (_movementInput == null)
            {
                _movementInput = GetComponentInParent<IMovementInput>();
                if (_movementInput == null)
                    Debug.LogError("[PlayerController] Thiếu IMovementInput! Nhân vật sẽ không thể di chuyển.");
            }

            if (_cameraTransform == null && Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            if (_cameraController == null && _cameraTransform != null)
            {
                _cameraController = _cameraTransform.GetComponent<CameraController>();
            }

            InitializeStates();
        }

        private void Start()
        {
            ChangeState(IdleState);
        }

        //Khởi tạo các state 
        private void InitializeStates()
        {
            IdleState = new PlayerIdleState(this);
            MoveState = new PlayerMoveState(this);
            PushState = new PlayerPushState(this);
            SteerState = new PlayerSteerState(this);
        }

        private void Update()
        {
            if (!pv.IsMine) return;
            if (_movementInput == null) return;

            HandleInput();
            HandleInteraction();

            _currentState?.Update();
        }

        private void FixedUpdate()
        {
            if (!pv.IsMine) return;

            _currentState?.FixedUpdate();
        }

        //hàm nhận nút di chuyển và nhảy
        private void HandleInput()
        {
            _currentInput = _movementInput.GetMovementVector();

            Debug.Log(_currentInput);

            if (_movementInput.JumpKeyPressed())
            {
                _jumpBufferCounter = _jumpBufferTime;
            }
        }

        //hàm tương tác vs xe
        private void HandleInteraction()
        {
            if (_targetVehicleSlot == null) return;

            //khi nhấn E và slot đó chưa có người ngồi thì sẽ chuyển sang trạng thái của slot đó
            if (_interactInput.InteractKeyPressed() && _targetVehicleSlot.CanInteract())
            {
                if (_targetVehicleSlot.GetSlotType() == VehicleSlotType.Push)
                {
                    ChangeState(PushState);
                }
                else
                {
                    ChangeState(SteerState);
                }
            }
        }

        //hàm ngồi lên xe
        public void MountVehicle(IVehicleable slot)
        {
            _targetVehicleSlot = slot;

            slot.OnInteractEnter(this);
        }

        //xuống xe
        public void DismountVehicle()
        {
            if (_targetVehicleSlot != null)
            {
                _targetVehicleSlot.OnInteractExit();
                _targetVehicleSlot = null;
            }
        }

        public void ChangeState(IPlayerState state)
        {
            _currentState?.Exit();
            _currentState = state;
            _currentState?.Enter();
        }

        //Hàm chạy phần di chuyển player và coyote timer lẫn jump
        public void HandleMovementUpdate()
        {
            UpdateTimers();
            CalculateHorizontalMovement();
            CalculateVerticalMovement();
            ApplyFinalMovement();
        }

        // coyote timer và delay khi lần nhảy kế tiếp
        private void UpdateTimers()
        {
            _coyoteCounter -= Time.deltaTime;
            _jumpBufferCounter -= Time.deltaTime;

            // kiểm tra có trên mặt đất hay ko
            bool isActuallyGrounded = IsGrounded || _controller.isGrounded;

            if (isActuallyGrounded && _verticalVelocity <= 0f)
            {
                _coyoteCounter = _coyoteTime;
                _hasJumped = false;

                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = GRAVITY_STICK_FORCE;
                }
            }
        }

        //tính toán phần di chuyển trục x z 
        private void CalculateHorizontalMovement()
        {
            Vector3 inputDirection = new Vector3(_currentInput.x, 0f, _currentInput.y).normalized;

            if (inputDirection.sqrMagnitude > 0.01f)
            {
                if (_cameraTransform != null)
                {
                    Vector3 camForward = _cameraTransform.forward;
                    Vector3 camRight = _cameraTransform.right;

                    camForward.y = 0;
                    camRight.y = 0;

                    camForward.Normalize();
                    camRight.Normalize();

                    Vector3 moveDirection = (camForward * inputDirection.z + camRight * inputDirection.x).normalized;
                    _currentMovement = moveDirection * _moveSpeed;

                    ApplyRotation(moveDirection);
                }
            }
            else
            {
                _currentMovement = Vector3.zero;
                ApplyRotation(Vector3.zero);
            }
        }

        // tính toán phần nhảy trục y
        private void CalculateVerticalMovement()
        {
            bool canJump = _coyoteCounter > 0f && !_hasJumped;
            bool jumpRequested = _jumpBufferCounter > 0f;

            if (canJump && jumpRequested)
            {
                _verticalVelocity = _jumpForce;
                _hasJumped = true;
                _jumpBufferCounter = 0f;
                _coyoteCounter = 0f;
            }

            if (_verticalVelocity < 0)
            {
                _verticalVelocity += _gravity * _fallMultiplier * Time.deltaTime;
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }

            _verticalVelocity = Mathf.Max(_verticalVelocity, TERMINAL_VELOCITY);
        }

        //xử lý vật lý di chuyển từ logic ở trên
        private void ApplyFinalMovement()
        {
            if (!_controller.enabled)
                return;

            Vector3 finalVelocity = _currentMovement;
            finalVelocity.y = _verticalVelocity;
            _controller.Move(finalVelocity * Time.deltaTime);
        }

        //này để khi camera xoay đến đâu thì ng xoay đến đó
        private void ApplyRotation(Vector3 moveDirection)
        {
            if (_cameraController != null && _cameraController.IsFirstPersonView)
            {
                transform.rotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
            }
            else if (moveDirection.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IVehicleable vehicle))
            {
                _targetVehicleSlot = vehicle;
                // hét lên là có player lại gần slot bằng observer, UI và những thag khác tự đăng ký lắng nghe
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IVehicleable vehicle) && vehicle == _targetVehicleSlot)
            {
                _targetVehicleSlot = null;
                // hét lên là có player lại rời xa slot bằng observer, UI và những thag khác tự đăng ký lắng nghe
            }
        }
    }
}