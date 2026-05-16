using Core.Interfaces;
using Game.Features.Player;
using UnityEngine;
using Core.Constains;

public class PlayerMoveState : IPlayerState
{
    private PlayerController _player;

    public PlayerMoveState(PlayerController player)
    {
        _player = player;
    }

    public void Enter()
    {
        _player.Animator.PlayAnimation(GameConstains.PlayerMove);
    }

    public void Update()
    {
        // về idle
        if (!_player.HasMovementInput())
        {
            _player.ChangeState(_player.IdleState);
            return;
        }
    }

    public void FixedUpdate()
    {
        _player.HandleMovementUpdate();
    }
    public void Exit() { }
}