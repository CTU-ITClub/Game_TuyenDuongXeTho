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
        public static readonly int PlayerPunch = Animator.StringToHash("PlayerPunch");

        public static readonly int WolfChase = Animator.StringToHash("WolfChase");
        public static readonly int WolfMove = Animator.StringToHash("WolfMove");
        public static readonly int WolfRandomAttack = Animator.StringToHash("WolfRandomAttack");
        public static readonly int WolfRandomIdle = Animator.StringToHash("WolfRandomIdle");


        #endregion
    }

}
