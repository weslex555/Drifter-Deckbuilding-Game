using UnityEngine;

public class DialogueResponseButton : MonoBehaviour
{
    [SerializeField] private int response;
    public void OnClick()
    {
        if (CardManager.Instance.NewCardPopup != null) return; // Unnecessary
        DialogueManager.Instance.DialogueResponse(response);
        GetComponentInParent<SoundPlayer>().PlaySound(0);
    }
}
