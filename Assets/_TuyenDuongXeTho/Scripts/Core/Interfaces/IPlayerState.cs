using UnityEngine;

namespace Core.Interfaces
{
    public interface IPlayerState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();

    }
}

