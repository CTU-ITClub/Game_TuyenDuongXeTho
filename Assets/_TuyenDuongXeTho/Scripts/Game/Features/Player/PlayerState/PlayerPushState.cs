using Core.Interfaces;
using Game.Features.Player;
using Game.Features.Vehicle;
using UnityEngine;
using Core.Constains;

public class PlayerPushState : IPlayerState
{
    private PlayerController _player;
    public VehicleController _vehicle;

    public PlayerPushState(PlayerController player)
    {
        _player = player;
    }

    public void Enter()
    {
        Debug.Log("ENTER PUSH");

        var slot = _player.CurrentVehicleSlot;
        _vehicle = slot.GetVehicleController();

        _player.MountVehicle(slot);
        _player.Animator.PlayAnimation(GameConstains.PlayerPush);
    }

    public void Update()
    {
        if (_player.ExitPressed())
        {
            _player.ChangeState(_player.IdleState);
        }
    }

    public void FixedUpdate()
    {
        Debug.Log("PUSH FIXED UPDATE");

        if (_vehicle == null) return;

        Vector2 input = _player.GetMovementInput();

        Debug.Log(input);

        float push = Mathf.Clamp(input.y, -1f, 1f);

        _vehicle.SetMotorInput(push);
    }

    public void Exit()
    {
        if (_vehicle != null)
            _vehicle.SetMotorInput(0f);

        _player.DismountVehicle();
    }
}