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
    private string currentFullSentence = "";    // Biến lưu câu thoại đầy đủ hiện tại 

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
            // Sử dụng phím "Space" để next
            DisplayNextDialogueLine();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;

        dialoguePanel.SetActive(true);

        // Xóa hàng đợi trước khi thêm các dòng thoại mới
        lines.Clear();
        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            // Thêm các dòng thoại từ dialogue vào hàng đợi lines
            lines.Enqueue(dialogueLine);
        }

        // Hiển thị dòng thoại đầu tiên
        DisplayNextDialogueLine();
    }

    public void DisplayNextDialogueLine()
    {
        if (isTyping)
        {
            // Nếu đang dang dở câu thoại trước => bỏ qua hiệu ứng gõ chữ và hiển thị toàn bộ câu thoại ngay lập tức
            StopAllCoroutines();
            dialogueArea.text = currentFullSentence;
            isTyping = false;
            return;
        }

        if (lines.Count == 0)
        {
            // Nếu không còn dòng thoại nào trong hàng đợi => kết thúc cuộc hội thoại
            EndDialogue();
            return;
        }
        // Lấy dòng thoại tiếp theo từ hàng đợi và hiển thị
        DialogueLine currentLine = lines.Dequeue();
        characterIcon.sprite = currentLine.character.icon;
        characterName.text = currentLine.character.name;
        currentFullSentence = currentLine.line;

        StartCoroutine(TypeSentence(currentLine.line));
    }

    IEnumerator TypeSentence(string sentence)
    {
        // Hiển thị từng chữ một với hiệu ứng gõ chữ
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