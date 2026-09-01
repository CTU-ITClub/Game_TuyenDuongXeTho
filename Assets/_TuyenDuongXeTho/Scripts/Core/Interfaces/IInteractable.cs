using UnityEngine;

/// <summary>
/// Bất kỳ object nào muốn tương tác được bằng phím E (item, cửa, NPC, rương...)
/// đều implement interface này. Giúp PlayerInteractor không cần biết cụ thể
/// nó đang tương tác với loại object gì.
/// </summary>
public interface IInteractable
{
    string InteractionPrompt { get; }
    void Interact(GameObject interactor);
}
