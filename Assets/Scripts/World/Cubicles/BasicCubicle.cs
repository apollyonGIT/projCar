using AutoCodes;
using Foundations.MVVM;
using System;
using World.Characters;
using World.Devices;
using World.Devices.Device_AI;

namespace World.Cubicles
{
    public interface ICubicleView : IModelView<BasicCubicle>
    {
        void notify_tick();

        void notify_set_worker(Character c);

        void notify_highlight(bool ret);
    }

    public class BasicCubicle : Model<BasicCubicle, ICubicleView>
    {
        public Character worker;
        public device_cubicles desc;

        public BasicCubicle(device_cubicles cub)
        {
            desc = cub;
        }

        public virtual void tick()
        {
            foreach (var view in views)
            {
                view.notify_tick();
            }
        }

        public void set_worker(Character c)
        {
            worker = c;
            foreach (var view in views)
            {
                view.notify_set_worker(c);
            }
        }

        public void HighlightCubicle(bool ret)
        {
            foreach(var view in views)
            {
                view.notify_highlight(ret);
            }
        }
    }

    public class DeviceCubicle : BasicCubicle
    {
        public Device device;

        public DeviceCubicle(device_cubicles cub) : base(cub)
        {
        }

        public override void tick()
        {


            foreach (var view in views)
            {
                view.notify_tick();
            }
        }
    }

    public class DeviceAttackCubicle : DeviceCubicle
    {
        public DeviceAttackCubicle(Device device, device_cubicles cub) : base(cub)
        {
            this.device = device;
        }

        public override void tick()
        {
            if (worker != null && worker.StateCanWork() && device != null && device is IAttack_New ia)
            {
                ia.TryToAutoAttack();
            }

            foreach (var view in views)
            {
                view.notify_tick();
            }
        }
    }

    public class DeviceLoadCubicle : DeviceCubicle
    {
        public DeviceLoadCubicle(Device device, device_cubicles cub) : base(cub)
        {
            this.device = device;
        }

        public override void tick()
        {
            if (worker != null && worker.StateCanWork() && device != null && device is IReload_New ir)
            {
                ir.TryToAutoReload();
            }
            foreach (var view in views)
            {
                view.notify_tick();
            }
        }
    }

    public class DeviceSharpCubicle : DeviceCubicle
    {
        public DeviceSharpCubicle(Device device, device_cubicles cub) : base(cub)
        {
            this.device = device;
        }
        public override void tick()
        {
            if (worker != null && worker.StateCanWork() && device != null && device is ISharpNew ish)
            {
                ish.TryToAutoSharp();
            }
            foreach (var view in views)
            {
                view.notify_tick();
            }
        }
    }

    public class DeviceRotateCubicle : DeviceCubicle
    {
        public DeviceRotateCubicle(Device device, device_cubicles cub) : base(cub)
        {
            this.device = device;
        }
        public override void tick()
        {
            if (worker != null && worker.StateCanWork() && device != null && device is IRotate_New ir)
            {
                ir.TryToAutoRotate();
            }
            foreach (var view in views)
            {
                view.notify_tick();
            }
        }
    }

    public class DeviceCarryCubicle : DeviceCubicle
    {
        public DeviceCarryCubicle(Device device, device_cubicles cub) : base(cub)
        {
            this.device = device;
        }
        public override void tick()
        {
            if (worker != null && worker.StateCanWork() && device != null && device is ICarry ic)
            {
                ic.TryToAutoCarry();
            }
            foreach (var view in views)
            {
                view.notify_tick();
            }
        }
    }

    public class DeviceChargeCubicle : DeviceCubicle
    {
        public DeviceChargeCubicle(Device device, device_cubicles cub) : base(cub)
        {
            this.device = device;
        }
        public override void tick()
        {
            if (worker != null && worker.StateCanWork() && device != null && device is ICharge ic)
            {
                ic.TryToAutoCharge();
            }
            foreach (var view in views)
            {
                view.notify_tick();
            }
        }
    }
}
