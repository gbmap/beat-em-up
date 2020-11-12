using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Catacumba.Data.Items;
using Catacumba.Data.Items.Characteristics;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ItemTests
    {
        public static Item[] GetItems()
        {
            return Resources.LoadAll<Item>(string.Empty);
        }

        [Test]
        public void Test_Compare()
        {
            Item[] items = GetItems();

            Item i1 = items[0];
            Item i2 = i1.Clone();

            Assert.True(Item.Compare(i1, i2));
            Assert.False(Item.CompareInstance(i1, i2));
        }

        [Test]
        public void Test_NewInstanceDoesntChangeOriginal()
        {
            Item i1 = GetItems().Where(i=>i.HasCharacteristic<CharacteristicStackable>())
                                .FirstOrDefault();
            CharacteristicStackable s1 = i1.GetCharacteristic<CharacteristicStackable>();

            Item i2 = i1.Clone();
            CharacteristicStackable s2 = i2.GetCharacteristic<CharacteristicStackable>();

            i2.Name = "TEST";
            i2.Description = "FAUWHAIUWHDIUAW$";
            s2.CurrentAmount += 100;

            Assert.AreNotEqual(i1.Name, i2.Name);
            Assert.AreNotEqual(i1.Description, i2.Description);
            Assert.AreNotEqual(s1.CurrentAmount, s2.CurrentAmount);
        }
    }
}