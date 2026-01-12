using World.Devices.Device_AI;
using World.Helpers;

namespace World.Relic.Relics
{
    public class Relic_Landing_Trigger_Shoot : Relic
    {
        float cd = 0f;
        int current_tick_cd = 0;

        public override void Get()
        {
            BattleContext.instance.land_event += do_shoot;
            cd = desc.parm_int[0] / 1000f;
            current_tick_cd = 0;
        }

        public override void Tick()
        {
            base.Tick();

            if (current_tick_cd <= 0)
            {
                current_tick_cd = 0;
            }
            else
            {
                current_tick_cd--;
            }
        }

        private void do_shoot()
        {
            if(current_tick_cd > 0)
            {
                return;
            }

            current_tick_cd =(int)( cd * Commons.Config.PHYSICS_TICKS_PER_SECOND);

            var devices = Device_Slot_Helper.GetAllDevice();

            foreach (var device in devices)
            {
                if(device is DScript_Shooter ds)
                {
                    ds.EnforcedShoot();
                }
            }
        }

        public override void Drop()
        {
            BattleContext.instance.land_event -= do_shoot;
        }
    }
}
