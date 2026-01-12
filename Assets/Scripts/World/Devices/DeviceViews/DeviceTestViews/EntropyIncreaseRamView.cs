using UnityEngine;
using World.Devices.Device_AI;

namespace World.Devices.DeviceViews
{
    public class EntropyIncreaseRamView :DeviceView_Spine
    {
        public override void notify_on_tick()
        {
            base.notify_on_tick();
            if (owner is EntropyIncreaseRam eir)
            {
                transform.localScale = Vector3.one * eir.entropy * BattleContext.instance.melee_scale_factor;
            }
        }
    }
}
