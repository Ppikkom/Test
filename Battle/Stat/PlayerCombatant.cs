using DeulPie.ExtraInk.Core.Managers;
using UnityEngine;

public class PlayerCombatant : BaseCombatant
{
    public override void BattleStart()
    {
        base.BattleStart();

        if (EquipmentManager.Instance.impactGearSlot != null)
        {
            공격력 += EquipmentManager.Instance.impactGearSlot.attackBonus + (EquipmentManager.Instance.impactGearSlot.bonusMartialArt == mainArt ? EquipmentManager.Instance.impactGearSlot.martialArtStatBonus : 0);
            방어력 += EquipmentManager.Instance.impactGearSlot.defenseBonus + (EquipmentManager.Instance.impactGearSlot.bonusMartialArt == mainArt ? EquipmentManager.Instance.impactGearSlot.martialArtStatBonus : 0);
        }

        if (EquipmentManager.Instance.physicalGearSlot != null)
        {
            공격력 += EquipmentManager.Instance.physicalGearSlot.attackBonus + (EquipmentManager.Instance.physicalGearSlot.bonusMartialArt == mainArt ? EquipmentManager.Instance.physicalGearSlot.martialArtStatBonus : 0);
            방어력 += EquipmentManager.Instance.physicalGearSlot.defenseBonus + (EquipmentManager.Instance.physicalGearSlot.bonusMartialArt == mainArt ? EquipmentManager.Instance.physicalGearSlot.martialArtStatBonus : 0);
        }

        if (EquipmentManager.Instance.supportGearSlot != null)
        {
            공격력 += EquipmentManager.Instance.supportGearSlot.attackBonus + (EquipmentManager.Instance.supportGearSlot.bonusMartialArt == mainArt ? EquipmentManager.Instance.supportGearSlot.martialArtStatBonus : 0);
            방어력 += EquipmentManager.Instance.supportGearSlot.defenseBonus + (EquipmentManager.Instance.supportGearSlot.bonusMartialArt == mainArt ? EquipmentManager.Instance.supportGearSlot.martialArtStatBonus : 0);
        }
    }

    public override void 행동력_변경(int v)
    {
        현재_행동력 += v;
    }
}