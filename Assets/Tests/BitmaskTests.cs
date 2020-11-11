using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class BitmaskTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void BitmaskTestsSimplePasses()
        {
            Debug.Log(1 << 0);
            Debug.Log(1 << 1);


            Bitmask bm = new Bitmask();
            for (ushort i = 0; i < 6500; i++) {
                bm.Set(i, true);
                Assert.True(bm.Get(i));

                bm.Set(i, false);
                Assert.False(bm.Get(i));
            }
        }

    }
}
