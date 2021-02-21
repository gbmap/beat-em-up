using UnityEngine;

namespace Catacumba.Events
{
    public class OnPlayerHit 
    {
        public AttackResult Attack { get; set; }
    }

    public class OnPlayerDamaged
    {
        public AttackResult Attack { get; set; }
    }
}