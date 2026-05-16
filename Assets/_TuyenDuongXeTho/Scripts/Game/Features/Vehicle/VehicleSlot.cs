using UnityEngine;
using Photon.Pun;
using Core.Interfaces;
using Game.Features.Player;

namespace Game.Features.Vehicle
{
    [RequireComponent(typeof(PhotonView))]
    public class VehicleSlot : MonoBehaviourPunCallbacks, IVehicleable
    {
        [Header("Vehicle")]
        [SerializeField] private VehicleController _vehicle;
        [SerializeField] private VehicleSlotType _slotType;

        [Header("Seat")]
        [SerializeField] private Transform _mountPoint;

        [Header("Hand Targets")]
        [SerializeField] private Transform _leftHandTarget;
        [SerializeField] private Transform _rightHandTarget;

        private PlayerController _currentPlayer;

        public VehicleSlotType GetSlotType() => _slotType;

        public string GetInteractionPrompt()
        {
            return $"Nhấn E để {(_slotType == VehicleSlotType.Push ? "đẩy" : "lái")}";
        }

        public bool CanInteract() => _currentPlayer == null;

        public Transform GetMountPoint() => _mountPoint;

        public VehicleController GetVehicleController() => _vehicle;

        // =====================================================
        // ENTER
        // =====================================================

        public void OnInteractEnter(PlayerController player)
        {
            if (_currentPlayer != null)
                return;

            PhotonView playerPV = player.GetComponent<PhotonView>();

            if (playerPV == null)
                return;

            photonView.RPC(
                nameof(RPC_HandlePlayerEnter),
                RpcTarget.AllBuffered,
                playerPV.ViewID
            );
        }

        // =====================================================
        // EXIT
        // =====================================================

        public void OnInteractExit()
        {
            if (_currentPlayer == null)
                return;

            photonView.RPC(
                nameof(RPC_HandlePlayerExit),
                RpcTarget.AllBuffered
            );
        }

        // =====================================================
        // RPC ENTER
        // =====================================================

        [PunRPC]
        private void RPC_HandlePlayerEnter(int playerViewID)
        {
            PhotonView targetPV = PhotonView.Find(playerViewID);

            if (targetPV == null)
                return;

            PlayerController player =
                targetPV.GetComponent<PlayerController>();

            if (player == null)
                return;

            _currentPlayer = player;

            // =============================
            // CHARACTER CONTROLLER
            // =============================

            CharacterController cc =
                player.GetComponent<CharacterController>();

            if (cc != null)
                cc.enabled = false;

            // =============================
            // CAPSULE COLLIDER
            // =============================

            CapsuleCollider capsule =
                player.GetComponent<CapsuleCollider>();

            if (capsule != null)
                capsule.isTrigger = true;

            // =============================
            // RIGIDBODY
            // =============================

            Rigidbody rb =
                player.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            // =============================
            // PHOTON TRANSFORM VIEW
            // =============================

            PhotonTransformView ptv =
                player.GetComponent<PhotonTransformView>();

            if (ptv != null)
                ptv.enabled = false;

            // =============================
            // SNAP VÀO GHẾ
            // =============================

            player.transform.SetParent(_mountPoint);

            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;

            // =============================
            // IK HAND
            // =============================

            if (_slotType != VehicleSlotType.Push)
            {
                player.PlayerRig().SetHandTargets(
                    _leftHandTarget,
                    _rightHandTarget
                );
            }
        }

        // =====================================================
        // RPC EXIT
        // =====================================================

        [PunRPC]
        private void RPC_HandlePlayerExit()
        {
            if (_currentPlayer == null)
                return;

            PlayerController player = _currentPlayer;

            // =============================
            // BỎ PARENT
            // =============================

            player.transform.SetParent(null);

            // =============================
            // ĐẨY PLAYER RA NGOÀI XE
            // =============================

            player.transform.position =
                _mountPoint.position + (_mountPoint.right * 1.5f);

            // =============================
            // CHARACTER CONTROLLER
            // =============================

            CharacterController cc =
                player.GetComponent<CharacterController>();

            if (cc != null)
                cc.enabled = true;

            // =============================
            // CAPSULE COLLIDER
            // =============================

            CapsuleCollider capsule =
                player.GetComponent<CapsuleCollider>();

            if (capsule != null)
                capsule.isTrigger = false;

            // =============================
            // RIGIDBODY
            // =============================

            Rigidbody rb =
                player.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.detectCollisions = true;
                rb.isKinematic = false;
            }

            // =============================
            // PHOTON TRANSFORM VIEW
            // =============================

            PhotonTransformView ptv =
                player.GetComponent<PhotonTransformView>();

            if (ptv != null)
                ptv.enabled = true;

            // =============================
            // CLEAR IK
            // =============================

            player.PlayerRig().ClearHandTargets();

            _currentPlayer = null;
        }
    }
}