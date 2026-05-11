using UnityEngine;
using Game.Features.Vehicle;

namespace Core.Interfaces
{
    public interface IVehicleable
    {
        VehicleSlotType GetSlotType();
        string GetInteractionPrompt();
        bool CanInteract();
        Transform GetMountPoint(); // Lấy vị trí để ngồi/đứng
        VehicleController GetVehicleController(); // Lấy controller của xe
        void OnInteractEnter(Game.Features.Player.PlayerController player);
        void OnInteractExit();
    }
}