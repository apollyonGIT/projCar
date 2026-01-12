using Foundations.MVVM;
using System;

namespace World.Devices.DeviceEmergencies
{
    public interface IDeviceEmergencyView : IModelView<DeviceEmergency>
    {
        void init();
        void reinit();
        void tick();
    }
    public class DeviceEmergency : Model<DeviceEmergency,IDeviceEmergencyView>
    {
        public bool removed = false;
        public Device owner;

        protected int recover_tick_max = 600;
        protected int recover_tick_current = 0;

        protected virtual void self_recover()
        {
            
        }

        public virtual void start()
        {

        }

        public virtual void end()
        {

        }
        public virtual void tick()
        {

            if(owner!=null && owner.current_hp <= 0 || owner.is_validate == false)
            {
                removed = true;
            }

            recover_tick_current++;

            if(recover_tick_current >= recover_tick_max)
            {
                self_recover();
                recover_tick_current = 0;
            }

            foreach (var view in views)
            {
                view.tick();
            }
        }

        public void init()
        {
            foreach(var view in views)
            {
                view.init();
            }
        }
    }
}
