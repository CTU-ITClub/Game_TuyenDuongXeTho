using Core.Interfaces;
using UnityEngine;
using VGDSystem.Animation;
using Game.Features.Player;

namespace Game.Features.Animal.Wolf
{
    public class WolfController : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private AnimatorHandler _animator;

        [Header("Wander")]
        [SerializeField] private float _wanderRadius = 8f;
        [SerializeField] private float _wanderInterval = 3f;
        [SerializeField] private float _moveSpeed = 2f;

        [Header("Detect")]
        [SerializeField] private float _detectRadius = 8f;
        [SerializeField] private LayerMask _playerLayer;

        [Header("Chase")]
        [SerializeField] private float _chaseSpeed = 4f;
        [SerializeField] private float _loseTargetBuffer = 1.2f;

        [Header("Attack")]
        [SerializeField] private float _attackRadius = 2f;
        [SerializeField] private float _attackCooldown = 1.5f;

        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 10f;

        public AnimatorHandler Animator => _animator;
        public Transform Target { get; private set; }
        public Vector3 CurrentDestination { get; set; }
        public bool HasDestination { get; set; }

        public Vector3 WanderCenter { get; private set; }
        public float WanderRadius => _wanderRadius;
        public float WanderInterval => _wanderInterval;
        public float MoveSpeed => _moveSpeed;
        public float DetectRadius => _detectRadius;
        public float ChaseSpeed => _chaseSpeed;
        public float AttackRadius => _attackRadius;
        public float AttackCooldown => _attackCooldown;
        public float RotationSpeed => _rotationSpeed;

        public IAnimalState IdleState { get; private set; }
        public IAnimalState MoveState { get; private set; }
        public IAnimalState ChaseState { get; private set; }
        public IAnimalState AttackState { get; private set; }

        private IAnimalState _currentState;

        private void Awake()
        {
            WanderCenter = transform.position;

            IdleState = new WolfIdleState(this);
            MoveState = new WolfMoveState(this);
            ChaseState = new WolfChaseState(this);
            AttackState = new WolfAttackState(this);
        }

        private void Start()
        {
            ChangeState(IdleState);
        }

        private void Update()
        {
            // Đang có mục tiêu nhưng mục tiêu đã chết
            if (Target != null && IsTargetDead())
            {
                ResetTargetAndReturnToIdle();
                return;
            }

            _currentState?.Update();
        }

        private void FixedUpdate() => _currentState?.FixedUpdate();

        public void ChangeState(IAnimalState newState)
        {
            if (_currentState == newState) return;
            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
        }

        public bool TryDetectPlayer(out Transform player)
        {
            Collider[] hits = Physics.OverlapSphere(
                transform.position,
                _detectRadius,
                _playerLayer
            );

            PlayerController nearestPlayer = null;
            float nearestDistanceSqr = float.MaxValue;

            foreach (Collider hit in hits)
            {
                PlayerController playerController =
                    hit.GetComponentInParent<PlayerController>();

                // Không phải player hoặc player đã chết
                if (playerController == null || playerController.isDead)
                    continue;

                float distanceSqr =
                    (playerController.transform.position - transform.position)
                    .sqrMagnitude;

                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestPlayer = playerController;
                }
            }

            if (nearestPlayer != null)
            {
                player = nearestPlayer.transform;
                return true;
            }

            player = null;
            return false;
        }

        public bool IsPlayerInAttackRange()
        {
            if (Target == null) return false;
            return Vector3.Distance(transform.position, Target.position) <= _attackRadius;
        }

        public bool IsTargetLost()
        {
            if (Target == null)
                return true;

            if (IsTargetDead())
                return true;

            float distance =
                Vector3.Distance(transform.position, Target.position);

            return distance > _detectRadius * _loseTargetBuffer;
        }

        public bool IsTargetDead()
        {
            if (Target == null)
                return true;

            PlayerController playerController =
                Target.GetComponent<PlayerController>();

            if (playerController == null)
            {
                playerController =
                    Target.GetComponentInParent<PlayerController>();
            }

            return playerController == null || playerController.isDead;
        }

        private void ResetTargetAndReturnToIdle()
        {
            ClearTarget();

            HasDestination = false;
            CurrentDestination = transform.position;

            ChangeState(IdleState);
        }

        public void SetTarget(Transform target) => Target = target;
        public void ClearTarget() => Target = null;

        // Di chuyển tới 1 điểm, trả về true nếu đã tới nơi
        public bool MoveTowards(Vector3 destination, float speed)
        {
            Vector3 flatDest = new Vector3(destination.x, transform.position.y, destination.z);
            transform.position = Vector3.MoveTowards(transform.position, flatDest, speed * Time.deltaTime);
            FaceDirection(flatDest - transform.position);

            return Vector3.Distance(transform.position, flatDest) <= 0.15f;
        }

        public void FaceDirection(Vector3 dir)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.001f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _detectRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _attackRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(WanderCenter, _wanderRadius);
        }
    }
}