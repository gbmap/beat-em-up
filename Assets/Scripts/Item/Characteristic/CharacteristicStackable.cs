



using UnityEngine;

namespace Catacumba.Data.Items.Characteristics
{
    public class CharacteristicStackable : ItemCharacteristic
    {
        public int CurrentAmount;
        public int MaxAmount;

        public void Stack(ref Item a, ref Item b)
        {
            if (a != b) return;
            CharacteristicStackable sa = a.GetCharacteristic<CharacteristicStackable>();
            CharacteristicStackable sb = b.GetCharacteristic<CharacteristicStackable>();
            int amount = sa.CurrentAmount + sb.CurrentAmount;
            sa.CurrentAmount = Mathf.Min(amount, sa.MaxAmount);
            sb.CurrentAmount = Mathf.Max(0, amount - sa.CurrentAmount);

            if (sb.CurrentAmount == 0)
            {
                Destroy(sb);
                sb = null;
            }
        }
    }
}