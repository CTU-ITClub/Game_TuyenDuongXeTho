using UnityEngine;
using Core.Interfaces;
using Game.Features.CameraSystem;
using Photon.Pun;
using VGDSystem.Animation;
using Core.Constains;
using TMPro;
using Game.Features.InputSystem;

namespace Game.Features.Player
{
    [RequireComponent(typeof(CharacterController), typeof(AnimatorHandler))]
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
        public PCInputHandler _pcInputHandler;

        [Header("--- Ragdoll Settings ---")]
        [Tooltip("Kéo xương hông (mixamorig:Hips) vào đây để đồng bộ vị trí khi đứng dậy")]
        [SerializeField] private Transform _ragdollHips;

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
        
        #region Ragdoll Private Fields
        private Rigidbody[] _ragdollRigidbodies;
        private Collider[] _ragdollColliders;
        #endregion

        public PhotonView pv;
        public TextMeshProUGUI interactBut, interactBut1;
        public bool CanMove = true;
        bool CountPlayer = false;
        public bool OnVehicle = false;
        public bool isDead = false;

        private void Awake()
        {
            pv = GetComponent<PhotonView>();
            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<AnimatorHandler>();
            if (_inputProvider != null)
            {
                _movementInput = _inputProvider.GetComponent<IMovementInput>();
                _interactInput = _inputProvider.GetComponent<IInteractInput>();
                _pcInputHandler = _inputProvider.GetComponent<PCInputHandler>();
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
            SetupRagdollComponents();
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
            //CounterCheckPlayer();

            if (!pv.IsMine) return;
            if (_movementInput == null) return;

            if (!CanMove) return;

            HandleInput();

            HandleInteraction();

            _currentState?.Update();

            // Left Shift để tăng lực đẩy/kéo
            if (Input.GetKeyDown(KeyCode.LeftShift) && CurrentPlayerState == PushState)
            {
                PushState._vehicle.SetBoost(true);
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                PushState._vehicle.SetBoost(false);
            }

            /*if (Input.GetKey(KeyCode.T))
            {
                StartRagdollWithBomb(bombPosition: transform.position, explosionForce: 15f, explosionRadius: 5f);
            }*/
        }

        private void FixedUpdate()
        {
            //CounterCheckPlayer();

            if (!pv.IsMine) return;

            if (!CanMove) return;

            _currentState?.FixedUpdate();
        }

        // Kiểm tra đủ người join phòng chưa
        private void CounterCheckPlayer()
        {
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playerCount < 2)
            {
                CountPlayer = false;
                CanMove = false;
                ChangeNotice1("Chờ người chơi khác...");
            }
            else
            {
                if (CountPlayer == false)
                {
                    CanMove = true;
                    ChangeNotice1("");
                    CountPlayer = true;
                }
            }
        }

        //hàm nhận nút di chuyển và nhảy
        private void HandleInput()
        {
            _currentInput = _movementInput.GetMovementVector();

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
                OnVehicle = true;

                _pcInputHandler.canPlaySound = false;

                if (_targetVehicleSlot.GetSlotType() == VehicleSlotType.Push)
                {
                    ChangeState(PushState);
                    ChangeNotice1("GIỮ W ĐỂ ĐẨY, S ĐỂ KÉO, LEFT SHIFT TÁC ĐỘNG MẠNH HƠN");
                }
                else
                {
                    ChangeState(SteerState);
                    ChangeNotice1("GIỮ A ĐỂ QUẸO TRÁI, D PHẢI");
                }
            }
        }

        //hàm ngồi lên xe
        public void MountVehicle(IVehicleable slot)
        {
            _targetVehicleSlot = slot;

            slot.OnInteractEnter(this);

            ChangeNotice("NHẤN F ĐỂ THOÁT");
        }

        //xuống xe
        public void DismountVehicle()
        {
            if (_targetVehicleSlot != null)
            {
                _targetVehicleSlot.OnInteractExit();
                _targetVehicleSlot = null;
            }

            ChangeNotice("");
            ChangeNotice1("");

            OnVehicle = false;
            _pcInputHandler.canPlaySound = true;
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

        public void ChangeNotice(string message)
        {
            if (!pv.IsMine) return;

            if (interactBut != null)
                interactBut.text = message;
        }

        public void ChangeNotice1(string message)
        {
            if (!pv.IsMine) return;

            if (interactBut1 != null)
                interactBut1.text = message;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!CanMove) return; // Không tương tác xe khi đang ngã Ragdoll
            if (other.TryGetComponent(out IVehicleable vehicle))
            {
                _targetVehicleSlot = vehicle;
                if (_targetVehicleSlot.CanInteract())
                    ChangeNotice("NHẤN E");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IVehicleable vehicle) && vehicle == _targetVehicleSlot)
            {
                _targetVehicleSlot = null;
                ChangeNotice("");
            }
        }

        #region Ragdoll Pure-Trigger Synchronization With Forces

        private void SetupRagdollComponents()
        {
            Transform mixamoRoot = transform.Find("Root/mixamorig:Hips");
            Transform searchRoot = mixamoRoot != null ? mixamoRoot : transform;

            _ragdollRigidbodies = searchRoot.GetComponentsInChildren<Rigidbody>();
            _ragdollColliders = searchRoot.GetComponentsInChildren<Collider>();

            SetRagdollPhysicsActive(false);
        }

        private void SetRagdollPhysicsActive(bool active)
        {
            if (_ragdollRigidbodies == null) return;

            foreach (var rb in _ragdollRigidbodies)
            {
                rb.isKinematic = !active;
            }

            foreach (var col in _ragdollColliders)
            {
                if (!col.isTrigger)
                    col.enabled = active;
            }
        }

        /// <summary>
        /// Gọi hàm này khi trúng bom, truyền vào vị trí quả bom để tính lực văng
        /// </summary>
        public void StartRagdollWithBomb(Vector3 bombPosition, float explosionForce, float explosionRadius)
        {
            // Gửi RPC kèm theo thông tin vị trí quả bom và sức nổ cho cả phòng
            pv.RPC(nameof(RPC_TriggerDeathRagdollWithForce), RpcTarget.All, bombPosition, explosionForce, explosionRadius);
        }

        [PunRPC]
        private void RPC_TriggerDeathRagdollWithForce(Vector3 bombPos, float force, float radius)
        {
            if (PushState._vehicle != null)
                PushState._vehicle.SetMotorInput(0f); // Dừng xe ngay lập tức nếu đang đẩy

            DismountVehicle();
            CanMove = false;
            isDead = true;
            _controller.enabled = false;
            _animator.DisableAnimator();

            // 1. Kích hoạt vật lý cho xương trước
            SetRagdollPhysicsActive(true);

            // 2. Tác dụng lực nổ vào TẤT CẢ các đốt xương để tăng lực nảy văng ra ngoài
            if (_ragdollRigidbodies != null)
            {
                foreach (var rb in _ragdollRigidbodies)
                {
                    // Tham số thứ 4 (ví dụ: 1f hoặc 2f) giúp hất nhân vật nẩy bổng lên trên trục Y
                    rb.AddExplosionForce(force, bombPos, radius, 3f, ForceMode.Impulse);
                }
            }
        }
        #endregion
    }
}