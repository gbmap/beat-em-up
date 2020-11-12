﻿using System;

namespace Catacumba.Data
{
    public enum EAttribute
    {
        Vigor,
        Strength,
        Dexterity,
        Magic
    }

    public abstract class TCharAttributes<T>
    {
        public T Vigor;
        public T Strength;
        public T Dexterity;
        public T Magic;

        public T GetAttr(EAttribute attr)
        {
            switch (attr)
            {
                case EAttribute.Vigor: return Vigor;
                case EAttribute.Strength: return Strength;
                case EAttribute.Magic: return Magic;
                case EAttribute.Dexterity: return Dexterity;
                default: throw new NotImplementedException(string.Format("Couldn't find attribute: {0}", attr.ToString()));
            }
        }

        public void SetAttr(EAttribute attr, T v)
        {
            switch (attr)
            {
                case EAttribute.Vigor: Vigor = v; break;
                case EAttribute.Strength: Strength = v; break;
                case EAttribute.Magic: Magic = v; break;
                case EAttribute.Dexterity: Dexterity = v; break;
                default: throw new NotImplementedException(string.Format("Couldn't find attribute: {0}", attr.ToString()));
            }
        }

        public abstract T Sum();
    }

    [Serializable] public class CharAttributesI : TCharAttributes<int> 
    {
        public override int Sum()
        {
            return Vigor + Strength + Dexterity + Magic;
        }  
    }
    [Serializable] public class CharAttributesF : TCharAttributes<float> 
    { 
        public override float Sum()
        {
            return Vigor + Strength + Dexterity + Magic;
        }  
    }

    public static class TCharAttributeExtension
    {
        public static void Add(this TCharAttributes<float> a, TCharAttributes<float> b)
        {
            a.Vigor += b.Vigor;
            a.Strength += b.Strength;
            a.Dexterity += b.Dexterity;
            a.Magic += b.Magic;
        }

        public static void Add(this TCharAttributes<int> a, TCharAttributes<int> b)
        {
            a.Vigor += b.Vigor;
            a.Strength += b.Strength;
            a.Dexterity += b.Dexterity;
            a.Magic += b.Magic;
        }
    }

}