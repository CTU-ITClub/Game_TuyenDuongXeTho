using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;

    public GameObject player;

    [Space]
    public Transform spawnPoint;

    [Header("UI")]
    public GameObject joinButton;

    public string roomName = "Room1";
    public string mapName = "UnNamed";

    void Awake()
    {
        instance = this;
    }

    public void JoinButtonPressed(int mapIndex)
    {
        Debug.Log("Connecting to Server...");

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2; //set max player ở đây

        options.CustomRoomProperties = new Hashtable()
        {
            {"mapSceneIndex", mapIndex},
            {"mapName", mapName}
        };

        options.CustomRoomPropertiesForLobby = new[]
        {
            "mapSceneIndex",
            "mapName"
        };


        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);

        SceneManager.LoadScene(mapIndex);
        joinButton.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined Room");
        //Spawn
        GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);
        _player.GetComponent<PlayerSetUp>().IsLocalPlayer();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Join failed: " + message);
    }

    public void ChangeMapName(string name)
    {
        mapName = name;
    }
}
