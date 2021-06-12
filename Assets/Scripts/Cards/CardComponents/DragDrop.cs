﻿using UnityEngine;

public class DragDrop : MonoBehaviour
{
    /* CARD_MANAGER_DATA */
    private const string PLAYER_ZONE = CardManager.PLAYER_ZONE;
    private const string ENEMY_ZONE = CardManager.ENEMY_ZONE;
    private const string ENEMY_CARD = CardManager.ENEMY_CARD;
    private const string BACKGROUND = CardManager.BACKGROUND;

    /* GAME_MANAGER_DATA */
    private const string PLAYER = "Player";
    private const string ENEMY = "Enemy";

    /* MANAGERS */
    private PlayerManager playerManager;
    private CardManager cardManager;
    private UIManager UIManager;

    /* ZONES */
    private GameObject background;
    private GameObject playerZone;
    private GameObject enemyZone;

    /* STATIC CLASS VARIABLES */
    public static bool CardIsDragging;

    /* CLASS VARIABLES */
    private GameObject enemy;
    private bool isOverEnemy;
    private bool isOverDropZone;
    private GameObject startParent;
    private Vector2 startPosition;
    private int startIndex;

    private bool isDragging;
    public bool IsDragging
    {
        get => isDragging;
        private set
        {
            isDragging = value;
            CardIsDragging = IsDragging;
        }
    }
    private bool isPlayed;

    void Start()
    {
        playerManager = PlayerManager.Instance;
        cardManager = CardManager.Instance;
        UIManager = UIManager.Instance;

        background = GameObject.Find(BACKGROUND);
        playerZone = GameObject.Find(PLAYER_ZONE);
        enemyZone = GameObject.Find(ENEMY_ZONE);

        CardIsDragging = false;
        isOverEnemy = false;
        isOverDropZone = false;
        IsDragging = false;
        isPlayed = false;
    }

    void Update()
    {
        if (IsDragging)
        {
            Vector3 dragPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(dragPoint.x, dragPoint.y, -2);
            transform.SetParent(background.transform, true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collisionObject = collision.gameObject;
        GameObject collisionObjectParent = collisionObject.transform.parent.gameObject;
        if (!isPlayed)
        {
            if (collisionObject == playerZone) isOverDropZone = true;
        }
        else
        {
            if (collisionObject.CompareTag(ENEMY_CARD) && collisionObjectParent == enemyZone) // change collisionObjectParent = enemyZone to IsPlayed ?
            {
                isOverEnemy = true;
                enemy = collisionObject;
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        GameObject collisionObject = collision.gameObject;
        if (!isPlayed)
        {
            if (collisionObject == playerZone) isOverDropZone = false;
        }
        else
        {
            if (collisionObject == enemy)
            {
                isOverEnemy = false;
                enemy = null;
            }
        }
    }

    private void ResetPosition()
    {
        transform.position = startPosition;
        cardManager.SetCardParent(gameObject, startParent.transform);
        transform.SetSiblingIndex(startIndex);
    }

    public void StartDrag()
    {
        UIManager.DestroyAllZoomObjects();
        if (!playerManager.IsMyTurn || CompareTag(ENEMY_CARD)) return;
        IsDragging = true;

        startParent = transform.parent.gameObject;
        startPosition = transform.position;
        startIndex = transform.GetSiblingIndex();
        gameObject.GetComponent<ChangeLayer>().ZoomLayer();
    }

    public void EndDrag()
    {
        if (!IsDragging || !playerManager.IsMyTurn || CompareTag(ENEMY_CARD)) return;
        IsDragging = false;

        if (!isPlayed)
        {
            if (isOverDropZone && cardManager.IsPlayable(gameObject))
            {
                isPlayed = true;
                if (gameObject.CompareTag("PlayerCard")) cardManager.PlayCard(gameObject, PLAYER);
                else cardManager.PlayCard(gameObject, ENEMY);
            }
            else ResetPosition();
        }
        else if (isOverEnemy && cardManager.CanAttack(gameObject))
        {
            ResetPosition();
            cardManager.Attack(gameObject, enemy);
        }
        else ResetPosition();
    }
}