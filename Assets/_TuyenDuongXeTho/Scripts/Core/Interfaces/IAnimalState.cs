using UnityEngine;

namespace Core.Interfaces
{
    public interface IAnimalState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();

    }
}

