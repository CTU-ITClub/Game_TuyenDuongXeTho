using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoomList : MonoBehaviourPunCallbacks
{
    public static RoomList instance;

    public GameObject roomMana;
    public RoomManager roomManager;

    [Header("UI")]
    public Transform contain;
    public GameObject roomListPrefab;

    public List<RoomInfo> list = new List<RoomInfo>();

    private string map = "Map1";
    void Awake()
    {
        instance = this;
    }

    IEnumerator Start()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }

        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected to Sever");
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            int index = list.FindIndex(x => x.Name == room.Name);

            if (room.RemovedFromList)
            {
                if (index != -1)
                    list.RemoveAt(index);
            }
            else
            {
                if (index != -1)
                {
                    list[index] = room; // update
                }
                else
                {
                    list.Add(room); // ? add room m?i
                }
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        foreach (Transform child in contain)
        {
            Destroy(child.gameObject);
        }
        foreach (var room in list)
        {
            GameObject _room = Instantiate(roomListPrefab, contain);

            string roomMapName = " ";
            object nameMapObj;

            if (room.CustomProperties.TryGetValue("mapName", out nameMapObj))
            {
                roomMapName = (string)nameMapObj;
            }

            _room.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = room.Name + "(" + roomMapName + ")";
            _room.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = room.PlayerCount + "/" + room.MaxPlayers;

            _room.GetComponent<RoomItemButton>().roomName = room.Name;

            int mapIndex = 0;

            object mapIndexObj;
            if(room.CustomProperties.TryGetValue("mapSceneIndex", out mapIndexObj))
            {
                mapIndex = (int)mapIndexObj;
            }


            _room.GetComponent<RoomItemButton>().roomIndex = mapIndex;

            Button btn = _room.GetComponent<Button>();

            if (room.PlayerCount >= room.MaxPlayers)
            {
                btn.interactable = false;
                _room.GetComponent<Image>().color = Color.gray;
            }
        }
    }

    public void ChangeRoomToCreate(string name)
    {
        RoomManager.instance.roomName = name;
    }

    public void JoinRoomByName(string name, int mapIndex)
    {
        gameObject.SetActive(false);
        PhotonNetwork.JoinRoom(name);
        SceneManager.LoadScene(mapIndex);
    }

    public void CreateRoomByIndex(int index)
    {
        JoinRoomByName(map , index);
    }
}
