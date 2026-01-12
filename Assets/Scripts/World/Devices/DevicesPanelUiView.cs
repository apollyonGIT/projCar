using Commons;
using Foundations;
using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;

namespace World.Devices
{
    public class DevicesPanelUiView : MonoBehaviour,IDeviceMgrView
    {
        public DeviceMgr owner;

        public Transform content;
        public DeviceOverview prefab;

        public List<DeviceOverview> device_overviews = new List<DeviceOverview>();
        public void Init()
        {
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);

            for(int i = 0; i < 5; i++)
            {
                var device_overview = Instantiate(prefab, content, false);
                device_overviews.Add(device_overview);

                device_overviews[i].SetIndexImage(i);
                device_overview.gameObject.SetActive(true);
            }


            foreach(var slot in dmgr.slots_device)
            {
                var slot_name = slot._name;
                var device = slot.slot_device;
                switch (slot_name)
                {
                    case "slot_top":
                        device_overviews[0].Init(slot_name, device,owner);
                        break;
                    case "slot_front_top":
                        device_overviews[1].Init(slot_name, device, owner);
                        break;
                    case "slot_front":
                        device_overviews[2].Init(slot_name, device, owner);
                        break;
                    case "slot_back":
                        device_overviews[3].Init(slot_name, device, owner);
                        break;
                    case "slot_back_top":
                        device_overviews[4].Init(slot_name, device, owner);
                        break;
                    default:
                        break;
                }
            }
        }

        private void set_slot(string slot_name,Device device)
        {
            switch (slot_name)
            {
                case "slot_top":
                    device_overviews[0].SetOverview(slot_name, device);
                    break;
                case "slot_front_top":
                    device_overviews[1].SetOverview(slot_name, device);
                    break;
                case "slot_front":
                    device_overviews[2].SetOverview(slot_name, device);
                    break;
                case "slot_back":
                    device_overviews[3].SetOverview(slot_name, device);
                    break;
                case "slot_back_top":
                    device_overviews[4].SetOverview(slot_name, device);
                    break;
                default:
                    break;
            }
        }


        #region IDeviceMgrView Implementation
        void IModelView<DeviceMgr>.attach(DeviceMgr owner)
        {
            this.owner = owner;
            Init();
        }

        void IModelView<DeviceMgr>.detach(DeviceMgr owner)
        {
            if (this.owner != null)
                this.owner = null;
            Destroy(gameObject);
        }

        void IDeviceMgrView.notify_install_device(string slot_name, Device device)
        {
            set_slot(slot_name, device);
        }

        void IDeviceMgrView.notify_remove_device(string slot_name)
        {
            set_slot(slot_name, null);
        }
        void IDeviceMgrView.notify_tick()
        {
            foreach(var overview in device_overviews)
            {
                overview.tick();
            }
        }

        void IDeviceMgrView.notify_tick1()
        {
            
        }

        void IDeviceMgrView.notify_select_device(Device device)
        {

            foreach (var overview in device_overviews)
            {
                overview.UnSelect();
            }

            if(device!=null)
                device_overviews.Find(overview => overview.data == device)?.Select();
        }

        #endregion
    }
}
