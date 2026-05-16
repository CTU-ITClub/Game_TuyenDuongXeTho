using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game.Features.Player
{
    public class PlayerRigging : MonoBehaviour
    {
        [Header("IK")]
        [SerializeField] private TwoBoneIKConstraint _leftHandIK;
        [SerializeField] private TwoBoneIKConstraint _rightHandIK;

        [SerializeField] private RigBuilder _rigBuilder;

        public void SetHandTargets(Transform left, Transform right)
        {
            var leftData = _leftHandIK.data;
            leftData.target = left;
            _leftHandIK.data = leftData;

            var rightData = _rightHandIK.data;
            rightData.target = right;
            _rightHandIK.data = rightData;

            _leftHandIK.weight = 1f;
            _rightHandIK.weight = 1f;

            _rigBuilder.Build();
        }

        public void ClearHandTargets()
        {
            _leftHandIK.weight = 0f;
            _rightHandIK.weight = 0f;
        }
    }
}