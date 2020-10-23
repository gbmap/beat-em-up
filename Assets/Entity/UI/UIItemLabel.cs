using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Catacumba.Entity;

public class UIItemLabel : MonoBehaviour
{
    public Text Title;
    public Text Rarity;
    public Text Description;
    public Text Attributes;
    public Text DamageScaling;
    
    public void SetItemData(ItemConfig item)
    {
        Title.text = item.Name;
        Rarity.text = string.Format("({0})", item.Stats.Rarity.ToString());
        Description.text = item.Description;

        if (item.Stats.ItemType == EItemType.Key ||
            item.Stats.ItemType == EItemType.Consumable)
        {
            Attributes.text = string.Empty;
            DamageScaling.text = string.Empty;
        }
        else
        {
            Attributes.text = AttributesToString(item.Stats.Attributes);
            DamageScaling.text = DamageScalingToString(item.Stats.DamageScaling);
        }
    }

    private string AttributesToString(CharAttributesI attr)
    {
        return FormatAttributeString(AttributeToString, attr.Vigor, " Vigor") +
               FormatAttributeString(AttributeToString, attr.Strength, " Strength") +
               FormatAttributeString(AttributeToString, attr.Dexterity, " Dexterity") +
               FormatAttributeString(AttributeToString, attr.Magic, " Magic", true);
    }

    private string AttributeToString(int attr)
    {
        return AttributeToString((float)attr);
    }

    private string AttributeToString(float attr)
    {
        return attr == 0 ? "" : ((attr > 0 ? "+" : "-") + attr.ToString());
    }

    private string DamageScaleToString(float d)
    {
        return AttributeToString(d*100f) + "%";
    }

    private string FormatAttributeString<T>(Func<T, string> attrToString, T value, string attrName, bool end=false)
    {
        return attrToString(value) + attrName + (end?"":"\n");
    }

    private string DamageScalingToString(CharAttributesF dmgScaling)
    {
        return FormatAttributeString(DamageScaleToString, dmgScaling.Vigor, " Vigor") +
               FormatAttributeString(DamageScaleToString, dmgScaling.Strength, " Strength") +
               FormatAttributeString(DamageScaleToString, dmgScaling.Dexterity, " Dexterity") +
               FormatAttributeString(DamageScaleToString, dmgScaling.Magic, " Magic", true);
    }


}
