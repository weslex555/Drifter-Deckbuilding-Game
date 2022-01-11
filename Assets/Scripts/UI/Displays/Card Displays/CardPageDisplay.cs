using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardPageDisplay : MonoBehaviour
{
    [SerializeField] private GameObject learnSkillButtonPrefab;
    [SerializeField] private GameObject recruitUnitButtonPrefab;
    [SerializeField] private GameObject removeCardButtonPrefab;
    [SerializeField] private GameObject cloneUnitButtonPrefab;
    [SerializeField] private GameObject pageCounter;
    [SerializeField] private GameObject cardGroup;
    [SerializeField] private GameObject costGroup;
    [SerializeField] private GameObject pageTitle;
    [SerializeField] private GameObject noCardsTooltip;

    private PlayerManager pMan;
    private UIManager uMan;
    private List<Card> cardGroupList;
    private List<GameObject> activeCards;
    private CardPageType cardPageType;
    private int currentPage;
    private int totalPages;

    private string PageCounterText
    {
        set
        {
            pageCounter.GetComponent<TextMeshProUGUI>().SetText(value);
        }
    }

    public enum CardPageType
    {
        LearnSkill,
        RemoveCard,
        RecruitUnit,
        CloneUnit,
    }

    public void DisplayCardPage(CardPageType cardPageType)
    {
        pMan = PlayerManager.Instance;
        uMan = UIManager.Instance;
        this.cardPageType = cardPageType;
        cardGroupList = new List<Card>();
        string titleText;

        switch (cardPageType)
        {
            case CardPageType.LearnSkill:
                titleText = "Learn a Skill";
                foreach (Card c in pMan.PlayerHero.HeroMoreSkills)
                    cardGroupList.Add(c);
                // Also include starting skills
                foreach (Card c in pMan.PlayerHero.HeroStartSkills)
                    cardGroupList.Add(c);
                break;
            case CardPageType.RemoveCard:
                titleText = "Remove a Card";
            foreach (Card c in pMan.PlayerDeckList)
                cardGroupList.Add(c);
                break;
            case CardPageType.RecruitUnit:
                titleText = "Recruit a Unit";
                foreach (Card c in CardManager.Instance.PlayerRecruitUnits)
                    cardGroupList.Add(c);
                break;
            case CardPageType.CloneUnit: // TESTING
                titleText = "Clone a Unit";
                foreach (Card c in pMan.PlayerDeckList)
                    if (c is UnitCard)
                        cardGroupList.Add(c);
                break;
            default:
                Debug.LogError("INVALID TYPE!");
                return;
        }

        pageTitle.GetComponent<TextMeshProUGUI>().SetText(titleText);
        activeCards = new List<GameObject>();
        currentPage = 1;
        if (cardGroupList.Count > 0)
        {
            cardGroupList.Sort((x, y) => string.Compare(x.CardName, y.CardName)); // TESTING
            cardGroupList.Sort((s1, s2) => s1.StartEnergyCost - s2.StartEnergyCost); // TESTING

            noCardsTooltip.SetActive(false);
            double result = cardGroupList.Count / 4.0;
            totalPages = (int)Math.Ceiling(result);
        }
        else
        {
            noCardsTooltip.SetActive(true);
            totalPages = 1;
        }
        LoadCardPage();
    }

    private void LoadCardPage()
    {
        PageCounterText = currentPage + " / " + totalPages;
        int firstIndex = (currentPage - 1) * 4;
        int index;
        foreach (GameObject go in activeCards) Destroy(go);
        activeCards.Clear();
        for (int i = 0; i < 4; i++)
        {
            index = firstIndex + i;
            if (index > cardGroupList.Count - 1) break;
            Card card = cardGroupList[firstIndex + i];
            GameObject cardObj =
                CombatManager.Instance.ShowCard(card, new Vector2(), CombatManager.DisplayType.Cardpage);
            cardObj.transform.SetParent(cardGroup.transform);

            CardDisplay cd = cardObj.GetComponent<CardDisplay>();
            cd.DisableVisuals();
            cardObj.transform.localScale = new Vector2(4, 4);

            activeCards.Add(cardObj);
            GameObject buttonPrefab;

            switch(cardPageType)
            {
                case CardPageType.LearnSkill:
                    buttonPrefab = learnSkillButtonPrefab;
                    break;
                case CardPageType.RemoveCard:
                    buttonPrefab = removeCardButtonPrefab;
                    break;
                case CardPageType.RecruitUnit:
                    buttonPrefab = recruitUnitButtonPrefab;
                    break;
                case CardPageType.CloneUnit:
                    buttonPrefab = cloneUnitButtonPrefab;
                    break;
                default:
                    Debug.LogError("INVALID TYPE!");
                    return;
            }

            GameObject button = Instantiate(buttonPrefab, costGroup.transform);
            switch (cardPageType)
            {
                case CardPageType.LearnSkill:
                    button.GetComponent<LearnSkillButton>().SkillCard = card as SkillCard;
                    break;
                case CardPageType.RemoveCard:
                    button.GetComponent<RemoveCardButton>().Card = card;
                    break;
                case CardPageType.RecruitUnit:
                    button.GetComponent<RecruitUnitButton>().UnitCard = card as UnitCard;
                    break;
                case CardPageType.CloneUnit:
                    button.GetComponent<CloneUnitButton>().UnitCard = card as UnitCard;
                    break;
                default:
                    Debug.LogError("INVALID TYPE!");
                    return;
            }
            activeCards.Add(button);
        }
    }

    public void NextPageButton_OnClick()
    {
        if (currentPage == totalPages) return;
        currentPage++;
        LoadCardPage();
    }

    public void PreviousPageButton_OnClick()
    {
        if (currentPage == 1) return;
        currentPage--;
        LoadCardPage();
    }
    
    public void LearnSkillButton_OnClick(SkillCard skillCard)
    {
        if (pMan.AetherCells < GameManager.LEARN_SKILL_COST)
            uMan.InsufficientAetherPopup();
        else uMan.CreateLearnSkillPopup(skillCard);
    }

    public void RecruitUnitButton_OnClick(UnitCard unitCard)
    {
        if (pMan.AetherCells < GameManager.RECRUIT_UNIT_COST)
            uMan.InsufficientAetherPopup();
        else uMan.CreateRecruitUnitPopup(unitCard);
    }

    public void RemoveCardButton_OnClick(Card card)
    {
        if (pMan.PlayerDeckList.Count <= GameManager.MINIMUM_DECK_SIZE)
            uMan.CreateFleetingInfoPopup("You must have at least " +
                GameManager.MINIMUM_DECK_SIZE + " cards in your deck!", true);
        else if (pMan.AetherCells < GameManager.REMOVE_CARD_COST)
            uMan.InsufficientAetherPopup();
        else uMan.CreateRemoveCardPopup(card);
    }

    public void CloneUnitButton_OnClick(UnitCard unitCard)
    {
        if (pMan.AetherCells < GameManager.CLONE_UNIT_COST)
            uMan.InsufficientAetherPopup();
        else uMan.CreateCloneUnitPopup(unitCard);
    }

    public void CloseCardPageButton_OnClick()
    {
        if (SceneLoader.IsActiveScene(SceneLoader.Scene.DialogueScene))
            DialogueManager.Instance.DisplayDialoguePopup();
        uMan.DestroyCardPagePopup();
    }
}
