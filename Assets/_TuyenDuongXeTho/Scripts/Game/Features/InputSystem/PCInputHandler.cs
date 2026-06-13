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


        [Header("Sound Settings")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _walkSound;
        public bool canPlaySound = true;

        void Update()
        {
            if (!canPlaySound && _audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }

        // IMovementInput Interface
        public Vector2 GetMovementVector()
        {
            if (_audioSource != null && _walkSound != null && canPlaySound)
            {
                // Debug bool canPlaySound để kiểm tra xem có được phép phát âm thanh hay không
                Debug.Log("canPlaySound: " + canPlaySound);
                if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
                {
                    if (!_audioSource.isPlaying)
                    {
                        _audioSource.clip = _walkSound;
                        _audioSource.Play();
                    }
                }
                else
                {
                    if (_audioSource.isPlaying)
                    {
                        _audioSource.Stop();
                    }
                }
            }

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            return new Vector2(horizontal, vertical);
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

        public void ChangeInvertYAxis(bool value)
        {
            _invertYAxis = value;
        }

    }
}