using UnityEngine;

public class PowerZoom : MonoBehaviour
{
    [SerializeField] private GameObject powerPopupPrefab;
    [SerializeField] private GameObject abilityPopupBoxPrefab;
    [SerializeField] private GameObject abilityPopupPrefab;
    [SerializeField] private bool abilityPopupOnly;
    public HeroPower LoadedPower { get; set; }

    private GameObject powerPopup;
    private GameObject abilityPopupBox;

    private const string POWER_POPUP_TIMER = "PowerPopupTimer";
    private const string ABILITY_POPUP_TIMER = "AbilityBoxTimer";

    public void OnPointerEnter()
    {
        if (CardZoom.ZoomCardIsCentered || DragDrop.DraggingCard) return;
        if (powerPopup != null) Destroy(powerPopup);
        if (!abilityPopupOnly) FunctionTimer.Create(() => CreatePowerPopup(), 1, POWER_POPUP_TIMER);
        else FunctionTimer.Create(() => ShowLinkedAbilities(LoadedPower, 3), 0.5f, POWER_POPUP_TIMER);
    }
    public void OnPointerExit()
    {
        FunctionTimer.StopTimer(POWER_POPUP_TIMER);
        FunctionTimer.StopTimer(ABILITY_POPUP_TIMER);
        if (powerPopup != null)
        {
            Destroy(powerPopup);
            powerPopup = null;
        }
        if (abilityPopupBox != null)
        {
            Destroy(abilityPopupBox);
            abilityPopupBox = null;
        }
    }

    private void CreatePowerPopup()
    {
        Transform tran = CardManager.Instance.PlayerHero.transform;
        float newX = tran.position.x - 200;
        float newY = tran.position.y + 250;
        Vector3 spawnPoint = new Vector2(newX, newY);
        float scaleValue = 2.5f;
        powerPopup = Instantiate(powerPopupPrefab, spawnPoint, Quaternion.identity);
        powerPopup.transform.localScale = new Vector2(scaleValue, scaleValue);
        HeroPower hp = gameObject.GetComponentInParent<PlayerHeroDisplay>().PlayerHero.HeroPower;
        powerPopup.GetComponent<PowerPopupDisplay>().PowerScript = hp;
        FunctionTimer.Create(() => ShowLinkedAbilities(hp, scaleValue), 1.5f, ABILITY_POPUP_TIMER);
    }

    private void ShowLinkedAbilities(HeroPower hp, float scaleValue)
    {
        abilityPopupBox = Instantiate(abilityPopupBoxPrefab, 
            UIManager.Instance.CurrentWorldSpace.transform);
        Transform tran = abilityPopupBox.transform;
        Vector2 position = new Vector2();
        if (!abilityPopupOnly) position.Set(0, -100);
        else position.Set(350, -50);
        tran.localPosition = position;
        tran.localScale = new Vector2(scaleValue, scaleValue);
        foreach (CardAbility ca in hp.LinkedAbilities) 
            CreateAbilityPopup(ca, abilityPopupBox.transform, 1);
    }

    private void CreateAbilityPopup(CardAbility ca, Transform parent, float scaleValue)
    {
        GameObject abilityPopup = Instantiate(abilityPopupPrefab, parent);
        abilityPopup.transform.localScale = new Vector2(scaleValue, scaleValue);
        abilityPopup.GetComponent<AbilityPopupDisplay>().AbilityScript = ca;
    }
}
