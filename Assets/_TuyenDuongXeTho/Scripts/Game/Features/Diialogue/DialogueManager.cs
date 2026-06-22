using UnityEngine.UI;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Game.Features.Player;
using UnityEngine.Splines;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Image characterIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;

    [Header("Settings")]
    public float typingSpeed = 0.05f;

    private Queue<DialogueLine> lines;

    public bool isDialogueActive = false;

    private bool isTyping = false;
    private string currentFullSentence = "";

    private Animator currentAnimator;
    private CinemachineCamera currentCam;
    public PlayerController playerControl;
    private DialogueTrigger currentTrigger;

    public AudioSource audioSource;
    public AudioSource audioSourceMusic;
    public AudioClip finalMusic;

    public CinemachineCamera lastCam;
    public GameObject chimLac, scroll_Scene;
    public GameObject[] objs;

    [Header("Exit feature")]
    bool canExit = false;
    bool isLockChimLac = false;
    public RoomManager roomMana;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        lines = new Queue<DialogueLine>();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.N))
        {
            DisplayNextDialogueLine();
        }

        if (Input.GetKeyDown(KeyCode.X) && canExit)
        {
            roomMana.LeaveRoom();
        }
    }

    public void StartDialogue(Dialogue dialogue, Animator animate, CinemachineCamera cam, PlayerController player, DialogueTrigger diaTrigger)
    {
        isDialogueActive = true;

        dialoguePanel.SetActive(true);

        currentAnimator = animate;
        currentCam = cam;
        playerControl = player;
        currentTrigger = diaTrigger;

        if (currentAnimator != null)
        {
            currentAnimator.SetBool("Talk", true);
            currentAnimator.Play("Talking", 0, 0f);
        }

        lines.Clear();

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextDialogueLine();
    }

    public void DisplayNextDialogueLine()
    {
        // RESET TALK ANIMATION
        if (currentAnimator != null)
        {
            currentAnimator.Play("Talking", 0, 0f);
        }

        // Nếu đang typing thì hiện full text
        if (isTyping)
        {
            // Tắt tiêng typing
            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.time = 0f;
            }

            StopAllCoroutines();

            dialogueArea.text = currentFullSentence;

            isTyping = false;
            return;
        }

        // Hết hội thoại
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = lines.Dequeue();

        if (characterIcon != null)
            characterIcon.sprite = currentLine.character.icon;

        characterName.text = currentLine.character.name;

        currentFullSentence = currentLine.line;

        StartCoroutine(TypeSentence(currentLine.line));
    }

    IEnumerator TypeSentence(string sentence)
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }

        isTyping = true;

        dialogueArea.text = "";

        StringBuilder sb = new StringBuilder();

        foreach (char letter in sentence.ToCharArray())
        {
            sb.Append(letter);

            dialogueArea.text = sb.ToString();

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void EndDialogue()
    {
        if (currentTrigger.isLastNPC)
        {
            ChangeFinalCam();
            return;
        }

        isDialogueActive = false;

        dialoguePanel.SetActive(false);

        if (currentAnimator != null)
        {
            currentAnimator.SetBool("Talk", false);
        }
        // Reset priority của cam1 để nó không còn là camera chính nữa
        if (currentCam != null)
        {
            currentCam.Priority = 0;
            currentCam = null;
        }

        if (playerControl != null)
        {
            playerControl.enabled = true;
            playerControl = null;
        }
        currentTrigger.isTalking = false;
        currentTrigger.playerControl.ChangeNotice(currentTrigger.notice);
        currentTrigger.playerControl._pcInputHandler.canPlaySound = true;
    }

    private void ChangeFinalCam()
    {
        if (audioSourceMusic != null)
        {
            audioSource.Stop();
            audioSourceMusic.clip = finalMusic;
            audioSourceMusic.Play();
        }

        //Tắt UI
        isDialogueActive = false;

        dialoguePanel.SetActive(false);

        if (currentCam != null)
        {
            currentCam.Priority = 0;
            currentCam = null;
        }
        //Chuyển sang cam cuối
        if (lastCam != null)
        {
            lastCam.Priority = 50;
        }
        //Sau 30s chuyển scene cuối
        StartCoroutine(LoadFinalScene());
    }

    IEnumerator WaitBirdFinish()
    {
        SplineAnimate chimLacAnimate =
            chimLac.GetComponent<SplineAnimate>();

        while (chimLacAnimate.NormalizedTime < 0.998f)
        {
            yield return null;
        }

        isLockChimLac = true;

        chimLacAnimate.enabled = false;
    }

    private IEnumerator LoadFinalScene()
    {
        yield return new WaitForSeconds(3f);
        if (lastCam != null)
        {
            GameObject cam = lastCam.gameObject;
            Animator camAnim = cam.GetComponent<Animator>();
            camAnim.enabled = true;

            foreach (GameObject obj in objs)
            {
                obj.SetActive(false);
            }
        }

        yield return new WaitForSeconds(20.2f);
        if (chimLac != null)
        {
            chimLac.SetActive(true);
        }

        StartCoroutine(WaitBirdFinish());

        yield return new WaitForSeconds(36.8f);
        if (scroll_Scene != null)
        {
            scroll_Scene.SetActive(true);
            canExit = true;

            Animator sc_animate = scroll_Scene.GetComponent<Animator>();
            if (sc_animate != null)
            {
                sc_animate.enabled = true;
            }

        }
    }
}