using Photon.Pun;
using UnityEngine;

namespace VGDSystem.Animation
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(Animator))]
    public class AnimatorHandler : MonoBehaviourPun
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private float _crossFadeDuration = 0.2f;

        private int _currentAnimationHash;

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();
        }

        public void PlayAnimation(
            int animationHash,
            bool forceRestart = false
        )
        {
            if (!forceRestart &&
                _currentAnimationHash == animationHash)
            {
                return;
            }

            _currentAnimationHash = animationHash;

            // Khi đang ở trong phòng Photon và object có ViewID
            if (PhotonNetwork.InRoom &&
                photonView != null &&
                photonView.ViewID != 0)
            {
                photonView.RPC(
                    nameof(RPC_PlayAnimation),
                    RpcTarget.All,
                    animationHash
                );
            }
            else
            {
                // Cho phép test offline trong Unity
                ApplyAnimation(animationHash);
            }
        }

        [PunRPC]
        private void RPC_PlayAnimation(int animationHash)
        {
            _currentAnimationHash = animationHash;
            ApplyAnimation(animationHash);
        }

        private void ApplyAnimation(int animationHash)
        {
            if (_animator == null)
            {
                return;
            }

            if (!_animator.HasState(0, animationHash))
            {
                return;
            }

            _animator.CrossFadeInFixedTime(
                animationHash,
                _crossFadeDuration,
                0,
                0f
            );
        }

        public void EnableAnimator()
        {
            if (_animator != null)
                _animator.enabled = true;
        }

        public void DisableAnimator()
        {
            if (_animator != null)
                _animator.enabled = false;
        }
    }
}