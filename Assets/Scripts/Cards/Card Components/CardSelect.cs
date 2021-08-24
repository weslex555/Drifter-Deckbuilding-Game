using UnityEngine;
using UnityEngine.EventSystems;

public class CardSelect : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject cardOutline;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color highlightedColor;
    public GameObject CardOutline { get => cardOutline; }
    public Color SelectedColor { get => selectedColor; }
    public Color HighlightedColor { get => highlightedColor; }
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button != PointerEventData.InputButton.Left) return;
        if (UIManager.Instance.PlayerIsTargetting)
            EffectManager.Instance.SelectTarget(gameObject);
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (!DragDrop.ArrowIsDragging) return;
        DragDrop.Enemy = gameObject;
        if (CardManager.Instance.CanAttack(DragDrop.DraggingCard, gameObject, true))
            UIManager.Instance.SelectEnemy(gameObject, true, true);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (!DragDrop.ArrowIsDragging) return;
        DragDrop.Enemy = null;
        if (CardManager.Instance.CanAttack(DragDrop.DraggingCard, gameObject, true))
            UIManager.Instance.SelectEnemy(gameObject, true);
    }
}
