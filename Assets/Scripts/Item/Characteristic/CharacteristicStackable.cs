using UnityEngine;

namespace Catacumba.Data.Items.Characteristics
{
    [CreateAssetMenu(menuName="Data/Item/Characteristic/Stackable", fileName="CharacteristicStackable")]
    public class CharacteristicStackable : ItemCharacteristic
    {
        public int CurrentAmount;
        public int MaxAmount;

        public void Stack(ref Item a, ref Item b)
        {
            if (!Item.Compare(a, b)) throw new System.Exception("Can't stack two different items.");
            if (Item.CompareInstance(a, b)) throw new System.Exception("Can't stack the same instance of items.");
            CharacteristicStackable sa = a.GetCharacteristic<CharacteristicStackable>();
            CharacteristicStackable sb = b.GetCharacteristic<CharacteristicStackable>();
            int amount = sa.CurrentAmount + sb.CurrentAmount;
            sa.CurrentAmount = Mathf.Min(amount, sa.MaxAmount);
            sb.CurrentAmount = Mathf.Max(0, amount - sa.CurrentAmount);

            if (sb.CurrentAmount == 0)
            {
                DestroyImmediate(sb);
                sb = null;

                DestroyImmediate(b);
                b = null;
            }
        }
    }
}