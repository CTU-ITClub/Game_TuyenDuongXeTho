using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.Cinemachine;
using System.Collections;
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
    bool isFirstTime = true;
    public bool isReach = false;
    public bool isTalking = false;
    public string notice = "PRESS E TO TALK";

    //NPC cuối 
    public bool isLastNPC = false;
    public GoalManager goalManager;
    public CinemachineBrain brain;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isFirstTime && !isTalking && isReach)
        {
            playerControl.ChangeNotice("");
            isTalking = true;
            StartConversation(playerControl);
        }
    }

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue, animate, cam1, playerControl, this);
    }

    private void StartConversation(PlayerController other)
    {
        // Tạm tắt di chuyển
        playerControl.enabled = false;
        // Idde player
        AnimatorHandler animate = other.GetComponent<AnimatorHandler>();
        if (animate != null)
        {
            animate.PlayAnimation(Animator.StringToHash("PlayerIdle"));
        }

        TriggerDialogue();
        // Tăng priority của cam1 để nó trở thành camera chính
        cam1.Priority = 50;

        // Khi nào cam được chuyển về cam1 thì gọi hàm success
        StartCoroutine(WaitForCameraBlendComplete());

        // Tắt sound của player do còn input khi di chuyển
        playerControl._pcInputHandler.canPlaySound = false;
    }

    private IEnumerator WaitForCameraBlendComplete()
    {
        // Đợi cho tới khi cam1 trở thành camera active
        yield return new WaitUntil(() =>
            brain != null &&
            brain.ActiveVirtualCamera == cam1);

        // Nếu đang blend thì đợi blend kết thúc
        yield return new WaitUntil(() =>
            brain != null &&
            !brain.IsBlending);

        if (isLastNPC && goalManager != null)
        {
            goalManager.Success();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PhotonView playerPV = other.GetComponent<PhotonView>();

        if ((playerPV != null && playerPV.IsMine && other.CompareTag("Player")) || (!playerPV.IsMine && isLastNPC && other.CompareTag("Player")))
        {
            playerControl = other.GetComponent<PlayerController>();

            if (playerPV != null && !playerPV.IsMine)
            {
                PlayerController[] player = FindObjectsOfType<PlayerController>();

                foreach (PlayerController p in player)
                {
                    if (p.pv.IsMine)
                    {
                        playerControl = p;
                        break;
                    }
                }
            }

            if (playerControl == null || playerControl.OnVehicle)
            {
                return;
            }

            isReach = true;

            if (isFirstTime)
            {
                isFirstTime = false;
                isTalking = true;

                StartConversation(playerControl);
            }
            else
            {
                playerControl.ChangeNotice(notice);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PhotonView playerPV = other.GetComponent<PhotonView>();

        if (playerPV != null && playerPV.IsMine && other.CompareTag("Player"))
        {
            isReach = false;
            playerControl.ChangeNotice("");
        }
    }
}