using UnityEngine;
using Photon.Pun;

namespace VGDSystem.Animation
{
    public class AnimatorHandler : MonoBehaviourPun
    {
        [SerializeField] private Animator _animator;

        [SerializeField]
        private float _crossFadeDuration = 0.2f;

        private int _currentAnimationHash;

        public void PlayAnimation(int animationHash)
        {
            if (_currentAnimationHash == animationHash)
                return;

            _currentAnimationHash = animationHash;

            photonView.RPC(
                nameof(RPC_PlayAnimation),
                RpcTarget.All,
                animationHash
            );
        }

        [PunRPC]
        private void RPC_PlayAnimation(int animationHash)
        {
            _animator.CrossFadeInFixedTime(
                animationHash,
                _crossFadeDuration
            );
        }
    }
}