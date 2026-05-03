using UnityEngine;

namespace Core.Interfaces
{
    public interface ICameraInput
    {
        Vector2 GetCameraLook();
        bool SwitchViewKeyPressed();
    }

}