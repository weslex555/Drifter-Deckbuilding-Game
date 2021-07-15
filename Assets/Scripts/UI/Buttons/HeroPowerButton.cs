using UnityEngine;
using UnityEngine.EventSystems;

public class HeroPowerButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (pointerEventData.button != PointerEventData.InputButton.Left) return;

        PlayerManager.Instance.UseHeroPower();
    }
}
