using UnityEngine;
using TMPro;

public class CloneUnitPopupDisplay : MonoBehaviour
{
    [SerializeField] private GameObject popupText;

    private UIManager uMan;
    private PlayerManager pMan;
    private GameManager gMan;
    private UnitCard unitCard;

    private string PopupText
    {
        set
        {
            popupText.GetComponent<TextMeshProUGUI>().SetText(value);
        }
    }

    private void Awake()
    {
        uMan = UIManager.Instance;
        pMan = PlayerManager.Instance;
        gMan = GameManager.Instance;
    }

    public UnitCard UnitCard
    {
        set
        {
            int aether = pMan.AetherCells;
            unitCard = value;
            string text = "Clone <u>" + unitCard.CardName +
                "</u> for " + gMan.GetCloneCost(unitCard) +
                " aether? (You have " + aether + " aether)";
            PopupText = text;
        }
    }

    public void ConfirmButton_OnClick()
    {
        CardManager.Instance.AddCard(unitCard, GameManager.PLAYER, true);
        pMan.AetherCells -= gMan.GetCloneCost(unitCard);
        uMan.CreateCardPagePopup(CardPageDisplay.CardPageType.CloneUnit, false);
    }

    public void CancelButton_OnClick() =>
        uMan.DestroyInteractablePopup(gameObject);
}
