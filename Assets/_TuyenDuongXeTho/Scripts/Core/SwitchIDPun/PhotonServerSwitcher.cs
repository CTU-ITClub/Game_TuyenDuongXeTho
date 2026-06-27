using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonServerSwitcher : MonoBehaviourPunCallbacks
{
    public static PhotonServerSwitcher Instance;

    private bool reconnectAfterDisconnect;

    public RoomList roomList;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ConnectToServer(string appId)
    {
        if (roomList != null)
            roomList.RefreshRoomList();

        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = appId;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "asia";

        reconnectAfterDisconnect = true;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        //LoadScene
        SceneManager.LoadScene("Menu");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (reconnectAfterDisconnect)
        {
            reconnectAfterDisconnect = false;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
}