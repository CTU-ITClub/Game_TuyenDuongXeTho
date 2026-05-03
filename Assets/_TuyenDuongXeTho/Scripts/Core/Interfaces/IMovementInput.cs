using UnityEngine;

namespace Core.Interfaces
{
    public interface IMovementInput
    {
        Vector2 GetMovementVector();
        bool JumpKeyPressed();
    }

}