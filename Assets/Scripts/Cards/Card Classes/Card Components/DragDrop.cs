﻿using UnityEngine;

public class DragDrop : MonoBehaviour
{
    private CardContainer container;
    private GameObject dragArrow;
    private ParticleSystemHandler particleHandler;
    private bool isOverDropZone;
    private bool isDragging;

    private const string SFX_DRAG_CARD = "SFX_DragCard";

    private bool IsDragging
    {
        get => isDragging;
        set
        {
            isDragging = value;
            container.IsDetached = isDragging;
        }
    }

    public int LastIndex { get; private set; }
    public bool IsPlayed { get; set; }

    public static GameObject DraggingCard;
    public static GameObject Enemy;
    public static bool ArrowIsDragging;

    void Start()
    {
        CardDisplay cd = GetComponent<CardDisplay>();
        if (cd.CardContainer != null)
            container = cd.CardContainer.GetComponent<CardContainer>();
        isOverDropZone = false;
        isDragging = false;
        IsPlayed = false;
    }

    void FixedUpdate()
    {
        if (IsDragging)
        {
            Vector3 dragPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector2(dragPoint.x, dragPoint.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsDragging && !IsPlayed)
            if (collision.gameObject == Managers.P_MAN.PlayZone)
            {
                isOverDropZone = true;
                Managers.U_MAN.SetPlayerZoneOutline(true, true);
            }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsDragging && !IsPlayed)
            if (collision.gameObject == Managers.P_MAN.PlayZone)
            {
                isOverDropZone = false;
                Managers.U_MAN.SetPlayerZoneOutline(true, false);
            }
    }

    private void ResetPosition()
    {
        transform.SetParent(CombatManager.Instance.CardZone.transform);

        transform.localPosition = container.transform.position;
        transform.SetSiblingIndex(LastIndex);
        IsPlayed = false;
        AnimationManager.Instance.RevealedHandState(gameObject);
    }

    public void StartDrag()
    {
        Managers.U_MAN.DestroyZoomObjects();
        if (!Managers.P_MAN.IsMyTurn || CompareTag(Managers.EN_MAN.CARD_TAG) || DraggingCard != null ||
            ArrowIsDragging || EffectManager.Instance.EffectsResolving ||
            EventManager.Instance.ActionsDelayed) return;

        FunctionTimer.StopTimer(CardZoom.ZOOM_CARD_TIMER);
        FunctionTimer.StopTimer(CardZoom.ABILITY_POPUP_TIMER);
        DraggingCard = gameObject;

        if (!IsPlayed)
        {
            IsDragging = true;
            LastIndex = transform.GetSiblingIndex();
            transform.SetParent(Managers.U_MAN.CurrentZoomCanvas.transform);

            Managers.U_MAN.SetPlayerZoneOutline(true, false);
            AnimationManager.Instance.RevealedDragState(gameObject);
            particleHandler = Managers.AN_MAN.CreateParticleSystem(gameObject,
                ParticleSystemHandler.ParticlesType.Drag);
        }
        else
        {
            if (!Managers.CO_MAN.CanAttack(gameObject, null))
            {
                DraggingCard = null;
                return;
            }

            ArrowIsDragging = true;
            if (dragArrow != null) Destroy(dragArrow);
            dragArrow = Instantiate(Managers.CA_MAN.DragArrowPrefab, Managers.U_MAN.CurrentCanvas.transform);
            dragArrow.GetComponent<DragArrow>().SourceCard = gameObject;

            foreach (GameObject enemyUnit in Managers.EN_MAN.PlayZoneCards)
                if (Managers.CO_MAN.CanAttack(gameObject, enemyUnit))
                    Managers.U_MAN.SelectTarget(enemyUnit, UIManager.SelectionType.Highlighted);

            foreach (GameObject playerUnit in Managers.P_MAN.PlayZoneCards)
                Managers.U_MAN.SelectTarget(playerUnit, UIManager.SelectionType.Disabled);

            if (Managers.CO_MAN.CanAttack(gameObject, Managers.EN_MAN.HeroObject))
                Managers.U_MAN.SelectTarget(Managers.EN_MAN.HeroObject, UIManager.SelectionType.Highlighted);

            particleHandler = Managers.AN_MAN.CreateParticleSystem(gameObject,
                ParticleSystemHandler.ParticlesType.MouseDrag);
        }
        Managers.AU_MAN.StartStopSound(SFX_DRAG_CARD, null, AudioManager.SoundType.SFX, false, true);
    }

    public void EndDrag()
    {
        if (!IsDragging && !ArrowIsDragging) return;

        DraggingCard = null;
        Managers.AU_MAN.StartStopSound(SFX_DRAG_CARD, null, AudioManager.SoundType.SFX, true);

        if (particleHandler != null)
        {
            particleHandler.StopParticles();
            particleHandler = null;
        }

        // From Hand
        if (!IsPlayed)
        {
            IsDragging = false;
            Managers.U_MAN.SetPlayerZoneOutline(false, false);

            if (isOverDropZone && Managers.CA_MAN.IsPlayable(gameObject))
            {
                // TUTORIAL!
                if (Managers.G_MAN.IsTutorial)
                {
                    switch (Managers.P_MAN.EnergyPerTurn)
                    {
                        case 1:
                            Managers.G_MAN.Tutorial_Tooltip(3);
                            break;
                        case 2:
                            if (!Managers.P_MAN.HeroPowerUsed)
                            {
                                ResetPosition();
                                return;
                            }
                            break;
                    }
                }

                IsPlayed = true;
                Managers.CA_MAN.PlayCard(gameObject);
                transform.SetParent(Managers.CO_MAN.CardZone.transform);
            }
            else ResetPosition();
            return;
        }

        // From Play
        ArrowIsDragging = false;
        Destroy(dragArrow);
        dragArrow = null;

        if (Enemy != null)
        {
            if (Managers.CO_MAN.CanAttack(gameObject, Enemy, false))
            {
                GameObject enemy = Enemy;
                EventManager.Instance.NewDelayedAction(() =>
                Managers.CO_MAN.Attack(gameObject, enemy), 0.25f);
            }
            Managers.U_MAN.SelectTarget(Enemy, UIManager.SelectionType.Disabled);
            Enemy = null;
        }


        foreach (GameObject enemyUnit in Managers.EN_MAN.PlayZoneCards)
            Managers.U_MAN.SelectTarget(enemyUnit, UIManager.SelectionType.Disabled);

        Managers.U_MAN.SelectTarget(Managers.EN_MAN.HeroObject, UIManager.SelectionType.Disabled);
        Managers.CA_MAN.SelectPlayableCards(true);
        Managers.CA_MAN.SelectPlayableCards();
    }
}