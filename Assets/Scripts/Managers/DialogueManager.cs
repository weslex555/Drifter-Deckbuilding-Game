using System.Collections;
using UnityEngine;
using TMPro;

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

    private GameManager gMan;
    private PlayerManager pMan;
    private UIManager uMan;
    private AudioManager auMan;
    private DialogueClip currentDialogueClip;
    private DialogueSceneDisplay dialogueDisplay;
    private string currentTypedText;
    private TextMeshProUGUI currentTmPro;
    private float typedTextDelay;
    private bool newEngagedHero;

    private bool allowResponse; // TESTING

    public DialogueSceneDisplay DialogueDisplay { get => dialogueDisplay; }
    public NPCHero EngagedHero { get; set; } // PUBLIC SET FOR COMBAT TEST BUTTON
    public Coroutine CurrentTextRoutine { get; private set; }
    private void Start()
    {
        gMan = GameManager.Instance;
        pMan = PlayerManager.Instance;
        uMan = UIManager.Instance;
        auMan = AudioManager.Instance;
        newEngagedHero = false;

        allowResponse = true; // TESTING
    }

    public void DisplayCurrentHeroes()
    {
        dialogueDisplay.PlayerHeroImage = pMan.PlayerHero.HeroPortrait;
        dialogueDisplay.PlayerHeroName = pMan.PlayerHero.HeroName;
        dialogueDisplay.NPCHeroImage = EngagedHero.HeroPortrait;
        dialogueDisplay.NPCHeroName = EngagedHero.HeroName;
    }

    public void StartDialogue()
    {
        auMan.StartStopSound("Soundtrack_Dialogue1",
            null, AudioManager.SoundType.Soundtrack);
        auMan.StopCurrentSoundscape(); // TESTING

        currentDialogueClip = EngagedHero.NextDialogueClip;
        if (currentDialogueClip == null)
        {
            Debug.LogError("CLIP IS NULL!");
            return;
        }
        dialogueDisplay = FindObjectOfType<DialogueSceneDisplay>();
        dialogueDisplay.PlayerHeroPortrait.SetActive(false);
        DialoguePrompt prompt = currentDialogueClip as DialoguePrompt;
        dialogueDisplay.NPCHeroPortrait.SetActive(!prompt.HideNPC);
        DisplayCurrentHeroes();
        DisplayDialoguePopup();
        AnimationManager.Instance.DialogueIntro();
    }

    public void EndDialogue()
    {
        EngagedHero = null;
        currentDialogueClip = null;
        StopTimedText();
    }

    public void DisplayDialoguePopup()
    {
        DialoguePrompt dpr = currentDialogueClip as DialoguePrompt;
        if (newEngagedHero)
        {
            AnimationManager.Instance.NewEngagedHero(false);
            dialogueDisplay.NPCHeroSpeech = "";
            newEngagedHero = false;

            allowResponse = false; // TESTING
            FunctionTimer.Create(() => allowResponse = true, 1.5f); // TESTING
            FunctionTimer.Create(() => DisplayDialoguePopup(), 1);
            return;
        }

        TimedText(dpr.DialoguePromptText, 
            dialogueDisplay.NPCHeroSpeechObject.GetComponent<TextMeshProUGUI>());

        // Response 1
        bool r1_Active = true;        
        if (string.IsNullOrEmpty(dpr.DialogueResponse1.ResponseText))
            r1_Active = false;
        dialogueDisplay.Response_1_Object.SetActive(r1_Active);
        if (r1_Active) 
            dialogueDisplay.Response_1 =
                FilterText(dpr.DialogueResponse1.ResponseText);

        // Response 2
        bool r2_Active = true;
        if (string.IsNullOrEmpty(dpr.DialogueResponse2.ResponseText))
            r2_Active = false;
        dialogueDisplay.Response_2_Object.SetActive(r2_Active);
        if (r2_Active)
            dialogueDisplay.Response_2 =
                FilterText(dpr.DialogueResponse2.ResponseText);

        // Response 3
        bool r3_Active = true;
        if (string.IsNullOrEmpty(dpr.DialogueResponse3.ResponseText))
            r3_Active = false;
        dialogueDisplay.Response_3_Object.SetActive(r3_Active);
        if (r3_Active)
            dialogueDisplay.Response_3 =
                FilterText(dpr.DialogueResponse3.ResponseText);
    }
    
    private string FilterText(string text)
    {
        if (pMan.PlayerHero != null)
        {
            string shortName = pMan.PlayerHero.HeroShortName;
            if (!string.IsNullOrEmpty(shortName))
                text = text.Replace("<HERO NAME>", shortName);
        }

        if (!GameManager.Instance.HideExplicitLanguage) 
            return text;
        string[] filterWords =
        {
            "fucking", "Fucking",
            "fucked", "Fucked",
            "fuckers", "Fuckers",
            "fucker", "Fucker",
            "fucks", "Fucks",
            "fuck", "Fuck",

            "bitching", "Bitching",
            "bitches", "Bitches",
            "bitch", "Bitch",

            "assholes", "Assholes",
            "asshole", "Asshole",

            "shitting", "Shitting",
            "shitters", "Shitters",
            "shitter", "Shitter",
            "shits", "Shits",
            "shit", "Shit"
        };

        string newText = text;
        foreach (string s in filterWords)
        {
            string stars = "";
            for (int i = 0; i < s.Length; i++) 
                stars += "*";
            newText = newText.Replace(s, stars);
        }
        return newText;
    }

    public void TimedText(string text, TextMeshProUGUI tmPro)
    {
        typedTextDelay = 0.05f;
        StopTimedText();
        string filteredText = FilterText(text);
        CurrentTextRoutine = 
            StartCoroutine(TimedTextNumerator(filteredText, tmPro));
    }
    public void StopTimedText(bool finishText = false)
    {
        if (CurrentTextRoutine != null)
        {
            StopCoroutine(CurrentTextRoutine);
            if (finishText) currentTmPro.SetText(currentTypedText);
            CurrentTextRoutine = null;
            currentTypedText = null;
            currentTmPro = null;
        }
    }
    private IEnumerator TimedTextNumerator(string text, TextMeshProUGUI tmPro)
    {
        currentTypedText = text;
        currentTmPro = tmPro;
        float delay = typedTextDelay;
        string fullText = text;
        string currentText;
        for (int i = 0; i < fullText.Length + 1; i++)
        {
            AudioManager.Instance.StartStopSound("SFX_Typing");
            currentText = fullText.Substring(0, i);
            tmPro.SetText(currentText);
            yield return new WaitForSeconds(delay);
        }
        CurrentTextRoutine = null;
    }

    public void DialogueResponse(int response)
    {
        if (allowResponse == false) // TESTING
            return;

        if (currentDialogueClip == null)
        {
            Debug.LogError("CURRENT CLIP IS NULL!");
            return;
        }
        if (CurrentTextRoutine != null)
        {
            StopTimedText(true);
            return;
        }

        DialoguePrompt prompt = currentDialogueClip as DialoguePrompt;
        DialogueResponse dResponse = null;

        switch (response)
        {
            case 1:
                dResponse = prompt.DialogueResponse1;
                break;
            case 2:
                dResponse = prompt.DialogueResponse2;
                break;
            case 3:
                dResponse = prompt.DialogueResponse3;
                break;
        }
        DialogueClip nextClip = dResponse.Response_NextClip;
        if (nextClip == null)
        {
            Debug.LogError("NEXT CLIP IS NULL!");
            return;
        }

        DialoguePrompt nextPrompt = null;
        if (nextClip is DialoguePrompt)
        {
            nextPrompt = nextClip as DialoguePrompt;
            if (nextPrompt.NewLocations.Length > 0)
            {
                foreach (NewLocation newLoc in nextPrompt.NewLocations)
                    gMan.GetActiveLocation(newLoc.Location, newLoc.NewNpc);
            }
        }

        // TESTING
        if (dResponse.NPC_NextClip != null)
        {
            EngagedHero.NextDialogueClip = dResponse.NPC_NextClip;
            Debug.LogWarning("NEXT CLIP: " + nextClip.ToString());
        }

        // New Location
        if (dResponse.Response_TravelLocation != null) // TESTING
        {
            Location location = dResponse.Response_TravelLocation;
            uMan.CreateTravelPopup(gMan.GetActiveLocation(location));
            return;
        }
        // Combat Start
        if (dResponse.Response_IsCombatStart)
        {
            SceneLoader.LoadScene(SceneLoader.Scene.CombatScene);
            if (nextClip is CombatRewardClip)
                EngagedHero.NextDialogueClip = nextClip; // TESTING
            else Debug.LogError("NEXT CLIP IS NOT COMBAT REWARD CLIP!");
            return;
        }
        // World Map Start
        if (dResponse.Response_IsWorldMapStart)
        {
            SceneLoader.LoadScene(SceneLoader.Scene.WorldMapScene);
            return;
        }
        // Recruitment
        if (dResponse.Response_IsRecruitmentStart)
        {
            uMan.CreateCardPagePopup(CardPageDisplay.CardPageType.RecruitUnit);
            return;
        }
        // Cloning
        if (dResponse.Response_IsShopStart)
        {
            uMan.CreateItemPagePopup();
            return;
        }
        // Cloning
        if (dResponse.Response_IsCloningStart)
        {
            uMan.CreateCardPagePopup(CardPageDisplay.CardPageType.CloneUnit);
            return;
        }
        // Exit
        if (dResponse.Response_IsExit)
        {
            uMan.CreateBetaFinishPopup(); // FOR BETA ONLY
            return;
        }
        if (nextPrompt != null)
        {
            // New Engaged Hero
            if (nextPrompt.NewEngagedHero != null)
            {
                EngagedHero = gMan.GetActiveNPC(nextPrompt.NewEngagedHero);
                newEngagedHero = true;
            }
            else
            {
                if (nextPrompt.HideNPC)
                {
                    if (!prompt.HideNPC)
                    {
                        AnimationManager.Instance.NewEngagedHero(true); // TESTING
                    }
                }
                /*
                {
                    if (prompt.HideNPC)
                    {
                        newEngagedHero = true;
                    }
                }
                */
            }
            // New Card
            if (nextPrompt.NewCard != null) UIManager.Instance.CreateNewCardPopup(nextPrompt.NewCard);
            else if (nextPrompt.AetherCells > 0)
            {
                int newAether = nextPrompt.AetherCells;
                int newTotal = newAether + pMan.AetherCells;
                UIManager.Instance.CreateAetherCellPopup(newAether, newTotal);
            }
        }

        currentDialogueClip = nextClip;
        if (currentDialogueClip is DialogueFork) currentDialogueClip = DialogueFork();
        if (nextPrompt != null && nextPrompt.NewCard == null && nextPrompt.AetherCells < 1 &&
            !dResponse.Response_IsRecruitmentStart) DisplayDialoguePopup();
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
