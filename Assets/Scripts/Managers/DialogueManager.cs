using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    /* SINGELTON PATTERN */
    public static DialogueManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    
    public NPCHero EngagedHero { get; private set; }
    private DialogueClip currentDialogueClip;
    private DialogueSceneDisplay dialogueDisplay;

    //private Coroutine typedTextRoutine;

    private void DisplayCurrentHeroes(NPCHero hero)
    {
        PlayerManager pm = PlayerManager.Instance;
        dialogueDisplay.PlayerHeroPortrait = pm.PlayerHero.HeroPortrait;
        dialogueDisplay.PlayerHeroName = pm.PlayerHero.HeroName;
        dialogueDisplay.NPCHeroPortrait = hero.HeroPortrait;
        dialogueDisplay.NPCHeroName = hero.HeroName;
    }

    public void StartDialogue(NPCHero npc)
    {
        AudioManager.Instance.StartStopSound("Soundtrack_DialogueScene", null, AudioManager.SoundType.Soundtrack);
        dialogueDisplay = FindObjectOfType<DialogueSceneDisplay>();
        EngagedHero = npc;
        currentDialogueClip = npc.NextDialogueClip;
        if (currentDialogueClip == null)
        {
            Debug.LogError("CLIP IS NULL!");
            return;
        }
        DisplayCurrentHeroes(npc);
        DisplayDialoguePopup();
    }

    public void EndDialogue(DialogueClip nextClip = null)
    {
        if (nextClip != null) EngagedHero.NextDialogueClip = nextClip;
        EngagedHero = null;
        currentDialogueClip = null;
        //StopTimedText();
    }

    private void DisplayDialoguePopup()
    {
        DialoguePrompt dpr = currentDialogueClip as DialoguePrompt;
        dialogueDisplay.NPCHeroSpeech = dpr.DialoguePromptText;
        // Response 1
        if (dpr.DialogueResponse1 == null)
            dialogueDisplay.Response_1 = "";
        else
            dialogueDisplay.Response_1 = dpr.DialogueResponse1.ResponseText;
        // Response 2
        if (dpr.DialogueResponse2 == null)
            dialogueDisplay.Response_2 = "";
        else
            dialogueDisplay.Response_2 = dpr.DialogueResponse2.ResponseText;
        // Response 3
        if (dpr.DialogueResponse3 == null)
            dialogueDisplay.Response_3 = "";
        else
            dialogueDisplay.Response_3 = dpr.DialogueResponse3.ResponseText;
        // Journal Notes
        if (dpr.JournalNotes.Count > 0)
        {
            Debug.LogWarning("NEW JOURNAL NOTE!");
            //JournalManager.Instance.NewJournalNote(currentDialogueClip.JournalNotes);
        }
    }

    public void DialogueResponse(int response)
    {
        /*
        if (TypedTextRoutine != null)
        {
            StopTimedText(true);
            return;
        }
        */
        DialoguePrompt dpr = currentDialogueClip as DialoguePrompt;
        DialogueResponse dr = null;
        switch (response)
        {
            case 1:
                dr = dpr.DialogueResponse1;
                break;
            case 2:
                dr = dpr.DialogueResponse2;
                break;
            case 3:
                dr = dpr.DialogueResponse3;
                break;
        }

        DialogueClip dc = dr.Response_NextClip;
        if (dc == null) return;

        EngagedHero.RespectScore += dr.Response_Respect;
        // Exit
        if (dr.Response_IsExit)
        {
            EndDialogue(dc);
            GameManager.Instance.EndGame(); // FOR TESTING ONLY
            return;
        }
        // Combat Start
        if (dr.Response_IsCombatStart)
        {
            EngagedHero.NextDialogueClip = dr.Response_NextClip;
            SceneLoader.LoadScene(SceneLoader.Scene.CombatScene);
            return;
        }

        if (dc is DialoguePrompt)
        {
            // New Engaged Hero
            if (dpr.NewEngagedHero != null)
            {
                EngagedHero.NextDialogueClip = dc;
                EngagedHero = GameManager.Instance.GetActiveNPC(dpr.NewEngagedHero);
                DisplayCurrentHeroes(EngagedHero);
            }
            // New Card
            if (dpr.NewCard != null)
                CardManager.Instance.AddCard(dpr.NewCard, GameManager.PLAYER, false);
        }
        currentDialogueClip = dc;
        if (currentDialogueClip is DialogueFork) currentDialogueClip = DialogueFork();
        DisplayDialoguePopup();
    }

    private DialogueClip DialogueFork()
    {
        DialogueFork df = currentDialogueClip as DialogueFork;
        DialogueClip nextClip = null;
        if (df.IsRespectCondition)
        {
            int respect = EngagedHero.RespectScore;
            if (respect <= df.Clip_2_Condition_Value) nextClip = df.Clip_1;
            else if (respect <= df.Clip_2_Condition_Value) nextClip = df.Clip_2;
            else nextClip = df.Clip_3;
        }
        return nextClip;
    }
}
