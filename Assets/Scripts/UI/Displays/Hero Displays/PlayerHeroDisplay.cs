using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHeroDisplay : HeroDisplay
{
    [SerializeField] private GameObject heroEnergy;
    [SerializeField] private GameObject skillDrawnIcon;
    [SerializeField] private GameObject skillsLeft;
    [SerializeField] private GameObject heroPower;
    [SerializeField] private GameObject powerImage;
    [SerializeField] private GameObject powerCost;
    [SerializeField] private GameObject powerUsedIcon;

    [SerializeField] private GameObject heroUltimate;
    [SerializeField] private GameObject ultimateImage;
    [SerializeField] private GameObject ultimateCost;
    [SerializeField] private GameObject ultimateUsedIcon;

    [SerializeField] private GameObject ultimateProgressBar;
    [SerializeField] private GameObject ultimateProgressFill;
    [SerializeField] private GameObject ultimateProgressText;

    public PlayerHero PlayerHero { get => HeroScript as PlayerHero; }
    public GameObject HeroEnergy { get => heroEnergy; }
    public GameObject SkillDrawnIcon { get => skillDrawnIcon; }
    public int SkillsLeft
    {
        set
        {
            skillsLeft.GetComponent<TextMeshProUGUI>().SetText(value.ToString());
        }
    }
    public GameObject HeroPower { get => heroPower; }
    public GameObject PowerUsedIcon { get => powerUsedIcon; }
    public GameObject HeroUltimate { get => heroUltimate; }
    public GameObject UltimateUsedIcon { get => ultimateUsedIcon; }
    public GameObject UltimateProgressBar { get => ultimateProgressBar; }
    public GameObject UltimateProgressFill { get => ultimateProgressFill; }
    public string UltimateProgressText
    {
        set
        {
            ultimateProgressText.GetComponent<TextMeshProUGUI>().SetText(value);
        }
    }
    public string PlayerActions
    {
        set => heroEnergy.GetComponent<TextMeshProUGUI>().SetText(value);
    }

    public override void DisplayHero()
    {
        base.DisplayHero();
        powerImage.GetComponent<Image>().sprite = PlayerHero.HeroPower.PowerSprite;
        powerCost.GetComponent<TextMeshProUGUI>().SetText(PlayerHero.HeroPower.PowerCost.ToString());
        ultimateImage.GetComponent<Image>().sprite = PlayerHero.HeroUltimate.PowerSprite;
        ultimateCost.GetComponent<TextMeshProUGUI>().SetText(PlayerHero.HeroUltimate.PowerCost.ToString());
    }
}
