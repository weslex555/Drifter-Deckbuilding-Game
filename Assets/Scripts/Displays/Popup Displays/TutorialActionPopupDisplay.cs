using TMPro;
using UnityEngine;

public class TutorialActionPopupDisplay : MonoBehaviour
{
    [SerializeField] private GameObject tipText;
    [SerializeField] private GameObject tipCounter;
    [SerializeField] private GameObject nextTipButton;
    [SerializeField] private GameObject previousTipButton;
    [SerializeField] private GameObject continueButton;

    private int currentTip;

    private TextMeshProUGUI tipTextMesh;
    private TextMeshProUGUI tipCountMesh;

    private void Awake()
    {
        tipTextMesh = tipText.GetComponent<TextMeshProUGUI>();
        tipCountMesh = tipCounter.GetComponent<TextMeshProUGUI>();

        currentTip = 1;
        SetCurrentTip();
    }

    public void NextTipButton_OnClick()
    {

        currentTip++;
        SetCurrentTip();
    }

    public void PreviousTipButton_OnClick()
    {
        currentTip--;
        SetCurrentTip();
    }

    public void ContinueButton_OnClick()
    {
        Managers.U_MAN.DestroyTutorialActionPopup();
        Managers.EV_MAN.PauseDelayedActions(false);
    }

    private void SetCurrentTip()
    {
        string text;
        Vector2 position = new();

        bool tipCounter_Active = true;
        bool continueButton_Active = false;
        bool previousButton_Active = true;
        bool nextButton_Active = true;

        switch (currentTip)
        {
            case 1:
                text = $"This is your {TextFilter.Clrz_ylw("Hero")}.";
                position.Set(470, 0);

                previousButton_Active = false;
                break;

            case 2:
                text = $"This is the {TextFilter.Clrz_ylw("Enemy Hero")}.";
                position.Set(-270, 230);
                break;

            case 3:
                text = $"Your hero has a {TextFilter.Clrz_ylw("Health")} value, " +
                    $"an {TextFilter.Clrz_ylw("Energy")} value, a {TextFilter.Clrz_ylw("Power")}, " +
                    $"and an {TextFilter.Clrz_ylw("Ultimate")}. " +
                    "\nHover over each component for an explanation.";
                position.Set(470, 0);
                break;

            case 4:
                text = $"Damage dealt to your hero reduces their {TextFilter.Clrz_ylw("Health")}. If your hero's health reaches 0, you lose.";
                position.Set(470, 0);
                break;

            case 5:
                text = $"{TextFilter.Clrz_ylw("Energy")} is used to play cards and use hero powers. " +
                    "You gain 1 energy at the start of your turn, increased by 1 each turn to a maximum of 10.";
                position.Set(470, 0);
                break;

            case 6:
                text = $"You can use your {TextFilter.Clrz_ylw("Hero Power")} once each turn. " +
                    $"Each time you use your hero power 3+ times, you can use your {TextFilter.Clrz_ylw("Hero Ultimate")} once.";
                position.Set(470, 0);
                break;

            case 7:
                text = $"You draw cards from your {TextFilter.Clrz_ylw("Deck")}. When it runs out of cards, " +
                    "you'll shuffle your discard pile back into it.";
                position.Set(-500, -150);
                break;

            case 8:
                text = $"You draw {GameManager.START_HAND_SIZE} cards at the beginning of each combat, and 1 each turn after that.";
                position.Set(-570, -190);
                break;

            case 9:
                text = "After you draw your first hand, you can replace any number of cards with new ones from your deck. " +
                    $"This is known as a {TextFilter.Clrz_ylw("Mulligan")}.";
                position.Set(-570, -190);
                break;

            case 10: // Last Case
                text = "Now you know the basics. Time to test your knowledge.";

                tipCounter_Active = false;
                continueButton_Active = true;
                nextButton_Active = false;
                break;

            case 11: // Catch Case
                currentTip = 10;
                return;

            default:
                Debug.LogError("INVALID TIP NUMBER!");
                return;
        }

        tipCounter.SetActive(tipCounter_Active);
        continueButton.SetActive(continueButton_Active);
        previousTipButton.SetActive(previousButton_Active);
        nextTipButton.SetActive(nextButton_Active);
        DisplayCurrentTip(text, position);
    }

    private void DisplayCurrentTip(string text, Vector2 position)
    {
        tipCountMesh.SetText(currentTip + "/10");
        tipTextMesh.SetText(text);
        transform.position = position;
    }
}
