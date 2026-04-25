using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[System.Serializable]
public class DialogueCharacter
{
    public string name;
    public Sprite icon;
}

[System.Serializable]
public class DialogueLine
{
    public DialogueCharacter character;
    [TextArea(3, 10)]
    public string line;
}

[System.Serializable]
public class Dialogue
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue);
    }

    public void OnTriggerEnter(Collider other)
    {
        // Kiểm tra xem đối tượng chạm vào NPC có phải là Player "CỦA MÌNH" không
        PhotonView playerPV = other.GetComponent<PhotonView>();

        if (playerPV != null && playerPV.IsMine && other.CompareTag("Player"))
        {
            TriggerDialogue();
        }
    }
}