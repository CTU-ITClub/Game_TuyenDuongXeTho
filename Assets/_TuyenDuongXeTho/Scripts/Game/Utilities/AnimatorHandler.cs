using UnityEngine;
using Photon.Pun;

namespace VGDSystem.Animation
{
    public class AnimatorHandler : MonoBehaviourPun
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private float _crossFadeDuration = 0.2f;

        private int _currentAnimationHash;

        public void PlayAnimation(int animationHash)
        {
            if (_animator == null) return;
            if (_currentAnimationHash == animationHash) return;

            _currentAnimationHash = animationHash;

            // Kiểm tra trạng thái kết nối Photon trước khi gọi RPC
            if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom && photonView != null)
            {
                photonView.RPC(nameof(RPC_PlayAnimation), RpcTarget.All, animationHash);
            }
            else
            {
                // Nếu chưa vào Room/Offline -> Chạy trực tiếp tại local
                ExecutePlayAnimation(animationHash);
            }
        }

        [PunRPC]
        private void RPC_PlayAnimation(int animationHash)
        {
            ExecutePlayAnimation(animationHash);
        }

        private void ExecutePlayAnimation(int animationHash)
        {
            if (_animator == null) return;
            _animator.CrossFadeInFixedTime(animationHash, _crossFadeDuration);
        }

        public void EnableAnimator()
        {
            if (_animator == null) return;
            _animator.enabled = true;
        }

        public void DisableAnimator()
        {
            if (_animator == null) return;
            _animator.enabled = false;
        }
    }
}