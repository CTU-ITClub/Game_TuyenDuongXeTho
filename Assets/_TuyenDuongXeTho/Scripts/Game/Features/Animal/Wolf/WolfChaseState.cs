using Core.Constains;

namespace Game.Features.Animal.Wolf
{
    public class WolfChaseState : WolfStateBase
    {
        public WolfChaseState(WolfController wolf) : base(wolf) { }

        public override void Enter()
        {
            Wolf.Animator.PlayAnimation(GameConstains.WolfChase);
        }

        protected override void OnUpdate()
        {
            if (Wolf.IsTargetLost())
            {
                Wolf.ClearTarget();
                Wolf.ChangeState(Wolf.IdleState);
                return;
            }

            Wolf.MoveTowards(Wolf.Target.position, Wolf.ChaseSpeed);
        }
    }
}