using UnityEngine;
using Core.Interfaces;
using Game.Features.Player;

namespace Game.Features.Vehicle
{
    public class VehicleSlot : MonoBehaviour, IVehicleable
    {
        [SerializeField] private VehicleController _vehicle;
        [SerializeField] private VehicleSlotType _slotType;
        [SerializeField] private Transform _mountPoint; 
        [SerializeField] private Transform _leftHandTarget;
        [SerializeField] private Transform _rightHandTarget;
        private PlayerController _currentPlayer;

        public VehicleSlotType GetSlotType() => _slotType;
        public string GetInteractionPrompt() => $"Nhấn E để {(_slotType == VehicleSlotType.Push ? "đẩy" : "lái")}";
        public bool CanInteract() => _currentPlayer == null;
        public Transform GetMountPoint() => _mountPoint;
        public VehicleController GetVehicleController() => _vehicle;

        public void OnInteractEnter(PlayerController player)
        {
            if (_currentPlayer != null) return;
            _currentPlayer = player;
            if (_slotType == VehicleSlotType.Push) return;
            _currentPlayer.PlayerRig().SetHandTargets(_leftHandTarget, _rightHandTarget);
        }

        public void OnInteractExit()
        {
            if (_currentPlayer == null) return;
            _currentPlayer.PlayerRig().ClearHandTargets();
            _currentPlayer = null;

        }
    }
}