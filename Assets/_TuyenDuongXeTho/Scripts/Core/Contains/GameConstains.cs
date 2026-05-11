using UnityEngine;
namespace Core.Constains
{
    public static class GameConstains
    {
        #region AnimationHash
        //Player
        public static readonly int PlayerIdle = Animator.StringToHash("PlayerIdle");
        public static readonly int PlayerMove = Animator.StringToHash("PlayerMove");
        public static readonly int PlayerPush = Animator.StringToHash("PlayerPush");
        public static readonly int PlayerSteer = Animator.StringToHash("PlayerSteer");


        #endregion
    }

}
