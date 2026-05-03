using UnityEngine;
namespace VGDSystem.Animation
{
    public class AnimatorHandler : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        [Tooltip("thời gian chuyển giữa các animator")]
        [SerializeField] private float _crossFadeDuration = 0.2f;

        private int _currentAnimationHash;

        public void PlayAnimation(int animationHash)
        {
            if (_currentAnimationHash == animationHash) return;

            _currentAnimationHash = animationHash;
            _animator.CrossFadeInFixedTime(animationHash, _crossFadeDuration);
        }

        // mở rộng nếu skill đặc biệt có delay khác nhau
        public void PlayAnimation(int animationHash, float customBlendDuration)
        {
            if (_currentAnimationHash == animationHash) return;

            _currentAnimationHash = animationHash;
            _animator.CrossFadeInFixedTime(animationHash, customBlendDuration);
        }
    }

}
