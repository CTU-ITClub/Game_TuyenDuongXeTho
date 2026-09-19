using Core.Constains;
using UnityEngine;

namespace Game.Features.Animal.Wolf
{
    public class WolfMoveState : WolfStateBase
    {
        public WolfMoveState(WolfController wolf) : base(wolf) { }

        public override void Enter()
        {
            Wolf.Animator.PlayAnimation(GameConstains.WolfMove);
            SetRandomDestination();
        }

        protected override void OnUpdate()
        {
            if (Wolf.TryDetectPlayer(out Transform player))
            {
                Wolf.SetTarget(player);
                Wolf.ChangeState(Wolf.ChaseState);
                return;
            }

            bool arrived = Wolf.MoveTowards(Wolf.CurrentDestination, Wolf.MoveSpeed);
            if (arrived)
            {
                Wolf.ChangeState(Wolf.IdleState);
            }
        }

        private void SetRandomDestination()
        {
            Vector2 randomCircle = Random.insideUnitCircle * Wolf.WanderRadius;
            Vector3 randomPoint = Wolf.WanderCenter + new Vector3(randomCircle.x, 0f, randomCircle.y);

            Wolf.CurrentDestination = randomPoint;
            Wolf.HasDestination = true;
        }
    }
}