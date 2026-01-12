using AutoCodes;
using Commons;
using Foundations;
using Foundations.Tickers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Devices.DeviceUiViews;
using World.Helpers;

namespace World.Devices
{
    public class Ditto : Device
    {
        enum Device_Ditto_FSM
        {
            idle,
            attack,
            broken,
        }

        Device_Ditto_FSM m_fsm;


        public int energy;
        public int max_energy;

        public const int C_max_energy = 100;
        public const int C_add_energy = 10;

        public const int C_ditto_tick = 10 * Config.PHYSICS_TICKS_PER_SECOND;

        protected List<uint> ban_id_list = new() { 1330110u, 1330111u };

        //==================================================================================================

        public override void InitData(device_all rc)
        {
            base.InitData(rc);

            max_energy = C_max_energy;
            energy = 0;
        }


        public override void tick()
        {
            base.tick();
        }


        public void try_to_cast()
        {
            energy += C_add_energy;
            if (energy <= max_energy) return;

            energy = Mathf.Min(energy, max_energy);

            do_ditto();
        }


        public void do_ditto()
        {
            Debug.Log("【百变怪】开始变身!");

            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr mgr);
            var slot_name = mgr.slots_device.Where(t => t.slot_device == this).First()._name;

            //当前设备位置
            var self_ui_view = (DeviceUiView)views.Where(t => t is DeviceUiView).First();
            var local_pos = self_ui_view.transform.localPosition;

            //装卸设备
            Device_Slot_Helper.RemoveDevice(slot_name);
            var other_device_id = calc_ditto_device_id();
            var other_device = Device_Slot_Helper.InstallDevice($"{other_device_id},0", slot_name);

            //目标设备位置
            var other_device_ui_view = (DeviceUiView)other_device.views.Where(t => t is DeviceUiView).First();
            other_device_ui_view.transform.localPosition = local_pos;

            Request_Helper.delay_do("ditto", C_ditto_tick, (_) => { end_ditto(); });

            #region 子函数 end_ditto
            void end_ditto()
            {
                Debug.Log("【百变怪】结束变身!");

                //规则：退出目标设备的控制
                InputController.instance.EndDeviceControl();

                var other_pos = other_device_ui_view.transform.localPosition;

                Device_Slot_Helper.RemoveDevice(slot_name);
                var new_ditto = Device_Slot_Helper.InstallDevice($"{desc.id},0", slot_name);

                var new_ditto_ui_view = (DeviceUiView)new_ditto.views.Where(t => t is DeviceUiView).First();
                new_ditto_ui_view.transform.localPosition = other_pos;
            }
            #endregion
        }


        public virtual string calc_ditto_device_id()
        {
            return default;
        }
    }
}

