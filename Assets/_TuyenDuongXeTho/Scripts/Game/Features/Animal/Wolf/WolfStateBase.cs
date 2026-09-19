using Core.Interfaces;

namespace Game.Features.Animal.Wolf
{
    public abstract class WolfStateBase : IAnimalState
    {
        protected readonly WolfController Wolf;

        protected WolfStateBase(WolfController wolf)
        {
            Wolf = wolf;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void FixedUpdate() { }

        // Update là "template method": check rule toàn cục trước, rồi mới chạy logic riêng của state con
        public virtual void Update()
        {
            if (Wolf.IsPlayerInAttackRange())
            {
                Wolf.ChangeState(Wolf.AttackState);
                return;
            }

            OnUpdate();
        }

        protected abstract void OnUpdate();
    }
}