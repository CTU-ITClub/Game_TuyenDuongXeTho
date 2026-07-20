using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomList : MonoBehaviourPunCallbacks
{
    public static RoomList instance;

    public GameObject roomMana;
    public RoomManager roomManager;

    [Header("UI")]
    public Transform contain;
    public GameObject roomListPrefab;
    public GameObject fullPlayerNotice;
    public GameObject connectNotice;
    public TextMeshProUGUI ccuCount;
    public TextMeshProUGUI serverName;
    public string[] serverListName;

    public List<RoomInfo> list = new List<RoomInfo>();
    private Dictionary<string, GameObject> roomObjects =
    new Dictionary<string, GameObject>();

    [Header("Create Room")]
    private string roomName = "Map1";
    private string roomMapName = "Map1";
    public int maxPlayerSet = 0;

    public bool cursorLocked = false;

    public override void OnEnable()
    {
        base.OnEnable();   // QUAN TRỌNG

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        maxPlayerSet = 2;
        instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;

        if (fullPlayerNotice != null)
            fullPlayerNotice.SetActive(false);

        if (connectNotice != null)
            connectNotice.SetActive(false);
    }

    IEnumerator Start()
    {
        // 1. Nếu vô tình vẫn đang kẹt trong phòng, thì thoát ra trước
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            yield return new WaitUntil(() => !PhotonNetwork.InRoom);
        }

        // 2. KHÔNG ngắt kết nối (Disconnect). Chỉ kết nối nếu thực sự chưa kết nối.
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            // 3. Nếu đã kết nối sẵn (do đi từ Scene Game về Menu), 
            // thì chỉ việc xin vào lại Lobby để lấy danh sách phòng.
            if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ChangeCursorState();
        }

        if (ccuCount != null)
        {
            ccuCount.text = PhotonNetwork.CountOfPlayers + "/20";
        }

        CheckConnection();
    }

    public void ChangeCursorState()
    {
        cursorLocked = !cursorLocked;
        if (cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public override void OnConnectedToMaster()
    {
        // Check server name
        for (int i = 0; i < serverListName.Length; i++)
        {
            if (PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime == serverListName[i])
            {
                if (i == 0) serverName.text = "Server: Siu";
                else if (i == 1) serverName.text = "Server: Siuuuuu";
            }
        }

        // Nếu CountOfPlayers > 20, nghĩa là họ là người thứ 21 trở đi.
        if (PhotonNetwork.CountOfPlayers > 20)
        {
            Debug.LogWarning("Server đã đầy (Đạt giới hạn 20 người). Đang ngắt kết nối...");

            if (fullPlayerNotice != null)
            {
                fullPlayerNotice.SetActive(true);
            }

            // Chủ động ngắt kết nối client này ra để không chiếm slot ngầm và không ảnh hưởng người khác
            PhotonNetwork.Disconnect();
            return;
        }

        // Nếu hợp lệ (< 20 người), tiếp tục chạy bình thường
        base.OnConnectedToMaster();
        Debug.Log("Connected to Server");

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected: " + cause);

        // Trường hợp bị từ chối từ phía Server Photon do đầy gói CCU của AppID
        if (cause == DisconnectCause.MaxCcuReached)
        {
            Debug.Log("Server Full (Photon Dashboard Limit)!");
            if (fullPlayerNotice != null)
            {
                fullPlayerNotice.SetActive(true);
            }
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                RemoveRoom(room);
            }
            else
            {
                AddOrUpdateRoom(room);
            }
        }
    }

    void AddOrUpdateRoom(RoomInfo room)
    {
        GameObject roomObj;

        if (roomObjects.TryGetValue(room.Name, out roomObj))
        {
            UpdateRoomObject(roomObj, room);
            return;
        }

        roomObj = Instantiate(roomListPrefab, contain);

        roomObjects.Add(room.Name, roomObj);

        UpdateRoomObject(roomObj, room);
    }

    void RemoveRoom(RoomInfo room)
    {
        if (roomObjects.TryGetValue(room.Name, out GameObject roomObj))
        {
            Destroy(roomObj);

            roomObjects.Remove(room.Name);
        }
    }

    void UpdateRoomObject(GameObject roomObj, RoomInfo room)
    {
        RoomItemButton roomItemBtn =
            roomObj.GetComponent<RoomItemButton>();

        roomItemBtn.maxPlayer = room.MaxPlayers;
        roomItemBtn.currentPlayer = room.PlayerCount;
        roomItemBtn.roomName = room.Name;

        string mapKey = " ";
        if (room.CustomProperties.TryGetValue("mapKey", out object keyObj))
        {
            mapKey = (string)keyObj;
        }
        roomItemBtn.mapKey = mapKey;

        string roomMapName = "";
        if (room.CustomProperties.TryGetValue("roomMapName", out object mapObj))
            roomMapName = mapObj.ToString();

        string roomName = "";
        if (room.CustomProperties.TryGetValue("mapName", out object nameObj))
            roomName = nameObj.ToString();

        var textName = roomObj.transform.GetChild(0)
            .GetComponent<TextMeshProUGUI>();

        var textPlayers = roomObj.transform.GetChild(1)
            .GetComponent<TextMeshProUGUI>();

        textName.text = roomName + " (" + roomMapName + ")";
        textPlayers.text = room.PlayerCount + "/" + room.MaxPlayers;

        bool isStartRoom = false;

        if (room.CustomProperties.TryGetValue("isStartRoom", out object value))
        {
            isStartRoom = (bool)value;
        }

        roomItemBtn.isFulled = isStartRoom;

        if ((roomItemBtn.currentPlayer >= roomItemBtn.maxPlayer && roomItemBtn != null) || (roomItemBtn != null && roomItemBtn.isFulled))
        {
            roomItemBtn.mapKeyNotice2.SetActive(true);
            roomItemBtn.isFulled = true;
        }
    }

    public void ChangeRoomToCreate(string name)
    {
        roomName = name;
    }

    public void JoinRoomByName(string name)
    {
        Debug.Log("Đang kết nối vào phòng...");
        PhotonNetwork.JoinRoom(name);
    }

    //CreatRoom sẽ gọi hàm này để tạo phòng với map index tương ứng
    public void JoinButtonPressed(int mapIndex)
    {
        if (maxPlayerSet == 0)
        {
            Debug.LogError("LỖI: Vui lòng chọn số người chơi trước khi tạo phòng!");
            return;
        }

        Debug.Log("Creating Room...");

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = maxPlayerSet;

        if (mapIndex == 1) roomMapName = "Map1";
        else if (mapIndex == 2) roomMapName = "Map2";
        else if (mapIndex == 3) roomMapName = "Map3";

        string randomKey = RandomMapKey();

        options.CustomRoomProperties = new Hashtable()
        {
            {"mapSceneIndex", mapIndex},
            {"mapName", roomName},
            {"mapKey", randomKey},
            {"roomMapName", roomMapName},
            {"isStartRoom", false }
        };

        options.CustomRoomPropertiesForLobby = new[]
        {
            "mapSceneIndex",
            "mapName",
            "mapKey",
            "roomMapName",
            "isStartRoom"
        };

        PhotonNetwork.CreateRoom(roomName, options);
        ClockCursor();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created Room SUCCESS");

        int mapIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["mapSceneIndex"];

        PhotonNetwork.LoadLevel(mapIndex);
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient) return; //tránh host load 2 lần

        Debug.Log("Joined Room (Client)");

        int mapIndex = (int)PhotonNetwork.CurrentRoom.CustomProperties["mapSceneIndex"];
    }

    string RandomMapKey()
    {
        int length = 6;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] result = new char[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
        }

        return new string(result);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangeMaxPlayer(string index)
    {
        // kiểm tra index phải là 2, 3 hoặc 4
        if (index != "2" && index != "3" && index != "4")
        {
            Debug.LogError("LỖI: Số người chơi phải là 2, 3 hoặc 4!");
            maxPlayerSet = 0;
            return;
        }
        maxPlayerSet = int.Parse(index);
    }

    public void ClockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    // Refresh Room List
    public void RefreshRoomList()
    {
        StartCoroutine(RefreshLobbyCoroutine());
    }

    IEnumerator RefreshLobbyCoroutine()
    {
        PhotonNetwork.LeaveLobby();

        yield return new WaitUntil(() => !PhotonNetwork.InLobby);

        foreach (var roomObj in roomObjects.Values)
        {
            Destroy(roomObj);
        }

        roomObjects.Clear();

        PhotonNetwork.JoinLobby();
    }

    //Check Connection
    private void CheckConnection()
    {
        if (!PhotonNetwork.IsConnected)
        {
            connectNotice.SetActive(true);
        }
        else
        {
            connectNotice.SetActive(false);
        }
    }
}
