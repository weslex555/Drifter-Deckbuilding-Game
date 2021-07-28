﻿using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    /* SINGELTON_PATTERN */
    public static PlayerManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        PlayerDeckList = new List<Card>();
        CurrentPlayerDeck = new List<Card>();
        HeroPowerUsed = false;
    }

    /* PLAYER_HERO */
    public PlayerHero PlayerHero
    {
        get => playerHero;
        set
        {
            playerHero = value;
            CardManager cm = CardManager.Instance;
            for (int i = 0; i < GameManager.PLAYER_START_FOLLOWERS; i++)
            {
                foreach (UnitCard uc in cm.PlayerStartUnits)
                    cm.AddCard(uc, GameManager.PLAYER);
            }
            foreach (SkillCard skill in PlayerHero.HeroSkills)
            {
                for (int i = 0; i < GameManager.PLAYER_START_SKILLS; i++)
                    cm.AddCard(skill, GameManager.PLAYER);
            }
        }
    }
    private PlayerHero playerHero;

    /* PLAYER_DECK */
    public List<Card> PlayerDeckList { get; private set; }
    public List<Card> CurrentPlayerDeck { get; private set; }

    /* IS_MY_TURN */
    public bool IsMyTurn { get; set; }

    /* ACTIONS_PER_TURN */
    public int ActionsPerTurn { get; set; }

    /* HEALTH */
    private int playerHealth;
    public int PlayerHealth
    {
        get => playerHealth;
        set
        {
            playerHealth = value;
            UIManager.Instance.UpdatePlayerHealth(PlayerHealth);
        }
    }

    /* ACTIONS_LEFT */
    private int playerActionsLeft;
    public int PlayerActionsLeft
    {
        get => playerActionsLeft;
        set
        {
            playerActionsLeft = value;
            if (playerActionsLeft > GameManager.MAXIMUM_ACTIONS) playerActionsLeft = GameManager.MAXIMUM_ACTIONS;
            UIManager.Instance.UpdatePlayerActionsLeft(PlayerActionsLeft);
        }
    }

    /* HERO_POWER */
    public bool HeroPowerUsed { get; set; }
    public void UseHeroPower()
    {
        if (UIManager.Instance.PlayerIsTargetting) return;
        else if (HeroPowerUsed == true)
        {
            Debug.Log("HERO POWER ALREADY USED THIS TURN!");
            // Create fleeting info popup
            return;
        }
        else if (PlayerActionsLeft < playerHero.HeroPower.PowerCost)
        {
            Debug.Log("NOT ENOUGH ACTIONS!");
            // Create fleeting info popup
            return;
        }
        else
        {
            PlayerActionsLeft -= playerHero.HeroPower.PowerCost;
            HeroPowerUsed = true;
            EffectManager.Instance.StartEffectGroupList(PlayerHero.HeroPower.EffectGroupList, CardManager.Instance.PlayerHero);

            foreach (Sound s in PlayerHero.HeroPower.PowerSounds)
                AudioManager.Instance.StartStopSound(null, s);
        }
    }
}
