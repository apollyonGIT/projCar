using System;
using World.Devices;
using World.Devices.Device_AI;

namespace World.Relic.Relics
{
    public class Relic_Reloading_Go_Off :Relic
    {
        public override void Get()
        {
            BattleContext.instance.load_event += immediatly_shoot;
        }

        private void immediatly_shoot(Device device)
        {
            var rate = desc.parm_int[0];
            var rnd = UnityEngine.Random.Range(0, 1000);

            if (rnd < rate)
            {
                if (device is DScript_Shooter bsc)
                {
                    bsc.EnforcedShoot();
                }
            }
        }

        public override void Drop()
        {
            BattleContext.instance.load_event -= immediatly_shoot;
        }
    }
}
