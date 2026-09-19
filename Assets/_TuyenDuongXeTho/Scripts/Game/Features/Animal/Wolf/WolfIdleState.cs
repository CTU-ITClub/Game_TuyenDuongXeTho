using Core.Constains;
using UnityEngine;

namespace Game.Features.Animal.Wolf
{
    public class WolfIdleState : WolfStateBase
    {
        private float _idleTimer;

        public WolfIdleState(WolfController wolf) : base(wolf) { }

        public override void Enter()
        {
            // Idle ngh?a là hi?n t?i không có m?c tiêu
            Wolf.ClearTarget();

            Wolf.Animator.PlayAnimation(
                GameConstains.WolfRandomIdle
            );

            Wolf.HasDestination = false;

            _idleTimer = Random.Range(
                1f,
                Wolf.WanderInterval
            );
        }

        protected override void OnUpdate()
        {
            if (Wolf.TryDetectPlayer(out Transform player))
            {
                Wolf.SetTarget(player);
                Wolf.ChangeState(Wolf.ChaseState);
                return;
            }

            _idleTimer -= Time.deltaTime;
            if (_idleTimer <= 0f)
            {
                Wolf.ChangeState(Wolf.MoveState);
            }
        }
    }
}