using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Catacumba.LevelGen;

namespace Tests
{
    public class DirectionHelperTests
    {

        [Test]
        public void Test_BitmaskCreation()
        {
            EDirectionBitmask mask =EDirectionBitmask.None;
            Assert.AreEqual("00000", DirectionHelper.ToString(mask));
            Assert.False(DirectionHelper.IsSet(mask, EDirectionBitmask.Up));
            Assert.False(DirectionHelper.IsSet(mask, EDirectionBitmask.Down));
            Assert.False(DirectionHelper.IsSet(mask, EDirectionBitmask.Left));
            Assert.False(DirectionHelper.IsSet(mask, EDirectionBitmask.Right));
        }
        
        [Test]
        public void DirectionHelperSetBitmask()
        {
            EDirectionBitmask mask =EDirectionBitmask.None;
            DirectionHelper.Set(ref mask, EDirectionBitmask.Up);
            Assert.AreEqual("01000", DirectionHelper.ToString(mask));
            Assert.That(DirectionHelper.IsSet(mask, EDirectionBitmask.Up));

            DirectionHelper.Set(ref mask, EDirectionBitmask.Left);
            Assert.AreEqual("01001", DirectionHelper.ToString(mask));

            DirectionHelper.Set(ref mask, EDirectionBitmask.Right);
            Assert.AreEqual("01101", DirectionHelper.ToString(mask));

            DirectionHelper.Set(ref mask, EDirectionBitmask.Down);
            Assert.AreEqual("01111", DirectionHelper.ToString(mask));
        }

    }
}
