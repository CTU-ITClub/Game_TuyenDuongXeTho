using UnityEngine;

public class RoomItemButton : MonoBehaviour
{
    public string roomName;
    public int roomIndex;

    public void OnClick()
    {
        RoomList.instance.JoinRoomByName(roomName, roomIndex);
    }
}
