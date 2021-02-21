using QFSW.QC;

namespace Catacumba.Configuration
{
    [CommandPrefix("char.")]
    public static class CharacterVariables
    {
        [Command("dash_duration")]
        public static float DashDuration = 0.75f;

        [Command("attack_dash_force_weak")]
        public static float AttackDashForceWeak = 5f;

        [Command("attack_dash_force_strong")]
        public static float AttackDashForceStrong = 10f;

        [Command("damage_dash_force_weak")]
        public static float DamageDashForceWeak = 5f;

        [Command("damage_dash_force_strong")]
        public static float DamageDashForceStrong = 10f;

        [Command("knockdown_dash_force")]
        public static float KnockdownDashForce = 10f;

        [Command("freeze_duration")]
        public static float FreezeDuration = 0.125f;

        [Command("freeze_time_power")]
        public static float FreezeTimePower = 10f;
    }

}