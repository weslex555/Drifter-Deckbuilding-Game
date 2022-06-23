using UnityEngine;
using TMPro;

public class NewCardPopupDisplay : MonoBehaviour
{
    [SerializeField] private GameObject popupTitle;
    [SerializeField] private GameObject newCardZone;
    [SerializeField] private GameObject newCardChest;
    [SerializeField] private GameObject[] addCardButtons;
    [SerializeField] private GameObject ignoreCardButton;
    
    private CombatManager coMan;
    private PlayerManager pMan;
    private DialogueManager dMan;
    private UIManager uMan;
    private AnimationManager anMan;
    private CardManager caMan;

    private Card newCard;
    private Card[] chooseCards;

    public string PopupTitle
    {
        set
        {
            popupTitle.GetComponent<TextMeshProUGUI>().text = value;
        }
    }
    public Card NewCard
    {
        get => newCard;
        set
        {
            newCard = value;
            DisplayNewCardChest();
        }
    }

    public Card[] ChooseCards
    {
        set
        {
            chooseCards = value;
            DisplayNewCardChest();
        }
    }

    private void Awake()
    {
        coMan = CombatManager.Instance;
        pMan = PlayerManager.Instance;
        dMan = DialogueManager.Instance;
        uMan = UIManager.Instance;
        anMan = AnimationManager.Instance;
        caMan = CardManager.Instance;

        ignoreCardButton.GetComponentInChildren<TextMeshProUGUI>().SetText
            ("Take " + GameManager.IGNORE_CARD_AETHER + " aether instead");
        GetComponent<SoundPlayer>().PlaySound(0);
    }

    private void DisplayNewCardChest()
    {
        newCardChest.SetActive(true);
        foreach (GameObject button in addCardButtons) button.SetActive(false);
        ignoreCardButton.SetActive(false);
        anMan.CreateParticleSystem(newCardChest, ParticleSystemHandler.ParticlesType.NewCard, 5);
    }

    private void SwitchToCards()
    {
        GetComponent<SoundPlayer>().PlaySound(1);
        newCardChest.SetActive(false);
        foreach (GameObject button in addCardButtons) button.SetActive(true);
        ignoreCardButton.SetActive(true);
        anMan.CreateParticleSystem(null, ParticleSystemHandler.ParticlesType.ButtonPress, 1);
    }

    public void DisplayNewCard()
    {
        SwitchToCards();
        // Card Popup
        GameObject newCard = coMan.ShowCard(NewCard, new Vector2(), CombatManager.DisplayType.NewCard);
        if (newCard == null)
        {
            Debug.LogError("CARD IS NULL!");
            return;
        }
        CardZoom cz = newCard.GetComponent<CardZoom>();
        newCard.transform.SetParent(newCardZone.transform, false);
        // Description Popup
        cz.CreateDescriptionPopup(new Vector2(-550, 0), 3);
        CardZoom.DescriptionPopup.transform.SetParent(newCardZone.transform, true);
        // Ability Popups
        cz.CreateAbilityPopups(new Vector2(550, 0), 3, false);
        CardZoom.AbilityPopupBox.transform.SetParent(newCardZone.transform, true);
        cz.enabled = false; // Disable more info tooltip
        // Card Popup
        newCard.transform.SetAsLastSibling();
        // Prevent DestroyZoomObjects() on ZoomAbilityIcon
        CardZoom.CurrentZoomCard = null;
        CardZoom.DescriptionPopup = null;
        CardZoom.AbilityPopupBox = null;
    }

    public void DisplayChooseCards()
    {
        SwitchToCards();
        foreach (Card card in chooseCards)
        {
            // Card Popup
            GameObject newCard = coMan.ShowCard(card, new Vector2(),
                CombatManager.DisplayType.ChooseCard); // TESTING
            CardDisplay cd = newCard.GetComponent<CardDisplay>();
            newCard.transform.SetParent(newCardZone.transform, false);
            cd.DisableVisuals();
            newCard.transform.localScale = new Vector2(3, 3);
        }

        foreach (GameObject button in addCardButtons)
        {
            button.transform.localPosition =
                new Vector2(button.transform.localPosition.x, -400); // TESTING
        }
    }

    public void AddCard_OnClick(int cardSelection)
    {
        GetComponent<SoundPlayer>().PlaySound(2);
        uMan.DestroyNewCardPopup();

        Card newCard;
        if (cardSelection == 0) newCard = NewCard;
        else newCard = chooseCards[cardSelection - 1];
        CardManager.Instance.AddCard(newCard, GameManager.PLAYER, true);

        DialogueClip nextClip = dMan.EngagedHero.NextDialogueClip;
        if (!SceneLoader.IsActiveScene(SceneLoader.Scene.CombatScene))
        {
            DialoguePrompt dp = nextClip as DialoguePrompt;
            if (dp.AetherCells > 0)
            {
                int newAether = dp.AetherCells;
                int newTotal = newAether + pMan.AetherCells;
                uMan.CreateAetherCellPopup(newAether, newTotal);
            }
            else dMan.DisplayDialoguePopup();
        }
        else if (nextClip is CombatRewardClip crc)
        {
            if (newCard is UnitCard)
            {
                uMan.CreateNewCardPopup(null, "New Action!",
                    caMan.ChooseCards(CardManager.ChooseCard.Action));
                return;
            }
            else if (newCard is SkillCard) { } // TESTING
            else if (newCard is ActionCard) // TESTING
            {
                if (crc.NewSkill)
                {
                    uMan.CreateNewCardPopup(null, "New Hero Skill!",
                        caMan.ChooseCards(CardManager.ChooseCard.Skill)); // TESTING
                    return;
                }
            }

            if (crc.AetherCells > 0)
            {
                int newAether = crc.AetherCells;
                int newTotal = newAether + pMan.AetherCells;
                uMan.CreateAetherCellPopup(newAether, newTotal);
            }
            else
            {
                dMan.EngagedHero.NextDialogueClip = crc.NextDialogueClip;
                SceneLoader.LoadScene(SceneLoader.Scene.WorldMapScene);
            }
        }
        else Debug.LogError("NEXT CLIP IS NOT COMBAT_REWARD_CLIP!");
    }

    public void IgnoreCard_OnClick()
    {
        GetComponent<SoundPlayer>().PlaySound(3);
        pMan.AetherCells += GameManager.IGNORE_CARD_AETHER;
        uMan.DestroyNewCardPopup();

        DialogueClip nextClip = dMan.EngagedHero.NextDialogueClip;
        if (!SceneLoader.IsActiveScene(SceneLoader.Scene.CombatScene))
        {
            DialoguePrompt dp = nextClip as DialoguePrompt;
            if (dp.AetherCells > 0)
            {
                int newAether = dp.AetherCells;
                int newTotal = newAether + pMan.AetherCells;
                uMan.CreateAetherCellPopup(newAether, newTotal);
            }
            else dMan.DisplayDialoguePopup();
        }
        else if (dMan.EngagedHero.NextDialogueClip is CombatRewardClip crc)
        {
            if (crc.AetherCells > 0)
            {
                int newAether = crc.AetherCells;
                int newTotal = newAether + pMan.AetherCells;
                uMan.CreateAetherCellPopup(newAether, newTotal);
            }
            else
            {
                dMan.EngagedHero.NextDialogueClip = crc.NextDialogueClip;
                SceneLoader.LoadScene(SceneLoader.Scene.DialogueScene);
            }
        }
        else Debug.LogError("NEXT CLIP IS NOT COMBAT_REWARD_CLIP!");
    }
}
