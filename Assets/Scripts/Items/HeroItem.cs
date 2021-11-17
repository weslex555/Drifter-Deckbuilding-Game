using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hero Item", menuName = "Hero Items/Hero Item")]
public class HeroItem : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] [TextArea] private string itemDescription;
    [SerializeField] private Sprite itemImage;
    [SerializeField] private List<EffectGroup> effectGroupList;

    public string ItemName { get => itemName; }
    public string ItemDescription { get => itemDescription; }
    public Sprite ItemImage { get => itemImage; }
    public List<EffectGroup> EffectGroupList { get => effectGroupList; }
}
