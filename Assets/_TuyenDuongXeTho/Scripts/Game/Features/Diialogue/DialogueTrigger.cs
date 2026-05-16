using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.Cinemachine;
using Game.Features.Player;
using VGDSystem.Animation;

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

    public Animator animate;
    public CinemachineCamera cam1;
    public PlayerController playerControl;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue, animate, cam1, playerControl);
    }

    private void OnTriggerEnter(Collider other)
    {
        PhotonView playerPV = other.GetComponent<PhotonView>();

        if (playerPV != null && playerPV.IsMine && other.CompareTag("Player"))
        {
            // Tạm tắt di chuyển
            playerControl = other.GetComponent<PlayerController>();
            playerControl.enabled = false;
            // Idde player
            AnimatorHandler animate = other.GetComponent<AnimatorHandler>();
            if (animate != null) {
                animate.PlayAnimation(Animator.StringToHash("PlayerIdle"));
            }

            TriggerDialogue();
            // Tăng priority của cam1 để nó trở thành camera chính
            cam1.Priority = 50;
            
        }
    }
}