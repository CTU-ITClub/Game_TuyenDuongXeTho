using Core.Constains;
using UnityEngine;

namespace Game.Features.Animal.Wolf
{
    public class WolfAttackState : WolfStateBase
    {
        private float _cooldownTimer;

        public WolfAttackState(WolfController wolf) : base(wolf) { }

        public override void Enter()
        {
            _cooldownTimer = 0f;
            PlayAttack();
        }

        public override void Update()
        {
            // Không còn target hoặc target đã chết
            if (Wolf.Target == null || Wolf.IsTargetDead())
            {
                Wolf.ClearTarget();
                Wolf.ChangeState(Wolf.IdleState);
                return;
            }

            Wolf.FaceDirection(
                Wolf.Target.position - Wolf.transform.position
            );

            // Player không còn trong phạm vi đánh
            if (!Wolf.IsPlayerInAttackRange())
            {
                if (Wolf.IsTargetLost())
                {
                    Wolf.ClearTarget();
                    Wolf.ChangeState(Wolf.IdleState);
                }
                else
                {
                    Wolf.ChangeState(Wolf.ChaseState);
                }

                return;
            }

            _cooldownTimer -= Time.deltaTime;

            if (_cooldownTimer <= 0f)
            {
                PlayAttack();
            }
        }

        protected override void OnUpdate() { }

        private void PlayAttack()
        {
            Wolf.Animator.PlayAnimation(
                GameConstains.WolfRandomAttack
            );

            _cooldownTimer = Wolf.AttackCooldown;
        }
    }
}