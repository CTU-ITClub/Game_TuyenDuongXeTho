using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetAPI : MonoBehaviour
{
    [SerializeField]
    private string apiUrl;

    public void SendMSSV(string roomName)
    {
        RoomName_Data data = new RoomName_Data
        {
            roomName = roomName
        };

        StartCoroutine(PostData(data));
    }

    IEnumerator PostData(RoomName_Data data)
    {
        string json = JsonUtility.ToJson(data);

        UnityWebRequest request =
            new UnityWebRequest(apiUrl, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        request.uploadHandler =
            new UploadHandlerRaw(bodyRaw);

        request.downloadHandler =
            new DownloadHandlerBuffer();

        request.SetRequestHeader(
            "Content-Type",
            "application/json");

        yield return request.SendWebRequest();

        if (request.result ==
            UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError(request.error);
        }
    }
}