﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Catacumba.Data.Items;
using Catacumba.Data.Items.Characteristics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class InventoryBagTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void Test_Grab()
        {
            Item[] items = ItemTests.GetItems();
            InventoryBag bag = new InventoryBag();

            bag.Grab(items[0]);
            
            Item itemInBag = bag.Get(0);
            Assert.IsTrue(Item.Compare(items[0], itemInBag));
            Assert.IsFalse(Item.CompareInstance(items[0], itemInBag));
        }

        [Test]
        public void Test_Stack()
        {
            InventoryBag bag = new InventoryBag();

            Item[] items = ItemTests.GetItems();
            Item item = items.FirstOrDefault(i => i.HasCharacteristic<CharacteristicStackable>());

            Item i1 = item.Clone();
            var s1 = i1.GetCharacteristic<CharacteristicStackable>();
            s1.CurrentAmount = (int)(s1.MaxAmount*0.25f);

            bag.Grab(i1);

            Item i2 = item.Clone();
            var s2 = i2.GetCharacteristic<CharacteristicStackable>();
            s2.CurrentAmount = (int)(s1.MaxAmount*0.1f);

            int targetAmount = s1.CurrentAmount + s2.CurrentAmount;

            bag.Grab(i2);

            Assert.AreEqual(1, bag.NumberOfItems);
            Assert.True(i2 == null);
            Assert.True(s2 == null);
            Assert.AreEqual(targetAmount, s1.CurrentAmount);
        }
    }
}
