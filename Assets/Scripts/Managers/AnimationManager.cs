﻿using System.Collections;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    /* SINGELTON_PATTERN */
    public static AnimationManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private CombatManager coMan;
    private Vector2 playerHandStart;

    private void Start() => coMan = CombatManager.Instance;

    public void ChangeAnimationState(GameObject go, string animationState)
    {
        if (go.TryGetComponent<ActionCardDisplay>(out _)) return;
        if (go.TryGetComponent(out Animator anim))
            if (anim.enabled)
                anim.Play(animationState);
            else Debug.LogWarning("ANIMATOR NOT FOUND!");
    }

    /* HERO_ANIMATIONS */
    public void ModifyHeroHealthState(GameObject hero) => ChangeAnimationState(hero, "Modify_Health");
    public void ReinforcementsState(GameObject enemyHero) => ChangeAnimationState(enemyHero, "Reinforcements");
    /* UNIT_ANIMATIONS */
    public void RevealedHandState(GameObject card) => ChangeAnimationState(card, "Revealed_Hand");
    //public void RevealedPlayState(GameObject card) => ChangeAnimationState(card, "Revealed_Play");
    public void RevealedDragState(GameObject card) => ChangeAnimationState(card, "Revealed_Drag");
    public void PlayedState(GameObject card)
    {
        card.GetComponent<CardDisplay>().CardArt = card.GetComponent<CardDisplay>().CardScript.CardArt;
        ChangeAnimationState(card, "Played");
        //card.transform.SetAsLastSibling(); // TESTING
    }
    //public void ModifyAttackState(GameObject card) => ChangeAnimationState(card, Modify_Attack);
    public void ModifyUnitHealthState(GameObject card) => ChangeAnimationState(card, "Modify_Health");
    public void ZoomedState(GameObject card) => ChangeAnimationState(card, "Zoomed");

    /******
     * *****
     * ****** SHIFT_PLAYER_HAND
     * *****
     *****/
    public void ShiftPlayerHand(bool isUpShift)
    {
        StartCoroutine(HandShiftNumerator(isUpShift));
    }
    private IEnumerator HandShiftNumerator(bool isUpShift)
    {
        float distance;
        float yTarget;
        GameObject hand = coMan.PlayerHand;
        if (isUpShift)
        {
            yTarget = -350;
            playerHandStart = hand.transform.position;
        }
        else yTarget = playerHandStart.y;
        Vector2 target = new Vector2(0, yTarget);

        do
        {
            distance = Vector2.Distance(hand.transform.position, target);
            hand.transform.position = Vector2.MoveTowards(hand.transform.position, target, 30);
            yield return new WaitForFixedUpdate();
        }
        while (distance > 0);
        UIManager uMan = UIManager.Instance;
        uMan.PlayerIsDiscarding = uMan.PlayerIsTargetting;
        uMan.DestroyZoomObjects();
    }

    /******
     * *****
     * ****** DIALOGUE_INTRO
     * *****
     *****/
    public void DialogueIntro()
    {
        StartCoroutine(DialogueIntroNumerator());
    }
    private IEnumerator DialogueIntroNumerator()
    {
        float distance;
        DialogueSceneDisplay dsp = DialogueManager.Instance.DialogueDisplay;
        GameObject playerPortrait = dsp.PlayerHeroPortrait;
        GameObject npcPortrait = dsp.NPCHeroPortrait;
        Vector2 pPortStart = playerPortrait.transform.localPosition;
        Vector2 nPortStart = npcPortrait.transform.localPosition;
        playerPortrait.SetActive(true);
        npcPortrait.SetActive(true);
        playerPortrait.transform.localPosition = new Vector2(600, pPortStart.y);
        npcPortrait.transform.localPosition = new Vector2(-600, nPortStart.y);

        yield return new WaitForSeconds(0.5f);
        do
        {
            distance = Vector2.Distance(playerPortrait.transform.localPosition, pPortStart);
            playerPortrait.transform.localPosition = Vector2.MoveTowards(playerPortrait.transform.localPosition, pPortStart, 30);
            npcPortrait.transform.localPosition = Vector2.MoveTowards(npcPortrait.transform.localPosition, nPortStart, 30);
            yield return new WaitForFixedUpdate();
        }
        while (distance > 0);
        // PLAY SOUND
    }

    /******
     * *****
     * ****** COMBAT_INTRO
     * *****
     *****/
    public void CombatIntro()
    {
        StartCoroutine(CombatIntroNumerator());
    }
    private IEnumerator CombatIntroNumerator()
    {
        float distance;
        HeroDisplay pHD = coMan.PlayerHero.GetComponent<HeroDisplay>();
        HeroDisplay eHD = coMan.EnemyHero.GetComponent<HeroDisplay>();
        GameObject pFrame = pHD.HeroFrame;
        GameObject eFrame = eHD.HeroFrame;
        GameObject pStats = pHD.HeroStats;
        GameObject eStats = eHD.HeroStats;
        Vector2 pFrameStart = pFrame.transform.position;
        Vector2 eFrameStart = eFrame.transform.position;
        Vector2 pStatsStart = pStats.transform.localPosition;
        Vector2 eStatsStart = eStats.transform.localPosition;
        float scaleSpeed = 0.1f;
        float fScale = 1;
        float fZoomScale = 1.5f;
        Vector2 scaleVec = new Vector2();
        pStats.SetActive(true);
        eStats.SetActive(true);
        pStats.transform.localPosition = new Vector2(pStatsStart.x, -450);
        eStats.transform.localPosition = new Vector2(eStatsStart.x, 450);
        
        do
        {
            distance = Vector2.Distance(pFrame.transform.position, eFrame.transform.position);
            pFrame.transform.position = Vector2.MoveTowards(pFrame.transform.position, eFrame.transform.position, 30);
            eFrame.transform.position = Vector2.MoveTowards(eFrame.transform.position, pFrame.transform.position, 30);
            if (fScale < fZoomScale) fScale += scaleSpeed;
            else if (fScale > fZoomScale) fScale = fZoomScale;
            scaleVec.Set(fScale, fScale);
            pFrame.transform.localScale = scaleVec;
            eFrame.transform.localScale = scaleVec;

            yield return new WaitForFixedUpdate();
        }
        while (distance > 700);

        UIManager.Instance.CreateVersusPopup();
        yield return new WaitForSeconds(3f);
        do
        {
            distance = Vector2.Distance(pFrame.transform.position, pFrameStart);
            pFrame.transform.position = Vector2.MoveTowards(pFrame.transform.position, pFrameStart, 20);
            eFrame.transform.position = Vector2.MoveTowards(eFrame.transform.position, eFrameStart, 20);
            if (fScale > 1) fScale -= scaleSpeed;
            else if (fScale < 1) fScale = 1;
            scaleVec.Set(fScale, fScale);
            pFrame.transform.localScale = scaleVec;
            eFrame.transform.localScale = scaleVec;
            yield return new WaitForFixedUpdate();
        }
        while (distance > 0);

        do
        {
            distance = Vector2.Distance(pStats.transform.localPosition, pStatsStart);
            pStats.transform.localPosition = Vector2.MoveTowards(pStats.transform.localPosition, pStatsStart, 20);
            eStats.transform.localPosition = Vector2.MoveTowards(eStats.transform.localPosition, eStatsStart, 20);
            yield return new WaitForFixedUpdate();
        }
        while (distance > 0);
    }

    /******
     * *****
     * ****** UNIT_ATTACK
     * *****
     *****/
    public void UnitAttack(GameObject attacker, GameObject defender, bool defenderIsUnit) => 
        StartCoroutine(AttackNumerator(attacker, defender, defenderIsUnit));

    private IEnumerator AttackNumerator(GameObject attacker, GameObject defender, bool defenderIsUnit = true)
    {
        float distance;
        float bufferDistance;
        float attackSpeed;
        float retreatSpeed;
        if (defenderIsUnit)
        {
            bufferDistance = 150;
            attackSpeed = 50;
            retreatSpeed = 30;
        }
        else
        {
            bufferDistance = 350;
            attackSpeed = 100;
            retreatSpeed = 75;
        }
        int atkIndex = attacker.transform.GetSiblingIndex();
        Transform atkStartParent = attacker.transform.parent;
        Vector3 atkStartPos = attacker.transform.position;
        Vector2 defPos = defender.transform.position;
        attacker.transform.SetParent(UIManager.Instance.CurrentCanvas.transform);
        attacker.transform.SetAsLastSibling(); // TESTING
        // ATTACK
        do
        {
            distance = Vector2.Distance(attacker.transform.position, defPos);
            attacker.transform.position = Vector3.MoveTowards(attacker.transform.position, defPos, attackSpeed);
            yield return new WaitForFixedUpdate();
        }
        while (distance > bufferDistance);
        // RETREAT
        do
        {
            distance = Vector2.Distance(attacker.transform.position, atkStartPos);
            attacker.transform.position = Vector3.MoveTowards(attacker.transform.position, atkStartPos, retreatSpeed);
            yield return new WaitForFixedUpdate();
        }
        while (distance > 0);

        attacker.transform.SetParent(atkStartParent);
        attacker.transform.SetSiblingIndex(atkIndex);
        attacker.transform.position = new Vector3(atkStartPos.x, atkStartPos.y, CombatManager.CARD_Z_POSITION);
    }
}