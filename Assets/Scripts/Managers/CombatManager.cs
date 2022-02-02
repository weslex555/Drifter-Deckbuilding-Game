using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    /* SINGELTON_PATTERN */
    public static CombatManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    [SerializeField] private GameObject dragArrowPrefab;
    [SerializeField] private GameObject cardContainerPrefab;

    private GameManager gMan;
    private CardManager caMan;
    private AudioManager auMan;
    private EffectManager efMan;
    private EventManager evMan;
    private UIManager uMan;
    private AnimationManager anMan;
    private PlayerManager pMan;
    private EnemyManager enMan;
    private const string PLAYER = GameManager.PLAYER;
    private const string ENEMY = GameManager.ENEMY;

    private int actionsPlayedThisTurn;
    private int lastCardIndex;
    private int lastContainerIndex;

    public const string CARD_ZONE = "CardZone";

    public const string PLAYER_CARD = "PlayerCard";
    public const string PLAYER_HERO = "PlayerHero";
    public const string PLAYER_HAND = "PlayerHand";
    public const string PLAYER_ZONE = "PlayerZone";
    public const string PLAYER_ACTION_ZONE = "PlayerActionZone";
    public const string PLAYER_DISCARD = "PlayerDiscard";

    public const string ENEMY_CARD = "EnemyCard";
    public const string ENEMY_HERO = "EnemyHero";
    public const string ENEMY_HAND = "EnemyHand";
    public const string ENEMY_ZONE = "EnemyZone";
    public const string ENEMY_DISCARD = "EnemyDiscard";

    public GameObject DragArrowPrefab { get => dragArrowPrefab; }
    public int ActionsPlayedThisTurn
    {
        get => actionsPlayedThisTurn;
        set
        {
            actionsPlayedThisTurn = value;
            if (pMan.IsMyTurn && actionsPlayedThisTurn == 1)
            {
                evMan.NewDelayedAction(() =>
                caMan.TriggerPlayedUnits(CardManager.TRIGGER_SPARK), 0);
            }
        }
    }

    /* CARD LISTS */
    public List<List<GameObject>> RevealedCardLists { get; private set; } // TESTING
    public List<GameObject> PlayerHandCards { get; private set; }
    public List<GameObject> PlayerZoneCards { get; private set; }
    public List<GameObject> PlayerActionZoneCards { get; private set; }
    public List<Card> PlayerDiscardCards { get; private set; }
    public List<GameObject> EnemyHandCards { get; private set; }
    public List<GameObject> EnemyZoneCards { get; private set; }
    public List<Card> EnemyDiscardCards { get; private set; }
    
    /* GAME ZONES */
    public GameObject CardZone { get; private set; }
    public GameObject PlayerHero { get; private set; }
    public GameObject PlayerHand { get; private set; }
    public GameObject PlayerZone { get; private set; }
    public GameObject PlayerActionZone { get; private set; }
    public GameObject PlayerDiscard { get; private set; }
    public GameObject EnemyHero { get; private set; }
    public GameObject EnemyHand { get; private set; }
    public GameObject EnemyZone { get; private set; }
    public GameObject EnemyDiscard { get; private set; }

    private void Start()
    {
        gMan = GameManager.Instance;
        caMan = CardManager.Instance;
        auMan = AudioManager.Instance;
        efMan = EffectManager.Instance;
        evMan = EventManager.Instance;
        uMan = UIManager.Instance;
        anMan = AnimationManager.Instance;
        pMan = PlayerManager.Instance;
        enMan = EnemyManager.Instance;
    }

    /******
     * *****
     * ****** START_COMBAT_SCENE
     * *****
     *****/
    public void StartCombatScene()
    {
        // GAME ZONES
        CardZone = GameObject.Find(CARD_ZONE);
        PlayerActionZone = GameObject.Find(PLAYER_ACTION_ZONE);
        PlayerHand = GameObject.Find(PLAYER_HAND);
        PlayerZone = GameObject.Find(PLAYER_ZONE);
        PlayerDiscard = GameObject.Find(PLAYER_DISCARD);
        PlayerHero = GameObject.Find(PLAYER_HERO);
        EnemyHand = GameObject.Find(ENEMY_HAND);
        EnemyZone = GameObject.Find(ENEMY_ZONE);
        EnemyDiscard = GameObject.Find(ENEMY_DISCARD);
        EnemyHero = GameObject.Find(ENEMY_HERO);
        // ZONE LISTS
        PlayerHandCards = new List<GameObject>();
        PlayerZoneCards = new List<GameObject>();
        PlayerActionZoneCards = new List<GameObject>();
        PlayerDiscardCards = new List<Card>();
        EnemyHandCards = new List<GameObject>();
        EnemyZoneCards = new List<GameObject>();
        EnemyDiscardCards = new List<Card>();
        // REVEALED CARD LISTS
        RevealedCardLists = new List<List<GameObject>>
        {
            PlayerHandCards,
            PlayerZoneCards,
            EnemyHandCards,
            EnemyZoneCards
        };
        uMan.SelectTarget(PlayerHero, false);
        uMan.SelectTarget(EnemyHero, false);
    }

    public UnitCardDisplay GetUnitDisplay(GameObject card)
        => card.GetComponent<CardDisplay>() as UnitCardDisplay;

    public bool IsUnitCard(GameObject target) => 
        target.TryGetComponent<UnitCardDisplay>(out _);

    public enum DisplayType
    {
        Default,
        HeroSelect,
        NewCard,
        Cardpage
    }

    /******
     * *****
     * ****** SHOW_CARD
     * *****
     *****/
    public GameObject ShowCard(Card card, Vector2 position, DisplayType type = DisplayType.Default)
    {
        if (card == null)
        {
            Debug.LogError("CARD IS NULL!");
            return null;
        }
        
        GameObject prefab = null;
        if (card is UnitCard)
        {
            prefab = caMan.UnitCardPrefab;
            if (type is DisplayType.NewCard)
                prefab = prefab.GetComponent<CardZoom>().UnitZoomCardPrefab;
        }
        else if (card is ActionCard)
        {
            prefab = caMan.ActionCardPrefab;
            if (type is DisplayType.NewCard)
                prefab = prefab.GetComponent<CardZoom>().ActionZoomCardPrefab;
        }

        GameObject parent = CardZone;
        if (parent == null) parent = uMan.CurrentCanvas;

        if (parent == null) Debug.LogError("PARENT IS NULL!");

        prefab = Instantiate(prefab, parent.transform);
        prefab.transform.position = position;
        CardDisplay cd = prefab.GetComponent<CardDisplay>();

        if (type is DisplayType.Default)
        {
            cd.CardScript = card;
            cd.CardContainer = Instantiate(cardContainerPrefab, uMan.CurrentCanvas.transform);
            cd.CardContainer.transform.position = position;
            CardContainer cc = cd.CardContainer.GetComponent<CardContainer>();
            cc.Child = prefab;
        }
        else if (type is DisplayType.HeroSelect) cd.CardScript = card; // TESTING
        else if (type is DisplayType.NewCard) cd.DisplayZoomCard(null, card);
        else if (type is DisplayType.Cardpage) cd.DisplayCardPageCard(card);
        return prefab;
    }

    /******
     * *****
     * ****** HIDE_CARD
     * *****
     *****/
    private Card HideCard(GameObject card, List<GameObject> currentZoneList)
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
    public void DrawCard(string hero)
    {
        List<Card> deck;
        List<GameObject> hand;
        string cardTag;
        string cardZone;
        Vector2 position = new Vector2();

        if (hero == PLAYER)
        {
            deck = pMan.CurrentPlayerDeck;
            hand = PlayerHandCards;
            if (hand.Count >= GameManager.MAX_HAND_SIZE)
            {
                uMan.CreateFleetingInfoPopup("Your hand is full!");
                return;
            }
            cardTag = PLAYER_CARD;
            cardZone = PLAYER_HAND;
            position.Set(-750, -350);
        }
        else if (hero == ENEMY)
        {
            deck = enMan.CurrentEnemyDeck;
            hand = EnemyHandCards;
            if (hand.Count >= GameManager.MAX_HAND_SIZE)
            {
                uMan.CreateFleetingInfoPopup("Enemy hand is full!");
                return;
            }
            cardTag = ENEMY_CARD;
            cardZone = ENEMY_HAND;
            position.Set(685, 370);
        }
        else
        {
            Debug.LogError("PLAYER <" + hero + "> NOT FOUND!");
            return;
        }

        // Shuffle discard into deck
        if (deck.Count < 1)
        {
            List<Card> discard;
            if (hero == PLAYER) discard = PlayerDiscardCards;
            else if (hero == ENEMY) discard = EnemyDiscardCards;
            else
            {
                Debug.LogError("PLAYER <" + hero + "> NOT FOUND!");
                return;
            }
            if (discard.Count < 1)
            {
                Debug.LogWarning("DISCARD IS EMPTY!");
                uMan.CreateFleetingInfoPopup("No cards left!");
                return;
            }
            foreach (Card c in discard) deck.Add(c);
            discard.Clear();
            caMan.ShuffleDeck(hero);
        }

        GameObject card = ShowCard(deck[0], position);
        if (card == null)
        {
            Debug.LogError("CARD IS NULL!");
            return;
        }
        deck.RemoveAt(0);
        card.tag = cardTag;
        ChangeCardZone(card, cardZone);

        if (hero == PLAYER)
        {
            PlayerHandCards.Add(card);
            // Added to NewDrawnCards for upcoming effects in the current effect group
            // Added to CurrentLegalTargets for the current effect in the current effect group
            efMan.NewDrawnCards.Add(card);
            
            if (uMan.PlayerIsTargetting &&
                efMan.CurrentEffect is DrawEffect de &&
                de.IsDiscardEffect) // TESTING
            {
                if (!efMan.CurrentLegalTargets.Contains(card))
                    efMan.CurrentLegalTargets.Add(card);
                uMan.SelectTarget(card, true);
            }
        }
        else EnemyHandCards.Add(card);
        auMan.StartStopSound("SFX_DrawCard");
    }

    /******
     * *****
     * ****** CHANGE_CARD_ZONE
     * *****
     *****/
    public void ChangeCardZone(GameObject card, string newZoneName, bool returnToIndex = false)
    {
        uMan.SelectTarget(card, false); // Unnecessary?
        GameObject newZone = null;

        switch (newZoneName)
        {
            // PLAYER
            case PLAYER_HAND:
                newZone = PlayerHand;
                anMan.RevealedHandState(card);
                break;
            case PLAYER_ZONE:
                newZone = PlayerZone;
                anMan.PlayedState(card);
                break;
            case PLAYER_ACTION_ZONE:
                newZone = PlayerActionZone;
                anMan.RevealedHandState(card);
                break;
            // ENEMY
            case ENEMY_HAND:
                newZone = EnemyHand;
                anMan.RevealedPlayState(card);
                break;
            case ENEMY_ZONE:
                newZone = EnemyZone;
                anMan.PlayedState(card);
                break;
        }

        CardDisplay cd = card.GetComponent<CardDisplay>();

        
        if (!returnToIndex)
        {
            lastCardIndex = card.GetComponent<DragDrop>().LastIndex; // TESTING
            lastContainerIndex = cd.CardContainer.transform.GetSiblingIndex(); // TESTING
        }

        cd.CardContainer.GetComponent<CardContainer>().MoveContainer(newZone);

        if (returnToIndex)
        {
            card.transform.SetSiblingIndex(lastCardIndex); // TESTING
            cd.CardContainer.transform.SetSiblingIndex(lastContainerIndex); // TESTING
        }

        if (cd is UnitCardDisplay ucd)
        {
            bool played = false;
            if (newZoneName == PLAYER_ZONE || newZoneName == ENEMY_ZONE) played = true;
            ucd.ResetUnitCard(played);
        }
        else card.GetComponent<DragDrop>().IsPlayed = false;
    }

    /******
     * *****
     * ****** PLAY_CARD [HAND >>> PLAY]
     * *****
     *****/
    public void PlayCard(GameObject card)
    {
        CardDisplay cd = card.GetComponent<CardDisplay>();
        CardContainer container = cd.CardContainer.GetComponent<CardContainer>();

        // PLAYER
        if (card.CompareTag(PLAYER_CARD))
        {
            pMan.EnergyLeft -= cd.CurrentEnergyCost;
            PlayerHandCards.Remove(card);

            if (cd is UnitCardDisplay)
            {
                PlayerZoneCards.Add(card);
                ChangeCardZone(card, PLAYER_ZONE);
                PlayUnit();
            }
            else if (cd is ActionCardDisplay)
            {
                PlayerActionZoneCards.Add(card);
                ChangeCardZone(card, PLAYER_ACTION_ZONE);
                container.OnAttachAction = () => PlayAction(); // TESTING
            }
            else
            {
                Debug.LogError("CARD DISPLAY TYPE NOT FOUND!");
                return;
            }
        }
        // ENEMY
        else if (card.CompareTag(ENEMY_CARD))
        {
            if (EnemyZoneCards.Count >= GameManager.MAX_UNITS_PLAYED) return;

            EnemyHandCards.Remove(card);
            if (IsUnitCard(card))
            {
                EnemyZoneCards.Add(card);
                ChangeCardZone(card, ENEMY_ZONE);
                PlayUnit();
            }
            else
            {
                Debug.LogError("CARD DISPLAY TYPE NOT FOUND!");
                return;
            }
        }
        else
        {
            Debug.LogError("CARD TAG NOT FOUND!");
            return;
        }

        void PlayUnit()
        {
            if (card.CompareTag(PLAYER_CARD)) // TESTING
            {
                if (!caMan.TriggerUnitAbility(card, CardManager.TRIGGER_PLAY))
                    efMan.TriggerGiveNextEffect(card);
            }
            PlayCardSound();
            PlayAbilitySounds();
        }
        void PlayAction()
        {
            auMan.StartStopSound("SFX_PlayCard");
            ResolveActionCard(card);
        }
        void PlayCardSound()
        {
            Sound playSound = cd.CardScript.CardPlaySound;
            if (playSound.clip == null) Debug.LogWarning("MISSING PLAY SOUND: " + cd.CardName);
            else auMan.StartStopSound(null, playSound);
        }
        void PlayAbilitySounds()
        {
            float delay = 0.3f;
            UnitCardDisplay ucd = cd as UnitCardDisplay;
            foreach (CardAbility ca in ucd.CurrentAbilities)
            {
                if (ca is StaticAbility sa)
                {
                    FunctionTimer.Create(() =>
                    auMan.StartStopSound(null, sa.GainAbilitySound), delay);
                }
                delay += 0.3f;
            }
        }
    }

    /******
     * *****
     * ****** RESOLVE_ACTION_CARD
     * *****
     *****/
    private void ResolveActionCard(GameObject card)
    {
        List<EffectGroup> groupList = 
            card.GetComponent<ActionCardDisplay>().ActionCard.EffectGroupList;
        efMan.StartEffectGroupList(groupList, card);
    }

    /******
     * *****
     * ****** DISCARD_CARD [HAND/ACTION_ZONE >>> DISCARD]
     * *****
     *****/
    public void DiscardCard(GameObject card, bool isAction = false)
    {
        if (isAction) PlayerActionZoneCards.Remove(card);
        if (card.CompareTag(PLAYER_CARD))
        {
            if (!isAction) PlayerHandCards.Remove(card);
            PlayerDiscardCards.Add(HideCard(card, PlayerHandCards)); // TESTING
        }
        else
        {
            EnemyHandCards.Remove(card);
            EnemyDiscardCards.Add(HideCard(card, EnemyHandCards)); // TESTING
        }
        if (!isAction) auMan.StartStopSound("SFX_DiscardCard");
    }

    /******
     * *****
     * ****** REFRESH_UNITS
     * *****
     *****/
    public void RefreshUnits(string hero)
    {
        List<GameObject> cardZoneList = null;
        if (hero == PLAYER) cardZoneList = PlayerZoneCards;
        else if (hero == ENEMY) cardZoneList = EnemyZoneCards;
        foreach (GameObject card in cardZoneList)
            GetUnitDisplay(card).IsExhausted = false;
    }

    /******
     * *****
     * ****** IS_PLAYABLE
     * *****
     *****/
    public bool IsPlayable(GameObject card)
    {
        CardDisplay display = card.GetComponent<CardDisplay>();
        int actionCost = display.CurrentEnergyCost;
        int playerActions = pMan.EnergyLeft;

        if (display is UnitCardDisplay)
        {
            if (PlayerZoneCards.Count >= GameManager.MAX_UNITS_PLAYED)
            {
                uMan.CreateFleetingInfoPopup("Too many units!");
                ErrorSound();
                return false;
            }
        }
        else if (display is ActionCardDisplay acd)
            if (!efMan.CheckLegalTargets(acd.ActionCard.EffectGroupList, card, true))
            {
                uMan.CreateFleetingInfoPopup("You can't play that right now!");
                ErrorSound();
                return false;
            }
        if (playerActions < actionCost)
        {
            uMan.CreateFleetingInfoPopup("Not enough energy!");
            ErrorSound();
            return false;
        }
        return true;
        void ErrorSound() => auMan.StartStopSound("SFX_Error");
    }

    /******
     * *****
     * ****** CAN_ATTACK
     * *****
     *****/
    public bool CanAttack(GameObject attacker, GameObject defender, bool preCheck)
    {
        if (defender != null)
        {
            if (attacker.CompareTag(defender.tag)) return false;
            if (attacker.CompareTag(PLAYER_CARD)) 
                if (defender == PlayerHero) return false;
        }
        else
        {
            if (!preCheck)
            {
                Debug.LogError("DEFENDER IS NULL!");
                return false;
            }
        }

        UnitCardDisplay atkUcd = GetUnitDisplay(attacker);
        if (atkUcd.IsExhausted)
        {
            if (preCheck)
                uMan.CreateFleetingInfoPopup("<b>Exhausted</b> units can't attack!");
            return false;
        }
        else if (atkUcd.CurrentPower < 1)
        {
            if (preCheck)
                uMan.CreateFleetingInfoPopup("Units with 0 power can't attack!");
            return false;
        }

        if (defender == null && preCheck) return true; // For StartDrag in DragDrop
        
        if (defender.TryGetComponent(out UnitCardDisplay defUcd))
        {
            if (EnemyHandCards.Contains(defender)) return false; // Unnecessary, already checked in CardSelect
            if (defUcd.CurrentHealth < 1) return false; // Destroyed units that haven't left play yet
            if (CardManager.GetAbility(defender, CardManager.ABILITY_STEALTH))
            {
                if (!preCheck)
                    uMan.CreateFleetingInfoPopup("Units with <b>Stealth</b> can't be attacked!");
                return false;
            }
        }
        return true;
    }

    /******
     * *****
     * ****** ATTACK
     * *****
     *****/
    public void Attack(GameObject attacker, GameObject defender)
    {
        GetUnitDisplay(attacker).IsExhausted = true;
        if (CardManager.GetAbility(attacker, CardManager.ABILITY_STEALTH))
            GetUnitDisplay(attacker).RemoveCurrentAbility(CardManager.ABILITY_STEALTH);
        
        if (!CardManager.GetAbility(attacker, CardManager.ABILITY_RANGED))
            anMan.UnitAttack(attacker, defender, IsUnitCard(defender));
        else
        {
            PlayAttackSound(attacker); // TESTING
            efMan.CreateEffectRay(attacker.transform.position, defender,
                () => Strike(attacker, defender, true), 0, false);
            pMan.IsMyTurn = false; // TESTING
        }
    }

    public void PlayAttackSound(GameObject unitCard)
    {
        bool isMeleeAttack = true;
        if (CardManager.GetAbility(unitCard, CardManager.ABILITY_RANGED))
            isMeleeAttack = false;

        string attackSound;
        if (GetUnitDisplay(unitCard).CurrentPower < 5)
        {
            if (isMeleeAttack) attackSound = "SFX_AttackMelee";
            else attackSound = "SFX_AttackRanged";
        }
        else
        {
            if (isMeleeAttack) attackSound = "SFX_AttackMelee_Heavy";
            else attackSound = "SFX_AttackRanged_Heavy";
        }
        auMan.StartStopSound(attackSound);
    }

    /******
     * *****
     * ****** STRIKE
     * *****
     *****/
    public void Strike(GameObject striker, GameObject defender, bool isCombat)
    {
        bool strikerDestroyed;
        //bool defenderDealtDamage; // No current use

        // COMBAT
        if (isCombat)
        {
            DealDamage(striker, defender, 
                out bool strikerDealtDamage, out bool defenderDestroyed);

            // delay
            if (IsUnitCard(defender))
            {
                if (!CardManager.GetAbility(striker, CardManager.ABILITY_RANGED))
                    DealDamage(defender, striker, out _, out strikerDestroyed);
                else
                {
                    //defenderDealtDamage = false;
                    strikerDestroyed = false;
                }

                if (!strikerDestroyed && CardManager.GetTrigger
                    (striker, CardManager.TRIGGER_DEATHBLOW))
                {
                    if (defenderDestroyed) DeathblowTrigger(striker);
                }
                if (!defenderDestroyed && CardManager.GetTrigger
                    (defender, CardManager.TRIGGER_DEATHBLOW))
                {
                    if (strikerDestroyed) DeathblowTrigger(defender);
                }
            }
            else if (!defenderDestroyed && strikerDealtDamage)
            {
                List<GameObject> targetZoneList;
                if (striker.CompareTag(PLAYER_CARD)) targetZoneList = EnemyZoneCards;
                else targetZoneList = PlayerZoneCards;

                foreach (GameObject unit in targetZoneList)
                {
                    if (CardManager.GetTrigger(unit, CardManager.TRIGGER_RETALIATE)) RetaliateTrigger(unit);
                }

                // Trigger Infiltrate BEFORE Retaliate, can cause Retaliate sources to be destroyed before triggering.
                if (CardManager.GetTrigger(striker, CardManager.TRIGGER_INFILTRATE)) InfiltrateTrigger(striker);
            }
        }

        /*
        // STRIKE EFFECTS // Currently non-existent
        else
        {
            DealDamage(striker, defender,
                out bool attackerDealtDamage, out bool defenderDestroyed);

            // delay
            if (IsUnitCard(defender))
            {
                if (defenderDestroyed &&
                CardManager.GetTrigger(striker, CardManager.TRIGGER_DEATHBLOW))
                    DeathblowTrigger(striker);
            }
            else if (attackerDealtDamage &&
                CardManager.GetTrigger(striker, CardManager.TRIGGER_INFILTRATE))
                InfiltrateTrigger(striker);
        }
        */

        void DealDamage(GameObject striker, GameObject defender,
            out bool dealtDamage, out bool defenderDestroyed)
        {
            int power = GetUnitDisplay(striker).CurrentPower;
            if (power < 1)
            {
                dealtDamage = false;
                defenderDestroyed = false;
                return;
            }

            if (IsUnitCard(defender))
            {
                if (!CardManager.GetAbility(defender, CardManager.ABILITY_FORCEFIELD))
                    dealtDamage = true;
                else dealtDamage = false;
            }
            else dealtDamage = true;

            if (TakeDamage(defender, power))
                defenderDestroyed = true;
            else defenderDestroyed = false;
        }
        
        void DelayTrigger(System.Action action) =>
            evMan.NewDelayedAction(action, 0, true);

        void InfiltrateTrigger(GameObject unit) =>
            DelayTrigger(() => caMan.TriggerUnitAbility(unit, CardManager.TRIGGER_INFILTRATE));
        
        void DeathblowTrigger(GameObject unit) => 
            DelayTrigger(() => caMan.TriggerUnitAbility(unit, CardManager.TRIGGER_DEATHBLOW));

        void RetaliateTrigger(GameObject unit) =>
            DelayTrigger(() => caMan.TriggerUnitAbility(unit, CardManager.TRIGGER_RETALIATE));
    }

    /******
     * *****
     * ****** TAKE_DAMAGE
     * *****
     *****/
    public bool TakeDamage(GameObject target, int damageValue)
    {
        if (damageValue < 1) return false;
        int targetValue;
        int newTargetValue;
        if (target == PlayerHero) targetValue = pMan.PlayerHealth;
        else if (target == EnemyHero) targetValue = enMan.EnemyHealth;
        else targetValue = GetUnitDisplay(target).CurrentHealth;

        if (targetValue < 1) return false; // Don't deal damage to targets with 0 health
        newTargetValue = targetValue - damageValue;

        // Damage to heroes
        if (target == PlayerHero)
        {
            pMan.PlayerHealth = newTargetValue;
            anMan.ModifyHeroHealthState(target);
        }
        else if (target == EnemyHero)
        {
            enMan.EnemyHealth = newTargetValue;
            anMan.ModifyHeroHealthState(target);
        }
        // Damage to Units
        else
        {
            if (CardManager.GetAbility(target, CardManager.ABILITY_FORCEFIELD))
            {
                GetUnitDisplay(target).AbilityTriggerState(CardManager.ABILITY_FORCEFIELD);
                GetUnitDisplay(target).RemoveCurrentAbility(CardManager.ABILITY_FORCEFIELD);
                return false;
            }
            else
            {
                GetUnitDisplay(target).CurrentHealth = newTargetValue;
                anMan.UnitTakeDamageState(target);
            }
        }
        if (newTargetValue < 1)
        {
            if (IsUnitCard(target)) DestroyUnit(target);
            else
            {
                anMan.SetAnimatorBool(target, "IsDestroyed", true);
                bool playerWins;
                if (target == PlayerHero) playerWins = false;
                else playerWins = true;
                gMan.EndCombat(playerWins);
            }
            return true;
        }
        else return false;
    }

    /******
     * *****
     * ****** HEAL_DAMAGE
     * *****
     *****/
    public void HealDamage(GameObject target, int healingValue)
    {
        if (healingValue < 1) return;
        int targetValue;
        int maxValue;
        int newTargetValue;
        if (target == PlayerHero)
        {
            targetValue = pMan.PlayerHealth;
            maxValue = pMan.MaxPlayerHealth;
        }
        else if (target == EnemyHero)
        {
            targetValue = enMan.EnemyHealth;
            maxValue = enMan.MaxEnemyHealth;
        }
        else
        {
            targetValue = GetUnitDisplay(target).CurrentHealth;
            maxValue = GetUnitDisplay(target).MaxHealth;
        }

        if (targetValue < 1) return; // Don't heal destroyed units or heroes
        newTargetValue = targetValue + healingValue;
        if (newTargetValue > maxValue) newTargetValue = maxValue;

        if (target == PlayerHero) pMan.PlayerHealth = newTargetValue;
        else if (target == EnemyHero) enMan.EnemyHealth = newTargetValue;
        else GetUnitDisplay(target).CurrentHealth = newTargetValue;
    }

    /******
     * *****
     * ****** IS_DAMAGED
     * *****
     *****/
    public bool IsDamaged(GameObject unitCard)
    {
        UnitCardDisplay ucd = unitCard.GetComponent<UnitCardDisplay>();
        bool isDamaged = false;
        if (ucd.CurrentHealth < ucd.MaxHealth) isDamaged = true;
        return isDamaged;
    }

    /******
     * *****
     * ****** GET_LOWEST_HEALTH_UNIT
     * *****
     *****/
    public GameObject GetLowestHealthUnit(List<GameObject> unitList)
    {
        if (unitList.Count < 1) return null;
        int lowestHealth = 999;
        List<GameObject> lowestHealthUnits = new List<GameObject>();

        foreach (GameObject unit in unitList)
        {
            int health = GetUnitDisplay(unit).CurrentHealth;
            if (health < 1) continue;
            if (health < lowestHealth)
            {
                lowestHealth = health;
                lowestHealthUnits.Clear();
                lowestHealthUnits.Add(unit);
            }
            else if (health == lowestHealth) lowestHealthUnits.Add(unit);
        }
        if (lowestHealthUnits.Count < 1) return null;
        if (lowestHealthUnits.Count > 1)
        {
            int randomIndex = Random.Range(0, lowestHealthUnits.Count - 1);
            return lowestHealthUnits[randomIndex];
        }
        else return lowestHealthUnits[0];
    }

    /******
     * *****
     * ****** DESTROY_UNIT [PLAY >>> DISCARD]
     * *****
     *****/
    public void DestroyUnit(GameObject card)
    {
        if (card == null)
        {
            Debug.LogError("CARD IS NULL!");
            return;
        }

        string cardTag = card.tag;
        DestroyFX();
        evMan.NewDelayedAction(() => Destroy(), 0.5f, true);
        if (HasDestroyTriggers())
            evMan.NewDelayedAction(() => DestroyTriggers(), 0, true);

        bool HasDestroyTriggers()
        {
            if (CardManager.GetTrigger(card,
                CardManager.TRIGGER_REVENGE)) return true;
            if (CardManager.GetAbility(card,
                CardManager.ABILITY_MARKED)) return true;
            return false;
        }
        void DestroyFX()
        {
            if (card == null)
            {
                Debug.LogError("CARD IS NULL!");
                return;
            }
            Sound deathSound = GetUnitDisplay(card).UnitCard.UnitDeathSound;
            AudioManager.Instance.StartStopSound(null, deathSound);
            anMan.DestroyUnitCardState(card);
        }
        void DestroyTriggers()
        {
            if (card == null)
            {
                Debug.LogError("CARD IS NULL!");
                return;
            }
            caMan.TriggerUnitAbility(card, CardManager.TRIGGER_REVENGE);
            if (CardManager.GetAbility(card, CardManager.ABILITY_MARKED))
            {
                GetUnitDisplay(card).AbilityTriggerState(CardManager.ABILITY_MARKED);
                DrawCard(PLAYER);
            }
        }
        void Destroy()
        {
            if (card == null)
            {
                Debug.LogError("CARD IS NULL!");
                return;
            }
            card.GetComponent<CardZoom>().DestroyZoomPopups();
            if (cardTag == PLAYER_CARD)
            {
                PlayerZoneCards.Remove(card);
                PlayerDiscardCards.Add(HideCard(card, PlayerZoneCards));
            }
            else if (cardTag == ENEMY_CARD)
            {
                EnemyZoneCards.Remove(card);
                EnemyDiscardCards.Add(HideCard(card, EnemyZoneCards));
            }
        }
    }
}
