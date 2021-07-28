using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Hero Power", menuName = "Heroes/Hero Power")]
public class HeroPower : ScriptableObject
{
    [Header("POWER NAME")]
    public string PowerName;
    [Header("POWER COST")]
    public int PowerCost;
    [Header("POWER IMAGE")]
    public Sprite PowerSprite;
    [Header("POWER SOUND")]
    public Sound[] PowerSounds;
    [Header("POWER DESCRIPTION")]
    [TextArea]
    public string PowerDescription;
    [Header("EFFECT GROUPS")]
    public List<EffectGroup> EffectGroupList;
}
