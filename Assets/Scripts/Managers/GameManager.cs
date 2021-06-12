﻿using UnityEngine;

public class GameManager : MonoBehaviour
{
    /* SINGELTON_PATTERN */
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    /* GAME_MANAGER_DATA */
    public const int STARTING_ACTIONS = 0;
    public const int ACTIONS_PER_TURN = 2;
    public const int MAXIMUM_ACTIONS = 6;
    private const string PLAYER = "Player";
    private const string ENEMY = "Enemy";

    /* MANAGERS */
    private PlayerManager playerManager;
    private EnemyManager enemyManager;
    private CardManager cardManager;
    private UIManager UIManager;

    /******
     * *****
     * ****** START
     * *****
     *****/
    private void Start()
    {
        playerManager = PlayerManager.Instance;
        enemyManager = EnemyManager.Instance;
        UIManager = UIManager.Instance;
        cardManager = CardManager.Instance;
    }

    /******
     * *****
     * ****** START/END_GAME
     * *****
     *****/

    public void StartGame()
    {
        PlayerManager.Instance.PlayerHealth = 10;
        EnemyManager.Instance.EnemyHealth = 10;
        PlayerManager.Instance.PlayerActionsLeft = STARTING_ACTIONS;
        EnemyManager.Instance.EnemyActionsLeft = STARTING_ACTIONS;

        FunctionTimer.Create(() => CardManager.Instance.DrawHand(GameManager.PLAYER), 1f);
        FunctionTimer.Create(() => StartTurn(PLAYER), 3f);
    }
    public void EndGame()
    {
        // VICTORY or DEFEAT animation
    }

    /******
     * *****
     * ****** START/END_TURN
     * *****
     *****/
    private void StartTurn(string activePlayer)
    {
        if (activePlayer == PLAYER)
        {
            playerManager.IsMyTurn = true;
            enemyManager.IsMyTurn = false;
            UIManager.UpdateEndTurnButton(playerManager.IsMyTurn);
            PlayerManager.Instance.PlayerActionsLeft += ACTIONS_PER_TURN;
            cardManager.DrawCard(activePlayer);
            cardManager.RefreshCards(activePlayer);
        }
        else if (activePlayer == ENEMY)
        {
            playerManager.IsMyTurn = false;
            enemyManager.IsMyTurn = true;
            UIManager.UpdateEndTurnButton(playerManager.IsMyTurn);
            EnemyManager.Instance.EnemyActionsLeft += ACTIONS_PER_TURN;
            cardManager.DrawCard(activePlayer);
            cardManager.RefreshCards(activePlayer);

            FunctionTimer.Create(() => CardManager.Instance.PlayCard(null, ENEMY), 1.5f);
            FunctionTimer.Create(() => EndTurn(ENEMY), 4f);
        }
    }

    public void EndTurn(string player)
    {
        // end of turn effects
        if (player == PLAYER) StartTurn(ENEMY);
        else if (player == ENEMY) StartTurn(PLAYER);
    }
}
