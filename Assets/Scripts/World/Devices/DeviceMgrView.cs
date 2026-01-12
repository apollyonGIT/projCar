using Addrs;
using AutoCodes;
using Foundations.MVVM;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.DeviceUiViews;

namespace World.Devices
{
    [Serializable]
    public class DeviceUiPanelPos
    {
        public Transform pos,special_pos;
        public string slot_name;
        public DeviceHpSlider hp_slider;

        [HideInInspector]
        public DeviceUiView device_ui_view;
    }

    public class DeviceMgrView : MonoBehaviour, IDeviceMgrView
    {
        public List<DeviceUiPanelPos> device_ui_panels_pos = new();

        public DeviceMgr owner;

        void IModelView<DeviceMgr>.attach(DeviceMgr owner)
        {
            this.owner = owner;

            init();
        }

        void IModelView<DeviceMgr>.detach(DeviceMgr owner)
        {
            Destroy(gameObject);
        }

        private void init()
        {
            foreach(var slot in owner.slots_device)
            {
                var device = slot.slot_device;
                var slot_name = slot._name;
                if (device != null)
                {
                    var duv = device_ui_panels_pos.Find(panel => panel.slot_name == slot_name);
                    if (duv == null)
                    {
                        Debug.LogWarning($"DeviceUiPanelPos not found for slot_name: {slot_name}");
                        continue;
                    }
                    else
                    {
                        Addressable_Utility.try_load_asset<DeviceUiView>(device.desc.ui_prefab, out var device_ui_view);
                        if (duv.special_pos != null && device.desc.size == 1)
                        {
                            duv.device_ui_view = Instantiate(device_ui_view, duv.special_pos, false);
                        }
                        else
                        {
                            duv.device_ui_view = Instantiate(device_ui_view, duv.pos, false);
                        }
                        device.add_view(duv.device_ui_view);
                        duv.hp_slider.gameObject.SetActive(true);

                        device_ranks.TryGetValue(device.desc.rank.ToString(), out var dr);
                        Addressable_Utility.try_load_asset<Sprite>(dr.img_panel_rank, out var sprite);

                        duv.hp_slider.InitRank(sprite);
                    }
                }
            }
        }

        void IDeviceMgrView.notify_install_device(string slot_name, Device device)
        {
            var duv = device_ui_panels_pos.Find(panel => panel.slot_name == slot_name);
            if(duv == null)
            {
                Debug.LogWarning($"DeviceUiPanelPos not found for slot_name: {slot_name}");
                return;
            }
            else
            {
                Addressable_Utility.try_load_asset<DeviceUiView>(device.desc.ui_prefab, out var device_ui_view);

                if(duv.special_pos != null && device.desc.size == 1)
                {
                    duv.device_ui_view = Instantiate(device_ui_view, duv.special_pos, false);
                }
                else
                {
                    duv.device_ui_view = Instantiate(device_ui_view, duv.pos, false);
                }

                device.add_view(duv.device_ui_view);
                duv.hp_slider.gameObject.SetActive(true);

                device_ranks.TryGetValue(device.desc.rank.ToString(), out var dr);
                Addressable_Utility.try_load_asset<Sprite>(dr.img_panel_rank, out var sprite);

                duv.hp_slider.InitRank(sprite);
            }
        }

        void IDeviceMgrView.notify_remove_device(string slot_name)
        {
            var duv = device_ui_panels_pos.Find(panel => panel.slot_name == slot_name);
            if (duv == null)
            {
                Debug.LogWarning($"DeviceUiPanelPos not found for slot_name: {slot_name}");
                return;
            }
            else
            {
                duv.device_ui_view = null;
                duv.hp_slider.gameObject.SetActive(false);
            }
        }

        void IDeviceMgrView.notify_select_device(Device device)
        {
            
        }

        void IDeviceMgrView.notify_tick()
        {
            foreach(var duv in device_ui_panels_pos)
            {
                if (duv.device_ui_view != null && duv.device_ui_view.owner != null)
                {
                    duv.hp_slider.UpdateHp((float)duv.device_ui_view.owner.current_hp / (float)duv.device_ui_view.owner.battle_data.hp);
                }
            }
        }

        void IDeviceMgrView.notify_tick1()
        {
            
        }
    }
}
