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

    /* TEST_HEROES */
    [SerializeField] private EnemyHero enemyTestHero; // FOR TESTING ONLY

    /* AUGMENT_EFFECTS */
    [SerializeField] private GiveNextUnitEffect augmentBiogenEffect;


    /* GAME_MANAGER_DATA */
    public const int START_ACTIONS_PER_TURN = 1;
    public const int MAX_ACTIONS_PER_TURN = 5;
    public const int MAXIMUM_ACTIONS = 5;

    public const string PLAYER = "Player";
    public const int PLAYER_STARTING_HEALTH = 20;
    public const int PLAYER_HAND_SIZE = 4;
    public const int PLAYER_START_FOLLOWERS = 3;
    public const int PLAYER_START_SKILLS = 2;

    public const string ENEMY = "Enemy";
    public const int ENEMY_STARTING_HEALTH = 20;
    public const int ENEMY_HAND_SIZE = 0;
    public const int ENEMY_START_FOLLOWERS = 8;
    public const int ENEMY_START_SKILLS = 2;

    /* MANAGERS */
    private PlayerManager playerManager;
    private EnemyManager enemyManager;
    private CardManager cardManager;
    private UIManager UIManager;
    EventManager eventManager;

    /******
     * *****
     * ****** START
     * *****
     *****/
    private void Start()
    {
        playerManager = PlayerManager.Instance;
        enemyManager = EnemyManager.Instance;
        cardManager = CardManager.Instance;
        UIManager = UIManager.Instance;
        eventManager = EventManager.Instance;
    }

    /******
     * *****
     * ****** NEW_GAME
     * *****
     *****/
    public void NewGame()
    {
        //PlayerManager.Instance.PlayerHero = playerTestHero; // FOR TESTING ONLY
        StartCombat(enemyTestHero); // FOR TESTING ONLY
    }

    /******
     * *****
     * ****** START/END_COMBAT
     * *****
     *****/
    private void StartCombat(EnemyHero enemyHero)
    {
        AudioManager.Instance.StartStopSound("Soundtrack_Combat1", null, AudioManager.SoundType.Soundtrack);
        FunctionTimer.Create(() => AudioManager.Instance.StartStopSound("SFX_StartCombat"), 1f);

        enemyManager.EnemyHero = enemyHero;
        cardManager.UpdateDeck(PLAYER);
        cardManager.UpdateDeck(ENEMY);

        /* PLAYER_HEALTH */
        int bonusHealth = 0;
        if (playerManager.GetAugment("Kinetic Regulator")) bonusHealth = 5;
        playerManager.PlayerHealth = PLAYER_STARTING_HEALTH + bonusHealth;

        /* PLAYER_ACTIONS */
        playerManager.PlayerActionsLeft = 0;
        int bonusActions = 0;
        if (playerManager.GetAugment("Synaptic Stabilizer")) bonusActions = 1;
        playerManager.ActionsPerTurn = START_ACTIONS_PER_TURN + bonusActions;

        /* ENEMY_HEALTH */
        enemyManager.EnemyHealth = ENEMY_STARTING_HEALTH;

        /* HERO_DISPLAYS */
        cardManager.PlayerHero.GetComponent<HeroDisplay>().HeroScript = playerManager.PlayerHero;
        cardManager.EnemyHero.GetComponent<HeroDisplay>().HeroScript = enemyManager.EnemyHero;

        /* OTHER AUGMENTS */
        if (playerManager.GetAugment("Biogenic Enhancer"))
        {
            GiveNextUnitEffect gnue = ScriptableObject.CreateInstance<GiveNextUnitEffect>();
            gnue.LoadEffect(augmentBiogenEffect);
            EffectManager.Instance.GiveNextEffects.Add(gnue);
        }

        for (int i = 0; i < PLAYER_HAND_SIZE; i++)
        {
            eventManager.NewDelayedAction(() => cardManager.DrawCard(PLAYER), 1f);
        }
        for (int i = 0; i < ENEMY_HAND_SIZE; i++)
        {
            eventManager.NewDelayedAction(() => cardManager.DrawCard(ENEMY), 1f);
        }
        eventManager.NewDelayedAction(() => StartTurn(PLAYER), 1f);
    }
    public void EndCombat(bool playerWins)
    {
        // VICTORY or DEFEAT animation
        if (playerWins)
        {
            Debug.LogWarning("PLAYER WINS!");
            AudioManager.Instance.StartStopSound(null, PlayerManager.Instance.PlayerHero.HeroWin);
        }
        else
        {
            Debug.LogWarning("ENEMY WINS!");
            AudioManager.Instance.StartStopSound(null, PlayerManager.Instance.PlayerHero.HeroLose);

        }
    }

    /******
     * *****
     * ****** START/END_TURN
     * *****
     *****/
    private void StartTurn(string player)
    {
        cardManager.RefreshFollowers(player);
        // PLAYER
        if (player == PLAYER)
        {
            playerManager.IsMyTurn = true;
            enemyManager.IsMyTurn = false;
            UIManager.UpdateEndTurnButton(true);
            playerManager.HeroPowerUsed = false;
            /* CUMULATIVE ACTION GAIN
            playerManager.PlayerActionsLeft += playerManager.ActionsPerTurn;
            if (playerManager.ActionsPerTurn < MAX_ACTIONS_PER_TURN)
                playerManager.ActionsPerTurn++;
            cardManager.DrawCard(PLAYER);
            */
            void RefillPlayerActions ()
            {
                playerManager.PlayerActionsLeft = playerManager.ActionsPerTurn;
                AudioManager.Instance.StartStopSound("SFX_ActionRefill");
            }
            // LINEAR ACTION GAIN
            EventManager.Instance.NewDelayedAction(() => RefillPlayerActions(), 0f);
            EventManager.Instance.NewDelayedAction(() => cardManager.DrawCard(PLAYER), 1f);
        }

        // ENEMY
        else if (player == ENEMY)
        {
            playerManager.IsMyTurn = false;
            enemyManager.IsMyTurn = true;
            UIManager.UpdateEndTurnButton(false);

            // ENEMY TURN
            EnemyManager.Instance.StartEnemyTurn();
        }
    }

    public void EndTurn(string player)
    {
        CardManager.Instance.RemoveTemporaryEffects(PLAYER);
        CardManager.Instance.RemoveTemporaryEffects(ENEMY);
        CardManager.Instance.RemoveGiveNextEffects();
        if (player == ENEMY) StartTurn(PLAYER);

        else if (player == PLAYER)
        {
            if (playerManager.ActionsPerTurn < MAX_ACTIONS_PER_TURN)
                playerManager.ActionsPerTurn++;

            StartTurn(ENEMY);
        }
    }
}
