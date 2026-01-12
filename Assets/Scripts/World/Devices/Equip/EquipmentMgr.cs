using Commons;
using Foundations;
using Foundations.MVVM;
using Foundations.Tickers;
using System.Collections.Generic;

namespace World.Devices.Equip
{
    public interface IEquipmentMgrView : IModelView<EquipmentMgr>
    {
        void init();
        void add_device(Device device);
        void tick();
        void update_slot();
        void remove_device(Device device);
        void select_device(Device device);
    }
    public class EquipmentMgr :Model<EquipmentMgr,IEquipmentMgrView>,IMgr
    {
        public List<Device> devices = new();

        public Device select_device = null;

        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        public EquipmentMgr(string name, int priority,params object[] args)
        {
            m_mgr_name = name;
            m_mgr_priority = priority;

            (this as IMgr).init(args);
        }

        public void Init(string[] devices_id)
        {
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            foreach(var d in devices_id)
            {
                devices.Add(dmgr.GetDevice(d));
            }

            foreach(var view in views)
            {
                view.init();
            }
        }

        public void tick()
        {
            
        }

        public void tick1()
        {

        }

        public void RemoveDevice(Device device)
        {
            devices.Remove(device); 
            foreach(var view in views)
            {
                view.remove_device(device);
            }
        }

        public void AddDevice(Device device)
        {
            if (device == null)
                return;
            devices.Add(device);
            foreach (var view in views)
            {
                view.add_device(device);
            }
        }

        public void SelectDevice(Device device)
        {
            select_device = device;
            foreach(var view in views)
            {
                view.select_device(device);
            }
        }

        public void UpdateSlot()
        {
            foreach(var view in views)
            {
                view.update_slot();
            }
        }

        void IMgr.init(params object[] args)
        {
            Mission.instance.attach_mgr(m_mgr_name, this);

            var ticker = Ticker.instance;
            ticker.add_tick(m_mgr_priority, m_mgr_name, tick);
            ticker.add_tick1(m_mgr_priority, m_mgr_name, tick1);
        }

        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Ticker.instance;
            ticker.remove_tick(m_mgr_name);
            ticker.remove_tick1(m_mgr_name);
        }
    }
}
