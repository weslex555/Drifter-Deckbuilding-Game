using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    /* SINGELTON_PATTERN */
    public static EffectManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private PlayerManager pMan;
    private EnemyManager enMan;
    private CombatManager coMan;
    private UIManager uMan;
    private AudioManager auMan;
    private AnimationManager anMan;
    private EventManager evMan;

    private GameObject dragArrow;

    private bool effectsResolving;
    private GameObject effectSource;
    private bool isPlayTrigger;
    private int currentEffectGroup;
    private int currentEffect;

    private List<EffectGroup> effectGroupList;
    private List<List<GameObject>> legalTargets;
    private List<List<GameObject>> acceptedTargets;

    private List<GameObject> newDrawnCards;
    private List<GameObject> unitsToDestroy;
    private List<GiveNextUnitEffect> giveNextEffects;

    public bool EffectsResolving // TESTING
    {
        get => effectsResolving;
        private set
        {
            effectsResolving = value;
            evMan.PauseDelayedActions(value);
        }
    }
    public List<GameObject> NewDrawnCards { get => newDrawnCards; }
    public List<GiveNextUnitEffect> GiveNextEffects { get => giveNextEffects; }
    public Effect CurrentEffect
    {
        get
        {
            if (effectGroupList == null)
            {
                Debug.LogError("GROUP LIST IS NULL!");
                return null;
            }
            return effectGroupList[currentEffectGroup].Effects[currentEffect];
        }
    }
    public List<GameObject> CurrentLegalTargets
    {
        get
        {
            if (effectGroupList == null)
            {
                Debug.LogError("GROUP LIST IS NULL!");
                return null;
            }
            return legalTargets[currentEffectGroup];
        }
    }

    private void Start()
    {
        pMan = PlayerManager.Instance;
        enMan = EnemyManager.Instance;
        coMan = CombatManager.Instance;
        uMan = UIManager.Instance;
        auMan = AudioManager.Instance;
        anMan = AnimationManager.Instance;
        evMan = EventManager.Instance;
        newDrawnCards = new List<GameObject>();
        unitsToDestroy = new List<GameObject>();
        giveNextEffects = new List<GiveNextUnitEffect>();
    }

    /******
     * *****
     * ****** IS_PLAYER_SOURCE
     * *****
     *****/
    private bool IsPlayerSource(GameObject source)
    {
        if (source == null)
        {
            Debug.LogError("SOURCE IS NULL!");
            return false;
        }
        if (source.CompareTag(CombatManager.PLAYER_CARD) ||
            source.CompareTag(CombatManager.PLAYER_HERO))
            return true;
        else return false;
    }

    /******
     * *****
     * ****** START_EFFECT_GROUP_LIST
     * *****
     *****/
    public void StartEffectGroupList(List<EffectGroup> groupList, GameObject source, bool isPlayTrigger = true)
    {
        if (source == null)
        {
            Debug.LogError("SOURCE IS NULL!");
            return;
        }

        // WATCH
        if (EffectsResolving) // TESTING
        {
            Debug.LogWarning("GROUP LIST DELAYED!");
            EventManager.Instance.NewDelayedAction(() =>
            StartEffectGroupList(groupList, source, isPlayTrigger), 0.5f, true);
            return;
        }

        EffectsResolving = true; // TESTING
        this.isPlayTrigger = isPlayTrigger; // TESTING
        effectGroupList = new List<EffectGroup>();
        foreach (EffectGroup eg in groupList) effectGroupList.Add(eg);
        effectSource = source;
        currentEffectGroup = 0;
        currentEffect = 0;
        newDrawnCards.Clear();

        if (!CheckLegalTargets(effectGroupList, effectSource))
            AbortEffectGroupList(false);
        else StartNextEffectGroup(true);
    }

    /******
     * *****
     * ****** START_NEXT_EFFECT_GROUP
     * *****
     *****/
    private void StartNextEffectGroup(bool isFirstGroup = false)
    {
        if (!isFirstGroup) currentEffectGroup++;
        if (currentEffectGroup < effectGroupList.Count)
        {
            Debug.Log("[GROUP #" + (currentEffectGroup + 1) +
                "] <" + effectGroupList[currentEffectGroup].ToString() + ">");
            StartNextEffect(true);
        }
        else if (currentEffectGroup == effectGroupList.Count)
            ResolveEffectGroupList();
        else Debug.LogError("EffectGroup > GroupList!");
    }

    /******
     * *****
     * ****** IS_TARGET_EFFECT
     * *****
     *****/
    private bool IsTargetEffect(EffectGroup group, Effect effect)
    {
        if (effect is DrawEffect de && de.IsDiscardEffect) return true;
        else if (effect is DrawEffect || effect is GiveNextUnitEffect) return false;
        else if (group.Targets.TargetsAll || group.Targets.TargetsSelf) return false;
        else if (group.Targets.PlayerHero || group.Targets.EnemyHero)
        {
            if (!group.Targets.PlayerUnit && !group.Targets.EnemyUnit) return false; // TESTING
        }
        
        return true;
    }

    /******
     * *****
     * ****** START_NEXT_EFFECT
     * *****
     *****/
    private void StartNextEffect(bool isFirstEffect = false)
    {
        EffectGroup eg = effectGroupList[currentEffectGroup];
        if (!isFirstEffect) currentEffect++;
        else currentEffect = 0;

        if (currentEffect < eg.Effects.Count)
        {
            if (eg.Effects[currentEffect] == null)
            {
                Debug.LogError("EFFECT IS NULL!");
                return;
            }

            Debug.Log("[EFFECT #" + (currentEffect + 1) + 
                "] <" + eg.Effects[currentEffect].ToString() + ">");
        }
        else if (currentEffect == eg.Effects.Count)
        {
            StartNextEffectGroup();
            return;
        }
        else Debug.LogError("CurrentEffect > Effects!");

        Effect effect = eg.Effects[currentEffect];
        if (IsTargetEffect(eg, effect)) StartTargetEffect(effect);
        else StartNonTargetEffect();
    }
    
    /******
     * *****
     * ****** START_NON_TARGET_EFFECT
     * *****
     *****/
    private void StartNonTargetEffect()
    {
        EffectTargets et = effectGroupList[currentEffectGroup].Targets;
        if (et.TargetsSelf)
        {
            acceptedTargets[currentEffectGroup].Add(effectSource);
            ConfirmNonTargetEffect();
            return;
        }

        if (IsPlayerSource(effectSource)) // TESTING
        {
            if (et.PlayerHero) acceptedTargets[currentEffectGroup].Add(coMan.PlayerHero);
            if (et.EnemyHero) acceptedTargets[currentEffectGroup].Add(coMan.EnemyHero);
            if (et.TargetsAll)
            {
                if (et.PlayerUnit)
                {
                    foreach (GameObject card in coMan.PlayerZoneCards)
                        acceptedTargets[currentEffectGroup].Add(card);
                }
                if (et.EnemyUnit)
                {
                    foreach (GameObject card in coMan.EnemyZoneCards)
                        acceptedTargets[currentEffectGroup].Add(card);
                }
            }
        }
        else
        {
            if (et.EnemyHero) acceptedTargets[currentEffectGroup].Add(coMan.PlayerHero);
            if (et.PlayerHero) acceptedTargets[currentEffectGroup].Add(coMan.EnemyHero);
            if (et.TargetsAll)
            {
                if (et.PlayerUnit)
                {
                    foreach (GameObject card in coMan.EnemyZoneCards)
                        acceptedTargets[currentEffectGroup].Add(card);
                }
                if (et.EnemyUnit)
                {
                    foreach (GameObject card in coMan.PlayerZoneCards)
                        acceptedTargets[currentEffectGroup].Add(card);
                }
            }
        }
        ConfirmNonTargetEffect();
    }

    /******
     * *****
     * ****** START_TARGET_EFFECT
     * *****
     *****/
    private void StartTargetEffect(Effect effect)
    {
        if (acceptedTargets[currentEffectGroup].Count > 0)
        {
            StartNextEffectGroup();
            return;
        }

        uMan.PlayerIsTargetting = true;
        string description = 
            effectGroupList[currentEffectGroup].EffectsDescription;

        if (effect is DrawEffect de && de.IsDiscardEffect)
        {
            description = "Discard a card.";
            anMan.ShiftPlayerHand(true);
            foreach (GameObject newTarget in newDrawnCards)
                legalTargets[currentEffectGroup].Add(newTarget);
            newDrawnCards.Clear();
        }
        else
        {
            if (isPlayTrigger) uMan.SetCancelEffectButton(true);
            if (dragArrow != null)
            {
                Destroy(dragArrow);
                Debug.LogError("DRAG ARROW ALREADY EXISTS!");
            }
            dragArrow = Instantiate(coMan.DragArrowPrefab, uMan.CurrentWorldSpace.transform);
            dragArrow.GetComponent<DragArrow>().SourceCard = effectSource;
        }

        uMan.CreateInfoPopup(description);

        // Disallow targetting same thing twice within an effect group list
        List<GameObject> redundancies = new List<GameObject>();
        foreach (GameObject target in legalTargets[currentEffectGroup])
        {
            if (target == null)
            {
                Debug.LogError("TARGET IS NULL!");
                continue;
            }
            foreach (List<GameObject> targetList in acceptedTargets)
            {
                if (targetList.Contains(target))
                    redundancies.Add(target);
            }
        }
        foreach (GameObject target in redundancies)
        {
            if (target == null) continue;
            foreach (List<GameObject> targetList in legalTargets)
                targetList.Remove(target);
        }

        foreach (GameObject target in legalTargets[currentEffectGroup])
        {
            if (target == null) continue;
            uMan.SelectTarget(target, true);
        }
    }

    /******
     * *****
     * ****** CHECK_LEGAL_TARGETS
     * *****
     *****/
    public bool CheckLegalTargets(List<EffectGroup> groupList, GameObject source, bool isPreCheck = false)
    {
        effectGroupList = groupList;
        effectSource = source;
        legalTargets = new List<List<GameObject>>();
        acceptedTargets = new List<List<GameObject>>();
        for (int i = 0; i < effectGroupList.Count; i++)
        {
            legalTargets.Add(new List<GameObject>());
            acceptedTargets.Add(new List<GameObject>());
        }
        int group = 0;
        List<int> invalidGroups = new List<int>(); // TESTING
        foreach (EffectGroup eg in effectGroupList)
        {
            foreach (Effect effect in eg.Effects)
                if (IsTargetEffect(eg, effect))
                {
                    if (!GetLegalTargets(group, effect, eg.Targets, GetAdditionalTargets(eg.Targets), out bool requiredEffect))
                    {
                        // TESTING
                        invalidGroups.Add(group);
                        int groupsRemaining = effectGroupList.Count - invalidGroups.Count;
                        Debug.LogWarning("INVALID EFFECT GROUP! <" + groupsRemaining + "> REMAINING!");
                        if (groupsRemaining < 1 || requiredEffect) // TESTING
                            return false;
                        else break;
                    }
                }
            group++;
        }
        if (isPreCheck) ClearTargets();
        else ClearInvalids(); // TESTING
        return true;

        int GetAdditionalTargets(EffectTargets targets)
        {
            int counter = 0;
            foreach (EffectGroup group in effectGroupList)
            {
                if (group.Targets.CompareTargets(targets))
                    counter++;
            }
            if (counter > 0) counter--;
            return counter;
        }

        void ClearInvalids() // TESTING
        {
            foreach (int i in invalidGroups)
            {
                effectGroupList.RemoveAt(i);
                legalTargets.RemoveAt(i);
                acceptedTargets.RemoveAt(i);
            }
        }
        void ClearTargets()
        {
            effectGroupList = null;
            effectSource = null;
            legalTargets = null;
            acceptedTargets = null;
        }
    }

    /******
     * *****
     * ****** GET/CLEAR/SELECT_LEGAL_TARGETS
     * *****
     *****/
    private bool GetLegalTargets(int currentGroup, Effect effect, EffectTargets targets, int additionalTargets, out bool requiredEffect)
    {
        List<List<GameObject>> targetZones = new List<List<GameObject>>();
        if (effectSource.CompareTag(CombatManager.PLAYER_CARD) || 
            effectSource.CompareTag(CombatManager.PLAYER_HERO))
        {
            if (targets.PlayerHand) targetZones.Add(coMan.PlayerHandCards);
            if (targets.PlayerUnit) targetZones.Add(coMan.PlayerZoneCards);
            if (targets.EnemyUnit) targetZones.Add(coMan.EnemyZoneCards);
            if (targets.PlayerHero) AddTarget(coMan.PlayerHero);
            if (targets.EnemyHero) AddTarget(coMan.EnemyHero);
        }
        else
        {
            if (targets.PlayerHand) targetZones.Add(coMan.EnemyHandCards);
            if (targets.PlayerUnit) targetZones.Add(coMan.EnemyZoneCards);
            if (targets.EnemyUnit) targetZones.Add(coMan.PlayerZoneCards);
            if (targets.EnemyHero) AddTarget(coMan.PlayerHero);
            if (targets.PlayerHero) AddTarget(coMan.EnemyHero);
        }

        foreach (List<GameObject> zone in targetZones)
            foreach (GameObject target in zone)
            {
                if (target != effectSource)
                {
                    if (coMan.IsUnitCard(target))
                    {
                        if (!unitsToDestroy.Contains(target))
                            if (coMan.GetUnitDisplay(target).CurrentHealth > 0)
                                AddTarget(target);
                    }
                    else AddTarget(target);
                }
            }
        if (effect.IsRequired) requiredEffect = true; // TESTING
        else requiredEffect = false; // TESTING
        if (effect is DrawEffect || effect is GiveNextUnitEffect) return true;
        Debug.Log("ADDITIONAL TARGETS <" + additionalTargets + ">");
        Debug.LogWarning("LEGAL TARGETS <" + (legalTargets[currentGroup].Count - additionalTargets) + ">");
        if (legalTargets[currentGroup].Count < 1 + additionalTargets) return false;
        if (effect.IsRequired && legalTargets[currentGroup].Count < 
            effectGroupList[currentGroup].Targets.TargetNumber + additionalTargets) return false;
        return true;

        void AddTarget(GameObject target) =>
            legalTargets[currentGroup].Add(target);
    }

    public void SelectTarget(GameObject selected)
    {
        if (legalTargets[currentEffectGroup].Contains(selected))
            AcceptEffectTarget(selected);
        else RejectEffectTarget();
    }

    /******
     * *****
     * ****** ACCEPT/REJECT_TARGET
     * *****
     *****/
    private void AcceptEffectTarget(GameObject target)
    {
        auMan.StartStopSound("SFX_AcceptTarget");
        acceptedTargets[currentEffectGroup].Add(target);
        legalTargets[currentEffectGroup].Remove(target);
        uMan.SelectTarget(target, true, true);
        EffectGroup eg = effectGroupList[currentEffectGroup];
        int targetNumber = eg.Targets.TargetNumber;
        if (!eg.Effects[currentEffect].IsRequired)
        {
            int possibleTargets = legalTargets[currentEffectGroup].Count + 
                acceptedTargets[currentEffectGroup].Count;
            if (possibleTargets < targetNumber && possibleTargets > 0) 
                targetNumber = possibleTargets;
        }
        Debug.Log("ACCEPTED TARGETS: <" + acceptedTargets[currentEffectGroup].Count +
            "> // TARGET NUMBER: <" + targetNumber + ">");
        if (acceptedTargets[currentEffectGroup].Count == targetNumber) ConfirmTargetEffect();
        else if (acceptedTargets[currentEffectGroup].Count > targetNumber)
            Debug.LogError("Accepted Targets > Target Number!");
    }
    private void RejectEffectTarget()
    {
        uMan.CreateFleetinInfoPopup("You can't target that!");
        auMan.StartStopSound("SFX_Error");
    }

    /******
     * *****
     * ****** CONFIRM_EFFECTS
     * *****
     *****/
    private void ConfirmNonTargetEffect()
    {
        EffectGroup eg = effectGroupList[currentEffectGroup];
        Effect effect = eg.Effects[currentEffect];
        if (effect is DrawEffect)
        {
            string hero;
            if (eg.Targets.PlayerHand) hero = GameManager.PLAYER;
            else hero = GameManager.ENEMY;
            for (int i = 0; i < effect.Value; i++)
                coMan.DrawCard(hero);
        }
        StartNextEffect();
    }
    private void ConfirmTargetEffect()
    {
        uMan.PlayerIsTargetting = false; // TESTING

        uMan.DismissInfoPopup();
        foreach (GameObject target in legalTargets[currentEffectGroup])
            uMan.SelectTarget(target, false);
        foreach (GameObject target in acceptedTargets[currentEffectGroup])
            uMan.SelectTarget(target, false);

        if (effectGroupList[currentEffectGroup].Effects[currentEffect] is DrawEffect de)
        {
            if (de.IsDiscardEffect)
                anMan.ShiftPlayerHand(false);
        }
        else
        {
            uMan.SetCancelEffectButton(false);
            Destroy(dragArrow);
            dragArrow = null;
        }
        StartNextEffect();
    }

    /******
     * *****
     * ****** RESOLVE_EFFECT
     * *****
     *****/
    public void ResolveEffect(List<GameObject> targets, Effect effect)
    {
        foreach (GameObject t in targets)
            if (t == null) targets.Remove(t);
        // DRAW
        if (effect is DrawEffect de)
        {
            EffectGroup eg = effectGroupList[currentEffectGroup];
            if (de.IsDiscardEffect)
            {
                string hero;
                if (eg.Targets.PlayerHand) hero = GameManager.PLAYER;
                else hero = GameManager.ENEMY;
                foreach (GameObject target in targets)
                    coMan.DiscardCard(target, hero);
            }
        }
        // DAMAGE
        else if (effect is DamageEffect)
        {
            foreach (GameObject target in targets)
                coMan.TakeDamage(target, effect.Value);
        }
        else if (effect is DestroyEffect)
        {
            foreach (GameObject target in targets)
                coMan.DestroyUnit(target, false);
        }
        // HEALING
        else if (effect is HealEffect)
        {
            foreach (GameObject target in targets)
                coMan.HealDamage(target, effect.Value);
        }
        else if (effect is ExhaustEffect ee)
        {
            foreach (GameObject target in targets)
                target.GetComponent<UnitCardDisplay>().IsExhausted = ee.SetExhausted;
        }
        else if (effect is ReplenishEffect)
        {
            int newActions = pMan.PlayerActionsLeft + effect.Value;
            if (newActions > pMan.ActionsPerTurn) 
                newActions = pMan.ActionsPerTurn;
            pMan.PlayerActionsLeft = newActions;
        }
        else if (effect is EvadeEffect)
        {
            int newRefo = enMan.NextReinforcements - effect.Value;
            if (newRefo < 0) newRefo = 0;
            enMan.NextReinforcements = newRefo;
            anMan.NextReinforcementsState();
        }
        else if (effect is GiveNextUnitEffect gnfe)
        {
            GiveNextUnitEffect newGnfe = ScriptableObject.CreateInstance<GiveNextUnitEffect>();
            newGnfe.LoadEffect(gnfe);
            giveNextEffects.Add(newGnfe);
        }
        // STAT_CHANGE/GIVE_ABILITY
        else if (effect is StatChangeEffect || effect is GiveAbilityEffect)
        {
            foreach (GameObject target in targets)
                AddEffect(target, effect);
        }
        else Debug.LogError("EFFECT TYPE NOT FOUND!");
    }

    /******
     * *****
     * ****** RESOLVE_EFFECT_GROUP_LIST
     * *****
     *****/
    private void ResolveEffectGroupList()
    {
        currentEffectGroup = 0;
        currentEffect = 0; // Unnecessary
        foreach (EffectGroup eg in effectGroupList)
        {
            if (eg.EffectGroupSound2.clip != null) 
                auMan.StartStopSound(null, eg.EffectGroupSound2);
            else auMan.StartStopSound(eg.EffectGroupSound);

            foreach (Effect effect in eg.Effects)
                ResolveEffect(acceptedTargets[currentEffectGroup], effect);
            currentEffectGroup++;
        }
        FinishEffectGroupList(false);
    }

    /******
     * *****
     * ****** ABORT_EFFECT_GROUP_LIST
     * *****
     *****/
    public void AbortEffectGroupList(bool isUserAbort)
    {
        if (effectSource == null)
        {
            Debug.LogError("EFFECT SOURCE IS NULL!");
            return;
        }

        string handZone;
        if (effectSource.CompareTag(CombatManager.PLAYER_CARD))
            handZone = CombatManager.PLAYER_HAND;
        else handZone = CombatManager.ENEMY_HAND;

        if (effectSource.TryGetComponent(out ActionCardDisplay acd))
        {
            coMan.ChangeCardZone(effectSource, handZone);
            anMan.RevealedHandState(effectSource);
            effectSource.GetComponent<DragDrop>().IsPlayed = false;
            pMan.PlayerActionsLeft += acd.CurrentActionCost;
        }
        else if (effectSource.TryGetComponent(out UnitCardDisplay ucd))
        {
            if (isUserAbort)
            {
                coMan.ChangeCardZone(effectSource, handZone);
                coMan.PlayerZoneCards.Remove(effectSource);
                coMan.PlayerHandCards.Add(effectSource);
                anMan.RevealedHandState(effectSource);
                effectSource.GetComponent<DragDrop>().IsPlayed = false;
                pMan.PlayerActionsLeft += ucd.CurrentActionCost;
            }
        }
        else if (effectSource.CompareTag(CombatManager.PLAYER_HERO))
        {
            pMan.HeroPowerUsed = false;
            pMan.PlayerActionsLeft += pMan.PlayerHero.HeroPower.PowerCost;
        }

        uMan.PlayerIsTargetting = false;
        uMan.DismissInfoPopup();

        if (legalTargets != null)
        {
            List<GameObject> targetList = legalTargets[currentEffectGroup];
            foreach (GameObject target in targetList)
                target.GetComponent<CardSelect>().CardOutline.SetActive(false);
        }
        else Debug.LogError("TARGET LIST IS NULL!");

        if (dragArrow != null)
        {
            Destroy(dragArrow);
            dragArrow = null;
        }
        FinishEffectGroupList(true);
    }
    
    /******
     * *****
     * ****** FINISH_EFFECT_GROUP_LIST
     * *****
     *****/
    private void FinishEffectGroupList(bool wasAborted)
    {
        string debugText = "RESOLVED";
        if (wasAborted) debugText = "ABORTED";

        Debug.LogWarning("GROUP LIST FINISHED! [" + debugText + "]");
        if (!wasAborted)
        {
            string hero;
            if (effectSource.CompareTag(CombatManager.PLAYER_CARD))
                hero = GameManager.PLAYER;
            else hero = GameManager.ENEMY;

            if (effectSource.TryGetComponent<ActionCardDisplay>(out _))
                coMan.DiscardCard(effectSource, hero, true);
            else if (coMan.IsUnitCard(effectSource))
            {
                if (isPlayTrigger)
                    TriggerGiveNextEffect(effectSource);
            }
        }
        newDrawnCards.Clear();
        currentEffect = 0;
        currentEffectGroup = 0;
        effectGroupList = null;
        legalTargets = null;
        acceptedTargets = null;

        effectSource = null; // TESTING
        EffectsResolving = false; // TESTING
    }

    /******
     * *****
     * ****** ADD_EFFECT
     * *****
     *****/
    public void AddEffect(GameObject card, Effect effect)
    {
        UnitCardDisplay ucd = coMan.GetUnitDisplay(card);
        // GIVE_ABILITY_EFFECT
        if (effect is GiveAbilityEffect gae)
        {
            GiveAbilityEffect newGae =
                ScriptableObject.CreateInstance<GiveAbilityEffect>();
            newGae.LoadEffect(gae);
            // If ability already exists, update countdown instead of adding
            if (!ucd.AddCurrentAbility(newGae.CardAbility, true))
            {
                foreach (Effect effect2 in ucd.CurrentEffects)
                    if (effect2 is GiveAbilityEffect gae2)
                        if (gae2.CardAbility == newGae.CardAbility)
                            if (newGae.Countdown == 0 || newGae.Countdown > gae2.Countdown)
                                gae2.Countdown = newGae.Countdown;
            }
            else ucd.CurrentEffects.Add(newGae);
        }
        // STAT_CHANGE_EFFECT
        else if (effect is StatChangeEffect sce)
        {
            if (ucd.CurrentHealth < 1) return; // TESTING
            StatChangeEffect newSce =
                ScriptableObject.CreateInstance<StatChangeEffect>();
            newSce.LoadEffect(sce);
            ucd.CurrentEffects.Add(newSce);
            int statChange = sce.Value;
            if (sce.IsNegative) statChange = -statChange;
            if (sce.IsDefenseChange)
            {
                ucd.MaxHealth += statChange;
                ucd.CurrentHealth += statChange;
            }
            else ucd.CurrentPower += statChange;
        }
        else
        {
            Debug.LogError("EFFECT TYPE NOT FOUND!");
            return;
        }
    }

    /******
     * *****
     * ****** REMOVE_TEMPORARY_EFFECTS
     * *****
     *****/
    public void RemoveTemporaryEffects(string hero)
    {
        static void DestroyEffect(Effect effect)
        {
            Destroy(effect);
            effect = null;
        }
        List<GameObject> cardZone;
        if (hero == GameManager.PLAYER) cardZone = coMan.PlayerZoneCards;
        else cardZone = coMan.EnemyZoneCards;
        foreach (GameObject card in cardZone)
        {
            UnitCardDisplay fcd = coMan.GetUnitDisplay(card);
            List<Effect> expiredEffects = new List<Effect>();
            foreach (Effect effect in fcd.CurrentEffects)
            {
                if (effect.Countdown == 1) // Check for EXPIRED effects
                {
                    Debug.LogWarning("EFFECT REMOVED: <" + effect.ToString() + ">");
                    // GIVE_ABILITY_EFFECT
                    if (effect is GiveAbilityEffect gae)
                        fcd.RemoveCurrentAbility(gae.CardAbility.AbilityName);
                    // STAT_CHANGE_EFFECT
                    else if (effect is StatChangeEffect sce)
                    {
                        int statChange = sce.Value;
                        if (sce.IsNegative) statChange = -statChange;
                        if (sce.IsDefenseChange)
                        {
                            fcd.CurrentHealth -= statChange;
                            fcd.MaxHealth -= statChange;
                            anMan.ModifyUnitHealthState(card);
                        }
                        else
                        {
                            fcd.CurrentPower -= statChange;
                            anMan.ModifyUnitPowerState(card);
                        }
                    }
                    expiredEffects.Add(effect);
                }
                else if (effect.Countdown != 0)
                {
                    effect.Countdown -= 1;
                    Debug.LogWarning("COUNTOWN FOR " + 
                        effect.ToString() + " is <" + effect.Countdown + ">");
                }
            }
            foreach (Effect effect in expiredEffects)
            {
                fcd.CurrentEffects.Remove(effect);
                DestroyEffect(effect);
            }
            fcd = null;
            expiredEffects = null;
        }
    }

    /******
     * *****
     * ****** TRIGGER_GIVE_NEXT_EFFECT
     * *****
     *****/
    public void TriggerGiveNextEffect(GameObject card)
    {
        static void DestroyEffect(Effect effect)
        {
            Destroy(effect);
            effect = null;
        }
        // GIVE_NEXT_FOLLOWER_EFFECTS
        List<GiveNextUnitEffect> resolvedGnue = new List<GiveNextUnitEffect>();
        if (GiveNextEffects.Count > 0)
        {
            List<GameObject> targets = new List<GameObject> { card };
            foreach (GiveNextUnitEffect gnue in GiveNextEffects)
            {
                // CHECK FOR ALLY/ENEMY HERE
                foreach (Effect e in gnue.Effects)
                    ResolveEffect(targets, e);
                if (--gnue.Multiplier < 1) resolvedGnue.Add(gnue);
            }
            foreach (GiveNextUnitEffect rGnue in resolvedGnue)
            {
                GiveNextEffects.Remove(rGnue);
                DestroyEffect(rGnue);
            }
        }
    }

    /******
     * *****
     * ****** REMOVE_GIVE_NEXT_EFFECTS
     * *****
     *****/
    public void RemoveGiveNextEffects()
    {
        static void DestroyEffect(Effect effect)
        {
            Destroy(effect);
            effect = null;
        }
        List<GiveNextUnitEffect> expiredGne = new List<GiveNextUnitEffect>();
        foreach (GiveNextUnitEffect gnfe in GiveNextEffects)
            if (gnfe.Countdown == 1) expiredGne.Add(gnfe);
            else if (gnfe.Countdown != 0) gnfe.Countdown -= 1;
        foreach (GiveNextUnitEffect xGnfe in expiredGne)
        {
            GiveNextEffects.Remove(xGnfe);
            DestroyEffect(xGnfe);
        }
    }
}
