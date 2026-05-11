using UnityEngine;
using Core.Interfaces;

namespace Game.Features.InputSystem
{
    public class PCInputHandler : MonoBehaviour, IMovementInput, ICameraInput, IInteractInput
    {
        [Header("--- PC Key Bindings ---")]
        [SerializeField] private KeyCode _jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode _switchViewKey = KeyCode.V;
        [SerializeField] private KeyCode _interactKey = KeyCode.E;
        [SerializeField] private KeyCode _exitKey = KeyCode.F;

        [Header("--- PC Mouse Settings ---")]
        [Tooltip("Đảo ngược trục Y của chuột")]
        [SerializeField] private bool _invertYAxis = false;

        // IMovementInput Interface
        public Vector2 GetMovementVector()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector2(horizontal, vertical).normalized;
        }

        public bool JumpKeyPressed() => Input.GetKeyDown(_jumpKey);

        // ICameraInput Interface
        public Vector2 GetCameraLook()
        {
            // Dùng GetAxis cho chuột để lấy delta mượt mà
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (_invertYAxis) mouseY = -mouseY;

            return new Vector2(mouseX, mouseY);
        }

        public bool SwitchViewKeyPressed() => Input.GetKeyDown(_switchViewKey);

        // IInteractInput Interface
        public bool InteractKeyPressed() => Input.GetKeyDown(_interactKey);
        public bool ExitPressed() => Input.GetKeyDown(_exitKey);

    }
}