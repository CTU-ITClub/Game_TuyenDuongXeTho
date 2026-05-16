using Core.Interfaces;
using Core.Constains;
using Game.Features.Player;
using UnityEngine;

public class PlayerIdleState : IPlayerState
{
    private PlayerController _player;

    public PlayerIdleState(PlayerController player)
    {
        _player = player;
    }

    public void Enter()
    {
        _player.Animator.PlayAnimation(GameConstains.PlayerIdle);
    }

    public void Update()
    {
        // chuyển sang move
        if (_player.HasMovementInput())
        {
            _player.ChangeState(_player.MoveState);
        }
    }

    public void FixedUpdate()
    {
        _player.HandleMovementUpdate();
    }
    public void Exit() { }
}