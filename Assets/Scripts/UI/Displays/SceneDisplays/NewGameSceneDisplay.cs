using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewGameSceneDisplay : MonoBehaviour
{
    [SerializeField] private GameObject selectedHero;
    [SerializeField] private GameObject skillCard_1;
    [SerializeField] private GameObject skillCard_2;
    [SerializeField] private GameObject heroPortrait;
    [SerializeField] private GameObject heroName;
    [SerializeField] private GameObject heroDescription;
    [SerializeField] private GameObject heroPowerImage;
    [SerializeField] private GameObject heroPowerDescription;

    [SerializeField] private GameObject selectedAugment;
    [SerializeField] private GameObject augmentName;
    [SerializeField] private GameObject augmentImage;
    [SerializeField] private GameObject augmentDescription;

    [SerializeField] private PlayerHero[] playerHeroes;
    [SerializeField] private HeroAugment[] heroAugments;

    private CombatManager coMan;
    private GameObject currentSkill_1;
    private GameObject currentSkill_2;
    private int currentSelection;
    private bool heroSelected;
    private bool augmentSelected;
    private PlayerHero SelectedHero { get => playerHeroes[currentSelection]; }
    private HeroAugment SelectedAugment { get => heroAugments[currentSelection]; }

    private void Start()
    {
        coMan = CombatManager.Instance;
        currentSelection = 0;
        heroSelected = false;
        augmentSelected = false;
        DisplaySelectedHero();
    }

    public void SelectBackButton()
    {
        if (heroSelected)
        {
            heroSelected = false;
            PlayerManager pm = PlayerManager.Instance;
            Destroy(pm.PlayerHero);
            pm.PlayerHero = null;
            currentSelection = 0;
            DisplaySelectedHero();
        }
        else GameManager.Instance.EndGame();
    }
    public void SelectRightArrow() => NextSelection(RightOrLeft.Right);
    public void SelectLeftArrow() => NextSelection(RightOrLeft.Left);
    public enum RightOrLeft { Right, Left }
    private void NextSelection(RightOrLeft rol)
    {
        int lastSelection;
        if (!heroSelected) lastSelection = playerHeroes.Length - 1;
        else lastSelection = heroAugments.Length - 1;
        
        if (rol == RightOrLeft.Right)
        { if (++currentSelection > lastSelection) currentSelection = 0; }
        else
        { if (--currentSelection < 0) currentSelection = lastSelection; }
        
        if (!heroSelected) DisplaySelectedHero();
        else DisplaySelectedAugment();
    }

    private void DisplaySelectedAugment()
    {
        selectedAugment.SetActive(true);
        selectedHero.SetActive(false);
        skillCard_1.SetActive(false);
        skillCard_2.SetActive(false);

        augmentName.GetComponent<TextMeshProUGUI>().SetText(SelectedAugment.AugmentName);
        augmentImage.GetComponent<Image>().sprite = SelectedAugment.AugmentImage;
        augmentDescription.GetComponent<TextMeshProUGUI>().SetText(SelectedAugment.AugmentDescription);
    }

    private void DisplaySelectedHero()
    {
        selectedHero.SetActive(true);
        selectedAugment.SetActive(false);
        skillCard_1.SetActive(true);
        skillCard_2.SetActive(true);

        heroName.GetComponent<TextMeshProUGUI>().SetText(SelectedHero.HeroName);
        heroPortrait.GetComponent<Image>().sprite = SelectedHero.HeroPortrait;
        heroDescription.GetComponent<TextMeshProUGUI>().SetText(SelectedHero.HeroDescription);
        heroPowerImage.GetComponent<Image>().sprite = SelectedHero.HeroPower.PowerSprite;
        heroPowerImage.GetComponentInParent<PowerZoom>().LoadedPower = SelectedHero.HeroPower;

        Sound[] snd = SelectedHero.HeroPower.PowerSounds;
        foreach (Sound s in snd) AudioManager.Instance.StartStopSound(null, s);

        int cost = SelectedHero.HeroPower.PowerCost;
        string actions;
        if (cost > 1) actions = "actions";
        else actions = "action";
        string description = " (" + cost + " " + actions + ", 1/turn): ";

        heroPowerDescription.GetComponent<TextMeshProUGUI>().SetText(SelectedHero.HeroPower.PowerName +
            description + SelectedHero.HeroPower.PowerDescription);

        if (currentSkill_1 != null)
        {
            Destroy(currentSkill_1);
            currentSkill_1 = null;
        }
        if (currentSkill_2 != null)
        {
            Destroy(currentSkill_2);
            currentSkill_2 = null;
        }

        currentSkill_1 = coMan.ShowCard(SelectedHero.HeroStartSkills[0]);
        currentSkill_2 = coMan.ShowCard(SelectedHero.HeroStartSkills[1]);
        currentSkill_1.transform.SetParent(skillCard_1.transform, false);
        currentSkill_2.transform.SetParent(skillCard_2.transform, false);
        Vector2 vec2 = new Vector2(4, 4);
        currentSkill_1.transform.localScale = vec2;
        currentSkill_2.transform.localScale = vec2;
    }

    public void ConfirmSelection()
    {
        PlayerManager pm = PlayerManager.Instance;
        if (!heroSelected)
        {
            PlayerHero newPH = ScriptableObject.CreateInstance<PlayerHero>();
            newPH.LoadHero(SelectedHero);
            pm.PlayerHero = newPH;
            heroSelected = true;
            currentSelection = 0;
            DisplaySelectedAugment();
        }
        else if (!augmentSelected)
        {
            HeroAugment ha = SelectedAugment;
            pm.HeroAugments.Add(ha);
            augmentSelected = true;
            GameManager.Instance.NextNarrative = pm.PlayerHero.HeroBackstory;
            SceneLoader.LoadScene(SceneLoader.Scene.NarrativeScene);
        }
        else Debug.LogWarning("HERO + AUGMENT already confirmed!");
    }
}
