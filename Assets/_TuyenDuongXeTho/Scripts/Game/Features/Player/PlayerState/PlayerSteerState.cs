using Core.Interfaces;
using Game.Features.Player;
using Game.Features.Vehicle;
using UnityEngine;
using Core.Constains;

public class PlayerSteerState : IPlayerState
{
    private PlayerController _player;
    private VehicleController _vehicle;

    public PlayerSteerState(PlayerController player)
    {
        _player = player;
    }

    public void Enter()
    {
        var slot = _player.CurrentVehicleSlot;
        _vehicle = slot.GetVehicleController();

        _player.MountVehicle(slot);
        _player.Animator.PlayAnimation(GameConstains.PlayerSteer);

    }

    public void Update()
    {
        if (_vehicle == null) return;

        Vector2 input = _player.GetMovementInput();

        _vehicle.SetSteerInput(input.x);

        if (_player.ExitPressed())
        {
            _player.ChangeState(_player.IdleState);
        }
    }

    public void FixedUpdate() { }

    public void Exit()
    {
        if (_vehicle != null)
            _vehicle.SetSteerInput(0f);

        _player.DismountVehicle();
    }
}