﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    /* SINGELTON_PATTERN */
    public static CardManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    #region FIELDS

    [Header("PREFABS"), SerializeField] private GameObject unitCardPrefab;
    [SerializeField] private GameObject actionCardPrefab, unitZoomCardPrefab,
        actionZoomCardPrefab, cardContainerPrefab, dragArrowPrefab;
    [Header("PLAYER START UNITS")]
    [SerializeField] private UnitCard[] playerStartUnits;
    [Header("TUTORIAL PLAYER UNITS")]
    [SerializeField] private UnitCard[] tutorialPlayerUnits;
    [Header("ULTIMATE CREATED CARDS"), SerializeField] private ActionCard exploit_Ultimate;
    [SerializeField] private ActionCard invention_Ultimate, scheme_Ultimate, extraction_Ultimate;

    // Related Card References
    [Header("RELATED CARD REFERENCES")]
    public Card[] Exploits;
    public Card[] Inventions;
    public Card[] Schemes;
    public Card[] Extractions;
    public Card[] Traps;

    private int lastCardIndex, lastContainerIndex;

    // Positive Keywords
    public const string ABILITY_ARMORED = "Armored";
    public const string ABILITY_BLITZ = "Blitz";
    public const string ABILITY_DEFENDER = "Defender";
    public const string ABILITY_FORCEFIELD = "Forcefield";
    public const string ABILITY_POISONOUS = "Poisonous";
    public const string ABILITY_RANGED = "Ranged";
    public const string ABILITY_REGENERATION = "Regeneration";
    public const string ABILITY_STEALTH = "Stealth";
    public const string ABILITY_WARD = "Ward";
    // Status Keywords
    public const string ABILITY_MARKED = "Marked";
    public const string ABILITY_POISONED = "Poisoned";
    public const string ABILITY_WOUNDED = "Wounded";
    // Ability Triggers
    public const string TRIGGER_DEATHBLOW = "Deathblow";
    public const string TRIGGER_INFILTRATE = "Infiltrate";
    public const string TRIGGER_PLAY = "Play";
    public const string TRIGGER_RESEARCH = "Research";
    public const string TRIGGER_RETALIATE = "Retaliate";
    public const string TRIGGER_REVENGE = "Revenge";
    public const string TRIGGER_SPARK = "Spark";
    public const string TRIGGER_TRAP = "Trap";
    public const string TRIGGER_TURN_START = "Turn Start";
    public const string TRIGGER_TURN_END = "Turn End";

    // Card Types
    public const string EXPLOIT = "Exploit";
    public const string INVENTION = "Invention";
    public const string SCHEME = "Scheme";

    public const string EXTRACTION = "Extraction";

    // Ability Keywords
    private static string[] AbilityKeywords = new string[]
    {
        // Positive Keywords
        ABILITY_ARMORED,
        ABILITY_BLITZ,
        ABILITY_DEFENDER,
        ABILITY_FORCEFIELD,
        ABILITY_POISONOUS,
        ABILITY_RANGED,
        ABILITY_REGENERATION,
        ABILITY_STEALTH,
        ABILITY_WARD,
        // Status Keywords
        ABILITY_MARKED,
        "Mark",
        ABILITY_POISONED,
        "Poison",
        "Silence",
        "Stunned",
        "Stun",
        "Evade",
        "Exhausted",
        "Refreshed",
        "Refresh",
        ABILITY_WOUNDED,
        // Ability Triggers
        TRIGGER_DEATHBLOW,
        TRIGGER_INFILTRATE,
        TRIGGER_PLAY,
        TRIGGER_RESEARCH,
        TRIGGER_RETALIATE,
        TRIGGER_REVENGE,
        TRIGGER_SPARK,
        "Traps",
        TRIGGER_TRAP,
        TRIGGER_TURN_START,
        TRIGGER_TURN_END
    };

    // Card Types
    private static string[] CardTypes = new string[]
    {
        EXPLOIT + "s",
        EXPLOIT,
        INVENTION + "s",
        INVENTION,
        SCHEME + "s",
        SCHEME,
        EXTRACTION + "s",
        EXTRACTION
    };

    // Unit Types
    public const string MAGE = "Mage";
    public const string MUTANT = "Mutant";
    public const string ROGUE = "Rogue";
    public const string TECH = "Tech";
    public const string WARRIOR = "Warrior";

    // Generatable Keywords
    [Header("GENERATABLE KEYWORDS")]
    public List<StaticAbility> GeneratableKeywords;

    // Positive Abilities
    public static string[] PositiveAbilities = new string[]
    {
        ABILITY_ARMORED,
        ABILITY_BLITZ,
        ABILITY_DEFENDER,
        ABILITY_FORCEFIELD,
        ABILITY_POISONOUS,
        ABILITY_RANGED,
        ABILITY_REGENERATION,
        ABILITY_STEALTH,
        ABILITY_WARD,
    };

    // Negative Abilities
    public static string[] NegativeAbilities = new string[]
    {
        ABILITY_MARKED,
        ABILITY_POISONED,
        ABILITY_WOUNDED,
    };
    #endregion

    #region PROPERTIES
    public GameObject UnitCardPrefab { get => unitCardPrefab; }
    public GameObject ActionCardPrefab { get => actionCardPrefab; }
    public GameObject UnitZoomCardPrefab { get => unitZoomCardPrefab; }
    public GameObject ActionZoomCardPrefab { get => actionZoomCardPrefab; }
    public GameObject CardContainerPrefab { get => cardContainerPrefab; }
    public GameObject DragArrowPrefab { get => dragArrowPrefab; }

    public UnitCard[] PlayerStartUnits { get => playerStartUnits; }
    public UnitCard[] TutorialPlayerUnits { get => tutorialPlayerUnits; }

    public ActionCard Exploit_Ultimate { get => exploit_Ultimate; }
    public ActionCard Invention_Ultimate { get => invention_Ultimate; }
    public ActionCard Scheme_Ultimate { get => scheme_Ultimate; }
    public ActionCard Extraction_Ultimate { get => extraction_Ultimate; }

    public List<UnitCard> PlayerRecruitUnits { get; private set; }
    public List<ActionCard> ActionShopCards { get; private set; }
    #endregion

    #region METHODS

    #region UTILITY
    private void Start()
    {
        PlayerRecruitUnits = new List<UnitCard>();
        ActionShopCards = new List<ActionCard>();
    }
    #endregion

    #region CARD HANDLING
    /******
     * *****
     * ****** GetRelatedCards
     * *****
     *****/
    public Card[] GetCreatedCards(string cardType, bool includeUltimates)
    {
        if (Enum.TryParse(cardType, out Card.CreatedCardType enumType))
            return GetCreatedCards(enumType, includeUltimates);
        else return null;
    }
    public Card[] GetCreatedCards(Card.CreatedCardType cardType, bool includeUltimates)
    {
        Card[] createdCards;
        Card ultimate;

        switch (cardType)
        {
            case Card.CreatedCardType.Exploit:
                createdCards = Exploits;
                ultimate = exploit_Ultimate;
                break;
            case Card.CreatedCardType.Invention:
                createdCards = Inventions;
                ultimate = invention_Ultimate;
                break;
            case Card.CreatedCardType.Scheme:
                createdCards = Schemes;
                ultimate = scheme_Ultimate;
                break;
            case Card.CreatedCardType.Extraction:
                createdCards = Extractions;
                ultimate = extraction_Ultimate;
                break;
            case Card.CreatedCardType.Trap:
                return Traps;
            default:
                Debug.LogError("INVALID CARD TYPE!");
                return null;
        }

        return includeUltimates ? createdCards.Concat(new Card[] { ultimate }).ToArray() : createdCards;
    }
    /******
     * *****
     * ****** NEW_CARD_INSTANCE
     * *****
     *****/
    public Card NewCardInstance(Card card, bool isExactCopy = false)
    {
        var cardScript = ScriptableObject.CreateInstance(card.GetType()) as Card;

        if (isExactCopy) cardScript.CopyCard(card);
        else cardScript.LoadCard(card);
        return cardScript;
    }
    /******
     * *****
     * ****** SHOW_CARD
     * *****
     *****/
    public enum DisplayType
    {
        Default,
        HeroSelect,
        NewCard,
        ChooseCard,
        Cardpage
    }
    public GameObject ShowCard(Card card, Vector2 position,
        DisplayType type = DisplayType.Default, bool banishAfterPlay = false)
    {
        if (card == null)
        {
            Debug.LogError("CARD IS NULL!");
            return null;
        }

        card.BanishAfterPlay = banishAfterPlay;
        GameObject prefab = null;

        if (card is UnitCard)
        {
            // Unused "New Card" functionality
            prefab = type is DisplayType.NewCard ? UnitZoomCardPrefab : UnitCardPrefab;
        }
        else if (card is ActionCard)
        {
            // Unused "New Card" functionality
            prefab = type is DisplayType.NewCard ? ActionZoomCardPrefab : ActionCardPrefab;
        }

        GameObject parent = null;
        if (Managers.CO_MAN.CardZone != null) parent = Managers.CO_MAN.CardZone;
        //else if (Managers.U_MAN.CurrentCanvas != null) parent = Managers.U_MAN.CurrentCanvas;
        else if (Managers.U_MAN.UICanvas != null) parent = Managers.U_MAN.UICanvas;

        if (parent == null)
        {
            Debug.LogError("PARENT IS NULL!");
            return null;
        }

        prefab = Instantiate(prefab, parent.transform);
        prefab.transform.position = position;
        var cd = prefab.GetComponent<CardDisplay>();

        if (type is DisplayType.Default)
        {
            cd.CardScript = card;

            //var containParent = Managers.U_MAN.CurrentCanvas != null ? Managers.U_MAN.CurrentCanvas : Managers.U_MAN.UICanvas;
            //cd.CardContainer = Instantiate(CardContainerPrefab, containParent.transform);

            cd.CardContainer = Instantiate(CardContainerPrefab, Managers.U_MAN.UICanvas.transform); // TESTING
            cd.CardContainer.transform.position = position;
            var cc = cd.CardContainer.GetComponent<CardContainer>();
            cc.Child = prefab;
        }
        else
        {
            prefab.tag = Managers.P_MAN.CARD_TAG; // For CardZoom.CreateAbilityPopups()
            var newCard = NewCardInstance(card);
            if (type is DisplayType.HeroSelect) cd.CardScript = newCard;
            else if (type is DisplayType.NewCard) cd.DisplayZoomCard(newCard);
            else if (type is DisplayType.ChooseCard or DisplayType.Cardpage) cd.DisplayCardPageCard(newCard);
        }
        return prefab;
    }
    /******
     * *****
     * ****** HIDE_CARD
     * *****
     *****/
    public Card HideCard(GameObject card)
    {
        card.GetComponent<CardZoom>().DestroyZoomPopups();
        Card cardScript = card.GetComponent<CardDisplay>().CardScript;
        Destroy(card.GetComponent<CardDisplay>().CardContainer);
        if (card != null) Destroy(card);
        return cardScript;
    }
    /******
     * *****
     * ****** DRAW_CARD
     * *****
     *****/
    public GameObject DrawCard(HeroManager hero, Card drawnCard = null, List<Effect> additionalEffects = null)
    {
        if (hero == null)
        {
            Debug.LogError("HERO IS NULL!");
            return null;
        }

        var deck = hero.CurrentDeck;
        var hand = hero.HandZoneCards;
        string cardTag = hero.CARD_TAG;
        Vector2 position = new();

        if (hero == Managers.P_MAN)
        {
            if (hand.Count >= GameManager.MAX_HAND_SIZE)
            {
                Managers.U_MAN.CreateFleetingInfoPopup("Your hand is full!");
                Debug.Log("PLAYER HAND IS FULL!");
                return null;
            }

            if (drawnCard == null) position.Set(-850, -410);
            else position.Set(0, -350);
        }
        else if (hero == Managers.EN_MAN)
        {
            if (hand.Count >= GameManager.MAX_HAND_SIZE)
            {
                Managers.U_MAN.CreateFleetingInfoPopup("Enemy hand is full!");
                Debug.Log("ENEMY HAND IS FULL!");
                return null;
            }

            if (drawnCard == null) position.Set(850, 427);
            else position.Set(0, 350);
        }

        // Shuffle discard into deck
        if (drawnCard == null && deck.Count < 1)
        {
            var discard = hero.DiscardZoneCards;
            if (discard.Count < 1)
            {
                Debug.LogError("DISCARD IS EMPTY!");
                return null;
            }

            deck.AddRange(discard);
            discard.Clear();
            ShuffleDeck(hero);
        }

        GameObject card;
        if (drawnCard == null)
        {
            Managers.AU_MAN.StartStopSound("SFX_DrawCard");
            card = ShowCard(deck[0], position);
        }
        else
        {
            Managers.AU_MAN.StartStopSound("SFX_CreateCard");
            card = ShowCard(drawnCard, position, DisplayType.Default, true);
        }

        if (card == null)
        {
            Debug.LogError("CARD IS NULL!");
            return null;
        }

        if (drawnCard == null) deck.RemoveAt(0);
        card.tag = cardTag;
        ChangeCardZone(card, hero.HandZone);
        Managers.AN_MAN.CreateParticleSystem(card, ParticleSystemHandler.ParticlesType.Drag, 1);

        hero.HandZoneCards.Add(card);

        if (additionalEffects != null)
        {
            foreach (var addEffect in additionalEffects)
                Managers.EF_MAN.ResolveEffect(new List<GameObject>
                { card }, addEffect, false, 0, out _, false);
        }

        return card;
    }
    /******
     * *****
     * ****** CHANGE_CARD_ZONE
     * *****
     *****/
    public enum ZoneChangeType
    {
        Default,
        ReturnToIndex,
        ChangeControl,
        LoadFromSave,
    }
    public void ChangeCardZone(GameObject card, GameObject newZone, ZoneChangeType changeType = ZoneChangeType.Default)
    {
        if (card == null)
        {
            Debug.LogError("CARD IS NULL!");
            return;
        }
        if (newZone == null)
        {
            Debug.LogError("NEW ZONE IS NULL"!);
            return;
        }

        var cd = card.GetComponent<CardDisplay>();
        var dd = card.GetComponent<DragDrop>();
        var container = cd.CardContainer.GetComponent<CardContainer>();
        Action action;

        bool isPlayed = true;
        bool wasPlayed = dd.IsPlayed; // For cards returned to hand

        Managers.U_MAN.SelectTarget(card, UIManager.SelectionType.Disabled); // Unnecessary?

        if (newZone == Managers.P_MAN.HandZone)
        {
            action = () => Managers.AN_MAN.RevealedHandState(card);
            isPlayed = false;
        }
        else if (newZone == Managers.P_MAN.PlayZone)
        {
            action = () => Managers.AN_MAN.PlayedUnitState(card);
        }
        else if (newZone == Managers.P_MAN.ActionZone)
        {
            action = () => Managers.AN_MAN.PlayedActionState(card);
        }
        else if (newZone == Managers.EN_MAN.HandZone)
        {
            action = () => Managers.AN_MAN.HiddenHandState(card);
            isPlayed = false;
        }
        else if (newZone == Managers.EN_MAN.PlayZone)
        {
            action = () => Managers.AN_MAN.PlayedUnitState(card);
        }
        else if (newZone == Managers.EN_MAN.ActionZone)
        {
            action = () => Managers.AN_MAN.PlayedActionState(card);
        }
        else
        {
            Debug.LogError("INVALID ZONE!");
            return;
        }

        container.OnAttachAction += () => action();

        if (changeType is not ZoneChangeType.ReturnToIndex)
        {
            lastCardIndex = dd.LastIndex;
            lastContainerIndex = cd.CardContainer.transform.GetSiblingIndex();

            /*
            if (newZone == Managers.P_MAN.HandZone) card.transform.SetAsLastSibling();
            else card.transform.SetAsFirstSibling();
            */

            // Set hand cards immediately or they will momentarily appear in front of others
            if (newZone == Managers.EN_MAN.HandZone) card.transform.SetAsFirstSibling();
            else if (newZone == Managers.P_MAN.HandZone) card.transform.SetAsLastSibling();
            else container.OnAttachAction += () => card.transform.SetAsFirstSibling();
        }

        container.MoveContainer(newZone);

        if (changeType is ZoneChangeType.ChangeControl) dd.IsPlayed = true;
        else if (changeType is ZoneChangeType.ReturnToIndex)
        {
            card.transform.SetSiblingIndex(lastCardIndex);
            cd.CardContainer.transform.SetSiblingIndex(lastContainerIndex);
            dd.IsPlayed = isPlayed;
        }
        else if (!isPlayed) // => PlayerHand OR EnemyHand
        {
            if (wasPlayed) // For ReturnCard effects (Play => Hand)
            {
                cd.ResetCard();
                dd.IsPlayed = false;
            }
            Managers.EF_MAN.ApplyChangeNextCostEffects(card);
        }

        if (cd is UnitCardDisplay ucd)
        {
            if (changeType is not ZoneChangeType.LoadFromSave) // !!! Load From Save !!!
            {
                bool isExhausted = false;

                if (isPlayed)
                {
                    if (changeType is not ZoneChangeType.ChangeControl &&
                        !GetAbility(card, ABILITY_BLITZ)) isExhausted = true;
                }

                ucd.IsExhausted = isExhausted;
            }

            if (isPlayed) ucd.EnableVFX();
            else ucd.DisableVFX();

            container.OnAttachAction += () => FunctionTimer.Create(() => SetStats(card), 0.1f);

            void SetStats(GameObject unitCard)
            {
                if (unitCard == null) return;
                Managers.AN_MAN.UnitStatChangeState(card, 0, 0, false, true);
            }
        }

    }
    /******
     * *****
     * ****** IS_PLAYABLE
     * *****
     *****/
    public bool IsPlayable(GameObject card, bool isPrecheck = false, bool ignoreCost = false)
    {
        if (card == null)
        {
            Debug.LogError("CARD IS NULL!");
            return false;
        }

        HeroManager hMan = HeroManager.GetSourceHero(card);
        CardDisplay cardDisplay = card.GetComponent<CardDisplay>();

        if (cardDisplay is UnitCardDisplay)
        {
            bool isPlayerHero = hMan == Managers.P_MAN;
            var zoneCards = isPlayerHero ? Managers.P_MAN.PlayZoneCards : Managers.EN_MAN.PlayZoneCards;
            string errorMessage = isPlayerHero ? "You can't play more units!" : "Enemy can't play more units!";

            if (zoneCards.Count >= GameManager.MAX_UNITS_PLAYED)
            {
                if (!isPrecheck)
                {
                    Managers.U_MAN.CreateFleetingInfoPopup(errorMessage);
                    ErrorSound();
                }
                return false;
            }
        }
        else if (cardDisplay is ActionCardDisplay acd)
        {
            if (!Managers.EF_MAN.CheckLegalTargets(acd.ActionCard.EffectGroupList, card, true))
            {
                if (!isPrecheck)
                {
                    Managers.U_MAN.CreateFleetingInfoPopup("You can't play that right now!");
                    ErrorSound();
                }
                return false;
            }
        }

        if (!ignoreCost)
        {
            int energyLeft;
            if (hMan == Managers.P_MAN) energyLeft = Managers.P_MAN.CurrentEnergy;
            else energyLeft = Managers.EN_MAN.CurrentEnergy;

            if (energyLeft < cardDisplay.CurrentEnergyCost)
            {
                if (!isPrecheck)
                {
                    Managers.U_MAN.CreateFleetingInfoPopup("Not enough energy!");
                    ErrorSound();
                }
                return false;
            }
        }

        return true;
        static void ErrorSound() => Managers.AU_MAN.StartStopSound("SFX_Error");
    }
    /******
     * *****
     * ****** PLAY_CARD [HAND >>> PLAY]
     * *****
     *****/
    public void PlayCard(GameObject card)
    {
        var hMan = HeroManager.GetSourceHero(card);
        var cd = card.GetComponent<CardDisplay>();
        var container = cd.CardContainer.GetComponent<CardContainer>();

        if (cd is UnitCardDisplay)
        {
            if (hMan.PlayZoneCards.Count >= GameManager.MAX_UNITS_PLAYED)
            {
                Debug.LogError("TOO MANY UNITS!");
                return;
            }
        }

        Managers.EV_MAN.PauseDelayedActions(true);
        container.OnAttachAction += () => Managers.EV_MAN.PauseDelayedActions(false);

        hMan.HandZoneCards.Remove(card);

        int energyLeft = hMan.CurrentEnergy;
        hMan.CurrentEnergy -= cd.CurrentEnergyCost;
        int energyChange = hMan.CurrentEnergy - energyLeft;

        if (energyChange != 0) Managers.AN_MAN.ModifyHeroEnergyState(energyChange, hMan.HeroObject, false);

        if (hMan == Managers.P_MAN) Managers.EV_MAN.NewDelayedAction(() => SelectPlayableCards(), 0, true);

        if (cd is UnitCardDisplay)
        {
            hMan.PlayZoneCards.Add(card);
            ChangeCardZone(card, hMan.PlayZone);
            Managers.EV_MAN.NewDelayedAction(() => PlayUnit(), 0, true);
        }
        else if (cd is ActionCardDisplay)
        {
            hMan.ActionZoneCards.Add(card);
            ChangeCardZone(card, hMan.ActionZone);
            Managers.EV_MAN.NewDelayedAction(() => PlayAction(), 0, true);
        }
        else
        {
            Debug.LogError("INVALID TYPE!");
            return;
        }

        void PlayUnit()
        {
            if (card == null)
            {
                Debug.LogError("CARD IS NULL!");
                return;
            }

            /*
            bool hasPlayTrigger = TriggerUnitAbility(card, TRIGGER_PLAY);
            if (HeroManager.GetSourceHero(card) == Managers.P_MAN)
            {
                if (!hasPlayTrigger) UnitTriggers();
            }
            else UnitTriggers();
            */

            if (!TriggerUnitAbility(card, TRIGGER_PLAY)) UnitTriggers(); // TESTING

            card.transform.SetAsFirstSibling();
            PlayCardSound();
            ParticleBurst();

            void UnitTriggers()
            {
                Managers.U_MAN.CombatLog_PlayCard(card);
                Managers.EF_MAN.ResolveChangeNextCostEffects(card); // Resolves IMMEDIATELY

                TriggerTrapAbilities(card); // Resolves 3rd
                Managers.EF_MAN.TriggerModifiers_PlayCard(card); // Resolves 2nd
                Managers.EF_MAN.TriggerGiveNextEffects(card); // Resolves 1st
            }
        }
        void PlayAction()
        {
            if (card == null)
            {
                Debug.LogError("CARD IS NULL!");
                return;
            }

            Managers.AU_MAN.StartStopSound("SFX_PlayCard");
            ResolveActionCard(card); // Resolves IMMEDIATELY
            ParticleBurst();
        }
        void PlayCardSound()
        {
            Sound playSound = cd.CardScript.CardPlaySound;
            if (playSound.clip == null) Debug.LogWarning("MISSING PLAY SOUND: " + cd.CardName);
            else Managers.AU_MAN.StartStopSound(null, playSound);
        }

        void ParticleBurst() =>
            Managers.AN_MAN.CreateParticleSystem(card, ParticleSystemHandler.ParticlesType.Play, 1);
    }

    /******
     * *****
     * ****** DISCARD_CARD [HAND/ACTION_ZONE >>> DISCARD]
     * *****
     *****/
    public void DiscardCard(GameObject card, bool isAction = false)
    {
        var cd = card.GetComponent<CardDisplay>();
        var hMan = HeroManager.GetSourceHero(card);

        var previousZone = isAction ? hMan.ActionZoneCards : hMan.HandZoneCards;
        var newZone = hMan.DiscardZoneCards;
        previousZone.Remove(card);

        if (cd.CardScript.BanishAfterPlay) HideCard(card);
        else
        {
            cd.ResetCard();
            newZone.Add(HideCard(card));
        }
        if (!isAction) Managers.AU_MAN.StartStopSound("SFX_DiscardCard");
    }

    /******
     * *****
     * ****** RESOLVE_ACTION_CARD
     * *****
     *****/
    private void ResolveActionCard(GameObject card)
    {
        var groupList = card.GetComponent<ActionCardDisplay>().ActionCard.EffectGroupList;
        Managers.EF_MAN.StartEffectGroupList(groupList, card);
    }

    /******
     * *****
     * ****** GET_COST_CONDITION_VALUE
     * *****
     *****/
    public int GetCostConditionValue(Card cardScript, GameObject source) =>
        GetCostConditionValue_Finish(source, cardScript.CostConditionType, cardScript.CostConditionValue, cardScript.CostConditionModifier);
    public int GetCostConditionValue(HeroPower power, GameObject source) =>
        GetCostConditionValue_Finish(source, power.CostConditionType, power.CostConditionValue, power.CostConditionModifier);

    private int GetCostConditionValue_Finish(GameObject source, Effect.ConditionType conditionType, int conditionValue, int conditionModifier)
    {
        var hMan_Source = HeroManager.GetSourceHero(source, out HeroManager hMan_Enemy);
        switch (conditionType)
        {
            case Effect.ConditionType.NONE:
                return 0;
            case Effect.ConditionType.EnemyWounded:
                if (!hMan_Enemy.IsWounded()) return 0;
                break;
            case Effect.ConditionType.AlliesDestroyed_ThisTurn:
                if (hMan_Source.AlliesDestroyed_ThisTurn < conditionValue) return 0;
                break;
            case Effect.ConditionType.EnemiesDestroyed_ThisTurn:
                if (hMan_Enemy.AlliesDestroyed_ThisTurn < conditionValue) return 0;
                break;
            case Effect.ConditionType.HasMoreCards_Player:
                if (hMan_Source.HandZoneCards.Count <= conditionValue) return 0;
                break;
            case Effect.ConditionType.HasLessCards_Player:
                if (hMan_Source.HandZoneCards.Count >= conditionValue) return 0;
                break;
            default:
                Debug.LogError("INVALID CONDITION TYPE!");
                return 0;
        }
        return conditionModifier;
    }

    /******
     * *****
     * ****** SELECT_PLAYABLE_CARDS
     * *****
     *****/
    public void SelectPlayableCards(bool setAllFalse = false)
    {
        Managers.EV_MAN.NewDelayedAction(() => SelectCards(), 0, true);

        void SelectCards()
        {
            int playableCards = 0;
            bool isPlayerTurn = !setAllFalse && Managers.P_MAN.IsMyTurn;

            // Cards in Hand
            foreach (var card in Managers.P_MAN.HandZoneCards)
            {
                if (card == null)
                {
                    Debug.LogError("CARD IS NULL!");
                    continue;
                }

                // Apply cost conditions
                card.GetComponent<CardDisplay>().UpdateCurrentEnergyCost();

                if (isPlayerTurn && IsPlayable(card, true))
                {
                    playableCards++;
                    Managers.U_MAN.SelectTarget(card, UIManager.SelectionType.Playable);
                }
                else Managers.U_MAN.SelectTarget(card, UIManager.SelectionType.Disabled);
            }

            // Hero Powers
            // Display cost condition values
            Managers.P_MAN.HeroObject.GetComponent<PlayerHeroDisplay>().DisplayHeroPowers();

            bool playerHasActions = playableCards > 0;

            foreach (var ally in Managers.P_MAN.PlayZoneCards)
            {
                if (isPlayerTurn && Managers.CO_MAN.CanAttack(ally, null, true, true))
                {
                    Managers.U_MAN.SelectTarget(ally, UIManager.SelectionType.Playable);
                    playerHasActions = true;
                }
                else Managers.U_MAN.SelectTarget(ally, UIManager.SelectionType.Disabled);
            }

            if (!playerHasActions)
            {
                if (Managers.P_MAN.UseHeroPower(false, true) ||
                    Managers.P_MAN.UseHeroPower(true, true)) playerHasActions = true;
            }

            if (setAllFalse) playerHasActions = false;
            Managers.U_MAN.SetReadyEndTurnButton(!playerHasActions);
        }
    }

    /******
     * *****
     * ****** CHANGE_UNIT_CONTROL
     * *****
     *****/
    public void ChangeUnitControl(GameObject card)
    {
        var hMan_Source = HeroManager.GetSourceHero(card, out HeroManager hMan_Enemy);

        if (hMan_Enemy.PlayZoneCards.Count >= GameManager.MAX_UNITS_PLAYED)
        {
            Debug.LogWarning("TOO MANY UNITS!");
            Managers.CO_MAN.DestroyUnit(card);
            return;
        }

        hMan_Source.PlayZoneCards.Remove(card);
        card.tag = hMan_Enemy.CARD_TAG;
        ChangeCardZone(card, hMan_Enemy.PlayZone, ZoneChangeType.ChangeControl);
        hMan_Enemy.PlayZoneCards.Add(card);
    }

    public string FilterCreatedCardProgress(string text, bool isPlayerSource)
    {
        HeroManager hMan = isPlayerSource ? Managers.P_MAN : Managers.EN_MAN;
        text = text.Replace("{EXPLOITS}", CardProgress(hMan.ExploitsPlayed));
        text = text.Replace("{INVENTIONS}", CardProgress(hMan.InventionsPlayed));
        text = text.Replace("{SCHEMES}", CardProgress(hMan.SchemesPlayed));
        text = text.Replace("{EXTRACTIONS}", CardProgress(hMan.ExtractionsPlayed));
        return text;

        static string CardProgress(int progress) =>
            $"{TextFilter.Clrz_ylw(progress.ToString())}/{TextFilter.Clrz_ylw("3")}";
    }

    public string FilterKeywords(string text)
    {
        foreach (string str in AbilityKeywords)
            text = text.Replace(str, TextFilter.Clrz_ylw(str));
        foreach (string str in CardTypes)
            text = text.Replace(str, TextFilter.Clrz_grn(str));

        text = FilterUnitTypes(text);
        return text;
    }

    public string FilterUnitTypes(string text)
    {
        string[] unitTypes =
        {
            MAGE + "s",
            MAGE,
            MUTANT + "s",
            MUTANT,
            ROGUE + "s",
            ROGUE,
            TECH + "s",
            TECH,
            WARRIOR + "s",
            WARRIOR
        };

        foreach (string s in unitTypes) text = text.Replace(s, TextFilter.Clrz_grn(s));
        return text;
    }

    public Color GetAbilityColor(CardAbility cardAbility)
    {
        if (cardAbility.OverrideColor) return cardAbility.AbilityColor;
        if (cardAbility is TriggeredAbility or ModifierAbility) return Color.yellow;

        foreach (string posAbi in PositiveAbilities)
            if (cardAbility.AbilityName == posAbi) return Color.green;
        foreach (string negAbi in NegativeAbilities)
            if (cardAbility.AbilityName == negAbi) return Color.red;

        return Color.yellow;
    }

    public void LoadNewActions()
    {
        var allActions = Resources.LoadAll<ActionCard>("Cards_Actions");
        allActions.Shuffle();
        List<ActionCard> actionList = new();

        foreach (var action in allActions)
        {
            // Card Rarity Functionality
            switch (action.CardRarity)
            {
                case Card.Rarity.Common:
                    AddCard();
                    AddCard();
                    AddCard();
                    goto case Card.Rarity.Rare;
                case Card.Rarity.Rare:
                    AddCard();
                    AddCard();
                    goto case Card.Rarity.Legend;
                case Card.Rarity.Legend:
                    AddCard();
                    break;
            }
            void AddCard() => actionList.Add(action);
        }

        while (ActionShopCards.Count < 8)
        {
            foreach (var ac in actionList)
            {
                if (ac == null) continue;
                int index = Managers.P_MAN.DeckList.FindIndex(x => x.CardName == ac.CardName);
                if (index == -1 && !ActionShopCards.Contains(ac))
                {
                    ActionShopCards.Add(ac);
                    if (ActionShopCards.Count > 7) break;
                }
            }
        }
    }
    public void LoadNewRecruits()
    {
        var allRecruits = Resources.LoadAll<UnitCard>("Cards_Units");
        allRecruits.Shuffle();

        List<UnitCard> recruitMages = new();
        List<UnitCard> recruitMutants = new();
        List<UnitCard> recruitRogues = new();
        List<UnitCard> recruitTechs = new();
        List<UnitCard> recruitWarriors = new();

        foreach (var unitCard in allRecruits)
        {
            List<UnitCard> targetList;
            switch (unitCard.CardType)
            {
                case MAGE:
                    targetList = recruitMages;
                    break;
                case MUTANT:
                    targetList = recruitMutants;
                    break;
                case ROGUE:
                    targetList = recruitRogues;
                    break;
                case TECH:
                    targetList = recruitTechs;
                    break;
                case WARRIOR:
                    targetList = recruitWarriors;
                    break;
                default:
                    Debug.LogError($"CARD TYPE NOT FOUND FOR <{unitCard.CardName}>");
                    return;
            }

            // Card Rarity Functionality
            switch (unitCard.CardRarity)
            {
                case Card.Rarity.Common:
                    AddCard();
                    AddCard();
                    AddCard();
                    goto case Card.Rarity.Rare;
                case Card.Rarity.Rare:
                    AddCard();
                    AddCard();
                    goto case Card.Rarity.Legend;
                case Card.Rarity.Legend:
                    AddCard();
                    break;
            }
            void AddCard() => targetList.Add(unitCard);
        }

        List<List<UnitCard>> recruitLists = new()
        {
            recruitMages,
            recruitMutants,
            recruitRogues,
            recruitTechs,
            recruitWarriors
        };

        while (PlayerRecruitUnits.Count < 8)
        {
            foreach (var list in recruitLists)
            {
                foreach (var uc in list)
                {
                    if (uc == null) continue;
                    int index = Managers.P_MAN.DeckList.FindIndex(x => x.CardName == uc.CardName);
                    if (index == -1 && !PlayerRecruitUnits.Contains(uc))
                    {
                        PlayerRecruitUnits.Add(uc);
                        if (PlayerRecruitUnits.Count > 7) goto FinishRecruits;
                        break;
                    }
                }
            }
        FinishRecruits:;
        }
    }

    public enum ChooseCard
    {
        Action,
        Unit
    }
    public Card[] ChooseCards(ChooseCard chooseCard)
    {
        Card[] allChooseCards;
        string chooseCardType;

        switch (chooseCard)
        {
            case ChooseCard.Action:
                chooseCardType = "Cards_Actions";
                break;
            case ChooseCard.Unit:
                chooseCardType = "Cards_Units";
                break;
            default:
                Debug.LogError("INVALID TYPE!");
                return null;
        }

        allChooseCards = Resources.LoadAll<Card>(chooseCardType);

        if (allChooseCards.Length < 1)
        {
            Debug.LogError("NO CARDS FOUND!");
            return null;
        }

        // Card Rarity Functionality
        List<Card> cardPool = new();
        foreach (var card in allChooseCards)
        {
            switch (card.CardRarity)
            {
                case Card.Rarity.Common:
                    AddCard();
                    AddCard();
                    AddCard();
                    goto case Card.Rarity.Rare;
                case Card.Rarity.Rare:
                    AddCard();
                    AddCard();
                    goto case Card.Rarity.Legend;
                case Card.Rarity.Legend:
                    AddCard();
                    break;
            }
            void AddCard() => cardPool.Add(card);
        }

        cardPool.Shuffle();
        var chooseCards = new Card[3];
        int index = 0;

        // Limit Duplicates
        GetChooseCards(true);
        if (index < 3)
        {
            index = 0;
            GetChooseCards(false);
        }

        return chooseCards;

        void GetChooseCards(bool limitDuplicates)
        {
            foreach (var card in cardPool)
            {
                if (chooseCards.Contains(card)) continue;

                int otherCopies = 0;
                if (limitDuplicates)
                {
                    foreach (var playerCard in Managers.P_MAN.DeckList)
                    {
                        if (playerCard.CardName == card.CardName)
                            otherCopies++;
                    }
                }

                if (!limitDuplicates || otherCopies < 1)
                {
                    chooseCards[index++] = card;
                    if (index == 3) break;
                }
            }
        }
    }

    /******
     * *****
     * ****** ADD/REMOVE_CARD
     * *****
     *****/
    public void AddCard(Card card, HeroManager hero, bool changeReputation = false)
    {
        var cardInstance = ScriptableObject.CreateInstance(card.GetType()) as Card;
        cardInstance.LoadCard(card);
        hero.DeckList.Add(cardInstance);

        if (changeReputation && hero == Managers.P_MAN)
            UnitReputationChange(card, false);
    }

    private void UnitReputationChange(Card card, bool isRemoval)
    {
        if (card is not UnitCard) return;
        GameManager.ReputationType repType;

        switch (card.CardType)
        {
            case MAGE:
                repType = GameManager.ReputationType.Mages;
                break;
            case MUTANT:
                repType = GameManager.ReputationType.Mutants;
                break;
            case ROGUE:
                repType = GameManager.ReputationType.Rogues;
                break;
            case TECH:
                repType = GameManager.ReputationType.Techs;
                break;
            case WARRIOR:
                repType = GameManager.ReputationType.Warriors;
                break;
            default:
                Debug.LogError("INVALID REPUTATION TYPE!");
                return;
        }
        Managers.G_MAN.ChangeReputation(repType, isRemoval ? -1 : 1);
    }

    /******
     * *****
     * ****** SHUFFLE_DECK
     * *****
     *****/
    public void ShuffleDeck(HeroManager hero, bool playSound = true)
    {
        if (hero == Managers.P_MAN) Managers.P_MAN.CurrentDeck.Shuffle();
        else if (hero == Managers.EN_MAN)
        {
            Managers.EN_MAN.CurrentDeck.Shuffle();
            playSound = false;
        }
        else
        {
            Debug.LogError("INVALID HERO!");
            return;
        }

        if (playSound) Managers.AU_MAN.StartStopSound("SFX_ShuffleDeck");
    }

    /******
     * *****
     * ****** UPDATE_DECK
     * *****
     *****/
    public void UpdateDeck(HeroManager hero)
    {
        hero.CurrentDeck.Clear();
        foreach (var card in hero.DeckList)
            hero.CurrentDeck.Add(NewCardInstance(card));
        hero.CurrentDeck.Shuffle();
    }

    /******
     * *****
     * ****** GET_SPECIAL_TRIGGER
     * *****
     *****/
    public static bool GetAbility(GameObject unitCard, string ability)
    {
        if (unitCard == null)
        {
            Debug.LogError("CARD IS NULL!");
            return false;
        }
        if (!unitCard.TryGetComponent(out UnitCardDisplay ucd))
        {
            Debug.LogError("TARGET IS NOT UNIT!");
            return false;
        }

        int abilityIndex = ucd.CurrentAbilities.FindIndex(x => x.AbilityName == ability);
        return abilityIndex != -1;
    }
    public int GetPositiveKeywords(GameObject unitCard)
    {
        if (unitCard == null)
        {
            Debug.LogError("CARD IS NULL!");
            return 0;
        }
        if (!unitCard.TryGetComponent(out UnitCardDisplay _))
        {
            Debug.LogError("TARGET IS NOT UNIT!");
            return 0;
        }

        int positiveKeywords = 0;
        foreach (string positiveKeyword in PositiveAbilities)
            if (GetAbility(unitCard, positiveKeyword)) positiveKeywords++;

        return positiveKeywords;
    }
    /******
     * *****
     * ****** GET_TRIGGER
     * *****
     *****/
    public static bool GetTrigger(GameObject unitCard, string triggerName)
    {
        if (unitCard == null)
        {
            Debug.LogError("CARD IS NULL!");
            return false;
        }
        if (!unitCard.TryGetComponent(out UnitCardDisplay ucd))
        {
            Debug.LogError("TARGET IS NOT UNIT!");
            return false;
        }

        foreach (var ca in ucd.CurrentAbilities)
            if (ca is TriggeredAbility tra)
                if (tra.AbilityTrigger.AbilityName == triggerName)
                {
                    if (tra.TriggerLimit != 0 && tra.TriggerCount >= tra.TriggerLimit) continue;
                    return true;
                }
        return false;
    }
    public static bool GetModifier(GameObject unitCard, ModifierAbility.TriggerType triggerType)
    {
        if (unitCard == null)
        {
            Debug.LogError("CARD IS NULL!");
            return false;
        }
        if (!unitCard.TryGetComponent(out UnitCardDisplay ucd))
        {
            Debug.LogError("TARGET IS NOT UNIT!");
            return false;
        }

        foreach (var ca in ucd.CurrentAbilities)
            if (ca is ModifierAbility ma)
                if (ma.SpecialTriggerType == triggerType) return true;
                /* Unnecessary, method only used to check if enemies have 'enemy hero Wounded' triggers
                {
                    if (ma.TriggerLimit != 0 && ma.TriggerCount >= ma.TriggerLimit) continue;
                    return true;
                }
                */
        return false;
    }
    /******
     * *****
     * ****** TRIGGER_UNIT_ABILITY
     * *****
     *****/
    public bool TriggerUnitAbility(GameObject unitCard, string triggerName)
    {
        if (unitCard == null)
        {
            Debug.LogWarning("CARD IS NULL!");
            return false;
        }
        if (!unitCard.TryGetComponent(out UnitCardDisplay ucd))
        {
            Debug.LogError("TARGET IS NOT UNIT CARD!");
            return false;
        }

        bool effectFound = false;
        int totalAbilities = 0;
        List<TriggeredAbility> resolveFirstAbilities = new();
        List<TriggeredAbility> resolveSecondAbilities = new();
        List<TriggeredAbility> resolveLastAbilities = new();

        foreach (var ca in ucd.CurrentAbilities.AsEnumerable().Reverse()) // Resolve abilities in top-down order
            if (ca is TriggeredAbility tra)
                if (tra.AbilityTrigger.AbilityName == triggerName)
                {
                    if (tra.TriggerLimit != 0 && tra.TriggerCount >= tra.TriggerLimit) continue;
                    tra.TriggerCount++;

                    Debug.Log("TRIGGER! <" + triggerName + ">");
                    effectFound = true;
                    int additionalTriggers = Managers.EF_MAN.TriggerModifiers_TriggerAbility(triggerName, unitCard);
                    int totalTriggers = 1 + additionalTriggers;

                    List<TriggeredAbility> targetList;
                    if (IsResolveLastAbility(tra)) targetList = resolveLastAbilities;
                    else if (IsResolveSecondAbility(tra)) targetList = resolveSecondAbilities;
                    else targetList = resolveFirstAbilities;

                    for (int i = 0; i < totalTriggers; i++)
                    {
                        targetList.Add(tra);
                        totalAbilities++;
                    }
                }

        List<string> enabledTriggers = new();

        foreach (var ca in ucd.CurrentAbilities)
            if (ca is TriggeredAbility tra &&
                (tra.TriggerLimit == 0 || tra.TriggerCount < tra.TriggerLimit))
                enabledTriggers.Add(tra.AbilityTrigger.AbilityName);

        foreach (var ca in ucd.CurrentAbilities)
            if (ca is TriggeredAbility tra &&
                (enabledTriggers.FindIndex(x => x == tra.AbilityTrigger.AbilityName) == -1))
                ucd.EnableTriggerIcon(tra.AbilityTrigger, false);

        float delay = 0.25f;
        int currentAbility = 0;
        ResolveAbilities(resolveLastAbilities);
        ResolveAbilities(resolveSecondAbilities);
        ResolveAbilities(resolveFirstAbilities);
        return effectFound;

        void ResolveAbilities(List<TriggeredAbility> abilities)
        {
            if (++currentAbility == totalAbilities) delay = 0;

            foreach (var tra in abilities)
            {
                Managers.EV_MAN.NewDelayedAction(() =>
                TriggerAbility(tra), delay, true);
            }
        }
        bool IsResolveSecondAbility(TriggeredAbility tra)
        {
            foreach (var eg in tra.EffectGroupList)
            {
                foreach (var e in eg.Effects)
                {
                    if (eg.Targets.TargetsSelf &&
                        (e is DamageEffect || e is DestroyEffect)) return true;
                }
            }
            return false;
        }
        bool IsResolveLastAbility(TriggeredAbility tra)
        {
            foreach (var eg in tra.EffectGroupList)
            {
                foreach (var e in eg.Effects)
                {
                    if (eg.Targets.TargetsSelf &&
                        e is ChangeControlEffect) return true;
                }
            }
            return false;
        }
        void TriggerAbility(TriggeredAbility tra)
        {
            if (unitCard == null)
            {
                Debug.LogWarning("UNIT IS NULL!");
                return;
            }
            Managers.EF_MAN.StartEffectGroupList(tra.EffectGroupList, unitCard, triggerName);
        }
    }
    /******
     * *****
     * ****** TRIGGER_PLAYED_UNITS
     * *****
     *****/
    public void TriggerPlayedUnits(string triggerName, HeroManager hero)
    {
        if (hero == null)
        {
            Debug.LogError("HERO IS NULL!");
            return;
        }

        foreach (var unit in hero.PlayZoneCards.AsEnumerable().Reverse()) // Trigger units in played order
        {
            var ucd = unit.GetComponent<UnitCardDisplay>();

            if (triggerName == TRIGGER_TURN_START)
            {
                int poisonValue = 0;
                foreach (var ca in ucd.CurrentAbilities)
                    if (ca.AbilityName == ABILITY_POISONED) poisonValue++;

                if (poisonValue > 0) Managers.EV_MAN.NewDelayedAction(() =>
                SchedulePoisonEffect(unit, poisonValue), 0, true);

            }

            if (triggerName == TRIGGER_TURN_END)
            {
                foreach (var ca in ucd.CurrentAbilities)
                    if (ca.AbilityName == ABILITY_REGENERATION)
                    {
                        Managers.EV_MAN.NewDelayedAction(() =>
                        ScheduleRegenerationEffect(unit), 0, true);
                        break;
                    }
            }

            if (!GetTrigger(unit, triggerName)) continue;

            Managers.EV_MAN.NewDelayedAction(() =>
            TriggerUnitAbility(unit, triggerName), 0.25f, true);
        }

        if (hero == Managers.EN_MAN)
        {
            var ehp = Managers.EN_MAN.HeroScript.CurrentHeroPower as EnemyHeroPower;
            if (ehp != null && ehp.PowerTrigger.AbilityName == triggerName)
                Managers.EV_MAN.NewDelayedAction(() =>
                Managers.EN_MAN.UseHeroPower(), 0.5f, true);
        }

        void ScheduleRegenerationEffect(GameObject unit)
        {
            if (unit != null)
            {
                var ucd = unit.GetComponent<UnitCardDisplay>();
                if (ucd.CurrentHealth > 0 && ucd.CurrentHealth < ucd.MaxHealth)
                    Managers.EV_MAN.NewDelayedAction(() => RegenerationEffect(unit), 0.5f, true);
            }
        }
        void SchedulePoisonEffect(GameObject unit, int poisonValue)
        {
            if (unit != null)
            {
                var ucd = unit.GetComponent<UnitCardDisplay>();
                if (ucd.CurrentHealth > 0)
                    Managers.EV_MAN.NewDelayedAction(() =>
                    PoisonEffect(unit, poisonValue), 0.5f, true);
            }
        }
        void RegenerationEffect(GameObject unit)
        {
            var healEffect = ScriptableObject.CreateInstance<HealEffect>();
            healEffect.HealFully = true;

            var ucd = unit.GetComponent<UnitCardDisplay>();
            ucd.AbilityTriggerState(ABILITY_REGENERATION);

            Managers.EF_MAN.ResolveEffect(new List<GameObject> { unit },
                healEffect, false, 0, out _, false);
        }
        void PoisonEffect(GameObject unit, int poisonValue)
        {
            var damageEffect = ScriptableObject.CreateInstance<DamageEffect>();
            damageEffect.Value = poisonValue;

            var ucd = unit.GetComponent<UnitCardDisplay>();
            ucd.AbilityTriggerState(ABILITY_POISONED);

            Managers.EF_MAN.ResolveEffect(new List<GameObject> { unit },
                damageEffect, false, 0, out _, false);
        }
    }
    /******
     * *****
     * ****** TRIGGER_TRAP_ABILITIES
     * *****
     *****/
    public void TriggerTrapAbilities(GameObject trappedUnit)
    {
        if (trappedUnit == null || GetAbility(trappedUnit, ABILITY_WARD)) return;

        HeroManager.GetSourceHero(trappedUnit, out HeroManager hMan_Enemy);
        var enemyZoneCards = hMan_Enemy.PlayZoneCards;
        List<GameObject> resolveFirstTraps = new();

        foreach (var trap in enemyZoneCards) // Trigger order doesn't matter, is handled manually
        {
            if (Managers.EF_MAN.UnitsToDestroy.Contains(trap)) continue;

            var ucd = trap.GetComponent<UnitCardDisplay>();
            foreach (var ca in ucd.CurrentAbilities)
                if (ca is TrapAbility trapAbility)
                {
                    if (trapAbility.ResolveLast) TriggerAllEffects(trap);
                    else resolveFirstTraps.Add(trap);
                }
        }

        foreach (var trap in resolveFirstTraps) TriggerAllEffects(trap);

        void TriggerAllEffects(GameObject trap)
        {
            var ucd = trap.GetComponent<UnitCardDisplay>();

            foreach (var ca in ucd.CurrentAbilities)
                if (ca is TrapAbility trapAbility)
                {
                    ucd.AbilityTriggerState(TRIGGER_TRAP);
                    Managers.AU_MAN.StartStopSound(null, ucd.UnitCard.CardPlaySound);
                    Managers.EF_MAN.UnitsToDestroy.Add(trap);
                    Managers.EF_MAN.TriggerModifiers_SpecialTrigger(ModifierAbility.TriggerType.AllyTrapDestroyed, enemyZoneCards);
                    Managers.EV_MAN.NewDelayedAction(() => Managers.EF_MAN.UnitsToDestroy.RemoveAll(unit => unit == null), 0, true);

                    foreach (var selfEffect in trapAbility.SelfEffects)
                        Managers.EV_MAN.NewDelayedAction(() =>
                        TriggerEffect(trap, selfEffect, false, trap), 0, true);

                    foreach (var trapEffect in trapAbility.TrapEffects)
                        Managers.EV_MAN.NewDelayedAction(() =>
                        TriggerEffect(trappedUnit, trapEffect, true, trap), 0, true);
                }
        }

        void TriggerEffect(GameObject unit, Effect effect, bool shootRay, GameObject source)
        {
            if (unit == null)
            {
                Debug.Log("TRAPPED UNIT IS NULL!");
                return;
            }
            if (source == null)
            {
                Debug.LogError("SOURCE TRAP IS NULL!");
                return;
            }

            Managers.EV_MAN.NewDelayedAction(() =>
            Managers.EF_MAN.ResolveEffect(new List<GameObject>
            { unit }, effect, shootRay, 0, out _, false, source), 0.5f, true);
        }
    }
    #endregion
    #endregion
}