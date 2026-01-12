
using Commons;

namespace World.Devices.DeviceEmergencies
{
    public class DeviceStupor : DeviceEmergency
    {
        public DeviceStupor(Device owner)
        {
            this.owner = owner;
        }
        public override void tick()
        {
            base.tick();
        }

        public void Kick()
        {
            owner.Hurt((int)Config.current.stupor_kick_damage);
            var rate = UnityEngine.Random.Range(0f,1f);

            if(rate < Config.current.stupor_kick_fix_rate)
            {
                removed = true;
            }
        }

        protected override void self_recover()
        {
            var rate = UnityEngine.Random.Range(0f, 1f);

            if (rate < Config.current.stupor_normal_fix_area)
            {
                removed = true;
            }
        }

        public void Fix()
        {
            var rate = UnityEngine.Random.Range(0f, 1f);

            if (rate < Config.current.stupor_normal_fix_area)
            {
                removed = true;
            }
        }
    }
}
