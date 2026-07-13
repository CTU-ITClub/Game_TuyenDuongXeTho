using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("DOTween Settings")]
    [SerializeField] private CustomTweenPro _dtweenStart;

    void Start()
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Chạy hiệu ứng (ví dụ: phóng to)
        _dtweenStart.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _dtweenStart.Rewind();
    }
}