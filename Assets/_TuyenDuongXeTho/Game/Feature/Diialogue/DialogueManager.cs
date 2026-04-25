using UnityEngine.UI;
using UnityEngine;
using System.Text; 
using System.Collections;
using System.Collections.Generic;
using TMPro;

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        lines = new Queue<DialogueLine>();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextDialogueLine();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;

        dialoguePanel.SetActive(true);

        lines.Clear();
        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextDialogueLine();
    }

    public void DisplayNextDialogueLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueArea.text = currentFullSentence;
            isTyping = false;
            return;
        }

        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = lines.Dequeue();
        characterIcon.sprite = currentLine.character.icon;
        characterName.text = currentLine.character.name;
        currentFullSentence = currentLine.line;

        StartCoroutine(TypeSentence(currentLine.line));
    }

    IEnumerator TypeSentence(string sentence)
    {
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
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
    }
}