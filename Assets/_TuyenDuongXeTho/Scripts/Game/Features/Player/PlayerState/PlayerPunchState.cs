using Core.Constains;
using Core.Interfaces;
using Game.Features.Player;
using UnityEngine;

public class PlayerPunchState : IPlayerState
{
    private PlayerController _player;
    private float _timer;

    private const float PunchDuration = 0.8f;

    public PlayerPunchState(PlayerController player)
    {
        _player = player;
    }

    public void Enter()
    {
        _timer = 0f;

        _player.Animator.PlayAnimation(
            GameConstains.PlayerPunch
        );
    }

    public void Update()
    {
        _timer += Time.deltaTime;

        if (_player.OnVehicle &&
            _player.CurrentVehicleSlot != null)
        {
            // Có input thì quay lại state điều khiển xe
            if (_player.HasMovementInput())
            {
                if (_player.CurrentVehicleSlot.GetSlotType()
                    == VehicleSlotType.Push)
                {
                    _player.ChangeState(_player.PushState);
                }
                else
                {
                    _player.ChangeState(_player.SteerState);
                }

                return;
            }
        }

        else if (_player.HasMovementInput())
        {
            _player.ChangeState(_player.MoveState);
            return;
        }

        if (_timer >= PunchDuration)
        {
            ReturnToNormalState();
        }
    }

    public void FixedUpdate()
    {
        _player.HandleMovementUpdate();
    }

    public void Exit()
    {

    }


    private void ReturnToNormalState()
    {
        if (_player.OnVehicle &&
            _player.CurrentVehicleSlot != null)
        {
            if (_player.CurrentVehicleSlot.GetSlotType()
                == VehicleSlotType.Push)
            {
                _player.ChangeState(_player.PushState);
            }
            else
            {
                _player.ChangeState(_player.SteerState);
            }

            return;
        }


        if (_player.HasMovementInput())
        {
            _player.ChangeState(_player.MoveState);
        }
        else
        {
            _player.ChangeState(_player.IdleState);
        }
    }
}