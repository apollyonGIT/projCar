using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Business;
using World.Devices.DeviceUpgrades;
using World.Helpers;
using static Foundations.Excels.Device_Type;

namespace World.Devices.Equip
{
    public class EquipSlot :MonoBehaviour,IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
    {
        public EquipmentMgrView view_owner;
        public TextMeshProUGUI slot_device_name;
        public Image slot_device_icon_bg;
        public GameObject content;

        public DeviceGoodsInfoView goods_info_view;


        public GameObject can_install;
        public GameObject cannot_install;

        public EquipDeviceJobView jobs;

        [SerializeField]
        private string slot_name;

        public EquipSlot(string str)
        {
            slot_name = str;            
        }

        public void update_slot()
        {
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            
            var select = view_owner.owner.select_device;

            var slot = dmgr.slots_device.Find(t => t._name == slot_name);
            var device = slot.slot_device;

            jobs?.Init(slot);

            if (select != null)
            {
                if (slot_name == "device_slot_wheel")
                {
                    //车轮走特殊逻辑

                    if(select.desc.device_type.value != EN_Device_Type.Wheel)
                    {
                        can_install.SetActive(false);
                        cannot_install.SetActive(true);
                    }
                    else
                    {
                        can_install.SetActive(true);
                        cannot_install.SetActive(false);
                    }
                       
                }
                else
                {

                    if(select.desc.device_type.value != EN_Device_Type.Wheel)
                    {
                        var select_width = select.desc.size;

                        var slot_width = slot.width;

                        if (slot_width < select_width)
                        {
                            can_install.SetActive(false);
                            cannot_install.SetActive(true);
                        }
                        else
                        {
                            can_install.SetActive(true);
                            cannot_install.SetActive(false);
                        }
                    }
                    else
                    {
                        can_install.SetActive(false);
                        cannot_install.SetActive(true);
                    }
                }
            }
            else
            {
                cannot_install.SetActive(false);
                can_install.SetActive(false);
            }

            if (device == null)
            {
                content.SetActive(false);
                slot_device_name.text = "";
            }
            else
            {
                content.SetActive(true);
                slot_device_name.text = Localization_Utility.get_localization(device.desc.name);

                var rank = device.desc.rank;
                device_ranks.TryGetValue(rank.ToString(), out var rank_db);

                Addressable_Utility.try_load_asset<Sprite>(rank_db.img_name_bar, out var name_bar);
                slot_device_icon_bg.sprite = name_bar;
            }
        }

        public void init()
        {
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            var slot = dmgr.slots_device.Find(t => t._name == slot_name);
            var device = slot.slot_device;

            jobs?.Init(slot);

            if (device == null)
            {
                content.SetActive(false);
                slot_device_name.text = "";
            }
            else
            {
                content.SetActive(true);
                slot_device_name.text = Localization_Utility.get_localization(device.desc.name);

                var rank = device.desc.rank;
                device_ranks.TryGetValue(rank.ToString(), out var rank_db);

                Addressable_Utility.try_load_asset<Sprite>(rank_db.img_name_bar, out var name_bar);
                slot_device_icon_bg.sprite = name_bar;
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!view_owner.can_install)
                return;

            var emgr = view_owner.owner;
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (slot_name == "device_slot_wheel")           //车轮不能卸载
                    return;
                var d = Device_Slot_Helper.RemoveDevice(slot_name);
                emgr.AddDevice(d);
            }
            else
            {
                Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
                var slot = dmgr.slots_device.Find(t => t._name == slot_name);
                if (view_owner.owner.select_device != null)
                {
                    if(view_owner.owner.select_device.desc.size > slot.width)
                    {

                    }
                    else
                    {
                        if (slot_name == "device_slot_wheel" && view_owner.owner.select_device.desc.device_type.value == EN_Device_Type.Wheel) //车轮槽位只能安装车轮
                        {
                            emgr.RemoveDevice(emgr.select_device);
                            var remove_device = Device_Slot_Helper.InstallDevice(emgr.select_device, slot_name);
                            emgr.select_device = null;
                            emgr.AddDevice(remove_device);
                        }
                        else if (slot_name != "device_slot_wheel" && view_owner.owner.select_device.desc.device_type.value != EN_Device_Type.Wheel && slot.width >= emgr.select_device.desc.size) //非车轮槽位不能安装车轮
                        {
                            emgr.RemoveDevice(emgr.select_device);
                            var remove_device = Device_Slot_Helper.InstallDevice(emgr.select_device, slot_name);
                            emgr.select_device = null;
                            emgr.AddDevice(remove_device);
                        }
                    }
                }
            }
            view_owner.owner.UpdateSlot();
        }

        public void InstallDevice()
        {
            if (!view_owner.can_install)
                return;
            var emgr = view_owner.owner;
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            var slot = dmgr.slots_device.Find(t => t._name == slot_name);
            if (slot_name == "device_slot_wheel" && view_owner.owner.select_device.desc.device_type.value == EN_Device_Type.Wheel) //车轮槽位只能安装车轮
            {
                emgr.RemoveDevice(emgr.select_device);
                var remove_device = Device_Slot_Helper.InstallDevice(emgr.select_device, slot_name);
                emgr.select_device = null;
                emgr.AddDevice(remove_device);
            }
            else if (slot_name != "device_slot_wheel" && view_owner.owner.select_device.desc.device_type.value != EN_Device_Type.Wheel && slot.width >= emgr.select_device.desc.size) //非车轮槽位不能安装车轮
            {
                emgr.RemoveDevice(emgr.select_device);
                var remove_device = Device_Slot_Helper.InstallDevice(emgr.select_device, slot_name);
                emgr.select_device = null;
                emgr.AddDevice(remove_device);
            }
            emgr.UpdateSlot();
        }

        public void OpenUpgradePanel()
        {
            var device = Device_Slot_Helper.GetDevice(slot_name);
            Addressable_Utility.try_load_asset<DeviceUpgradesView>("DeviceUpgradesView", out var upgrade_panel);
            var u_view = Instantiate(upgrade_panel, WorldSceneRoot.instance.uiRoot.transform, false);
            u_view.Init(device);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            var device = Device_Slot_Helper.GetDevice(slot_name);
            if (goods_info_view != null&& device!=null)
            {
                goods_info_view.gameObject.SetActive(true);
                goods_info_view.Init(Device_Slot_Helper.GetDevice(slot_name));
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            var device = Device_Slot_Helper.GetDevice(slot_name);
            if (goods_info_view != null && device != null)
            {
                goods_info_view.gameObject.SetActive(false);
                goods_info_view.Init(Device_Slot_Helper.GetDevice(slot_name));
            }
        }

    }
}
