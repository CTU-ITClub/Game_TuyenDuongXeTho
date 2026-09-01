using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }

    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI promptText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Hide();
    }

    public void Show(string text)
    {
        promptText.text = text;
        root.SetActive(true);
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}
