using System.Collections;
using System.Collections.Generic;
using Catacumba.Data;
using Catacumba.Entity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class CharacterComponentConfigurationTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void CharacterComponentConfigurationTestsSimplePasses()
        {
            string configurationPath = "Data/Characters/ComponentConfigurations/ComponentConfigurationDefault";
            var configuration = Resources.Load<CharacterComponentConfiguration>(configurationPath);
            
            GameObject instance = new GameObject("Instance_Test");
            configuration.AddComponentsToObject(instance);

            Assert.IsNotNull(instance.GetComponent<CharacterHealth>());
            Assert.IsNotNull(instance.GetComponent<CharacterMovementBase>());
            Assert.IsNotNull(instance.GetComponent<CharacterCombat>());
        }
    }
}
