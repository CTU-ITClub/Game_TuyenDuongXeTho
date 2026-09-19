using UnityEngine;
using Photon.Pun;

public enum NPCState
{
    Injured,
    Move,
    End
}

public class State_NPC_Injured : MonoBehaviour
{
    public NPCState currentState = NPCState.Injured;
    public PhotonView photonView;
    public Object_moving object_Moving;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        object_Moving = GetComponent<Object_moving>();
    }

    public void RequestGetMedicalAid()
    {
        Debug.Log("[State_NPC_Injured] Yêu cầu cứu NPC bị thương từ client.");

        photonView.RPC(
            nameof(RPC_ReceiveMedicalAid),
            RpcTarget.MasterClient
        );
    }

    void ChangeState(NPCState newState)
    {
        if (newState == NPCState.Move)
        {
            currentState = newState;

            Debug.Log("[State_NPC_Injured] Chuyển sang trạng thái Move.");
            // Change Animator to Move state

            // Play ActiveMove của Object_moving
            if (object_Moving != null)
            {
                object_Moving.enabled = true;
                object_Moving.ActiveMove();
            }
        }
        else if (newState == NPCState.End)
        {
            currentState = newState;

            Debug.Log("[State_NPC_Injured] Chuyển sang trạng thái End.");
            // Change Animator to End state

            // Play End logic
        }
    }

    [PunRPC]
    void RPC_ReceiveMedicalAid()
    {
        // Chỉ MasterClient xử lý yêu cầu cứu NPC.
        if (!PhotonNetwork.IsMasterClient)
            return;

        // Tránh cứu nhiều lần khiến ActiveMove bị gọi lại.
        if (currentState != NPCState.Injured)
            return;

        photonView.RPC(
            nameof(RPC_ChangeState),
            RpcTarget.All,
            (int)NPCState.Move
        );
    }

    [PunRPC]
    void RPC_ChangeState(int newState)
    {
        // Tất cả máy đều cập nhật trạng thái.
        ChangeState((NPCState)newState);
    }
}
