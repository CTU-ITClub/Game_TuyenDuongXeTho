using UnityEngine;
using Core.Interfaces;

namespace Game.Features.InputSystem
{
    /// <summary>
    /// Script nh?n di?n Input s? d?ng Legacy Input Manager m?c ??nh c?a Unity.
    /// G?n script này vào cùng object v?i Player ho?c m?t object qu?n lý Input riêng.
    /// </summary>
    public class CameraInput : MonoBehaviour
    {
        [Header("--- Phím Tùy Ch?nh (Key Bindings) ---")]
        [SerializeField] private KeyCode _jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode _switchViewKey = KeyCode.V;

        [Header("--- Tùy Ch?nh Camera ---")]
        [Tooltip("Có mu?n ??o ng??c tr?c Y (chu?t lên thì nhìn xu?ng) không?")]
        [SerializeField] private bool _invertYAxis = false;

        // ==========================================
        // TH?C THI INTERFACE IInputReader
        // ==========================================

        public Vector2 GetInputVector()
        {
            // Dùng GetAxisRaw ?? nhân v?t d?ng l?i/??i h??ng ngay l?p t?c (không có quán tính c?a Input)
            // Quán tính di chuy?n nên ???c x? lý b?ng code v?t lý thay vì code input.
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            return new Vector2(horizontal, vertical).normalized;
        }

        public bool JumpKeyPressed()
        {
            return Input.GetKeyDown(_jumpKey);
        }

        public Vector2 GetCameraInput()
        {
            // GetAxis th??ng (không Raw) ?? m??t mà h?n cho chu?t
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (_invertYAxis)
            {
                mouseY = -mouseY;
            }

            return new Vector2(mouseX, mouseY);
        }

        public bool SwitchViewKeyPressed()
        {
            return Input.GetKeyDown(_switchViewKey);
        }
    }
}