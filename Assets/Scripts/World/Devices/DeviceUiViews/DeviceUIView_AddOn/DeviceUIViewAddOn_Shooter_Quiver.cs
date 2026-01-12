using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.Device_AI;
using static World.Devices.Device_AI.Shooter_Quiver;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Quiver : DeviceUIViewAddOn_Shooter
    {
        public DevicePanelAttachment_Ratchet_Crank widget_ratchet_for_shoot;
        public DevicePanelAttachment_Quiver_Ammo_Slots widget_ammo_reload;
        public DevicePanelAttachment_Ammo_Slot widget_ammo_slot;
        public List<DevicePanelAttachment_Process_Bar> widget_process_bars = new();

        public TextMeshProUGUI ammoText;
        public Image lever;

        protected new Shooter_Quiver owner;

        public override void attach(Device owner)
        {
            base.attach(owner);

            widget_ratchet_for_shoot.Pre_Init_Ratchet_Data(this.owner.Shoot_Ratchet_Rotation_Handle);
            widget_ratchet_for_shoot.Init();

            widget_ammo_reload.Init();
            widget_ammo_slot.Init();
            widget_ammo_slot.Update_Ammo_Slot_View(this.owner.current_ammo);

            for (int i = 0; i < this.owner.ammo_slots.Count; i++)
            {
                widget_process_bars[i].Update_View((float)this.owner.ammo_slots[i].filling_tick / Quiver_Ammo_Slot.filling_ticks_need, this.owner.ammo_slots[i].ammo_state == 2);
            }
        }

        protected override void attach_owner(Device owner)
        {
            base.attach_owner(owner);
            this.owner = owner as Shooter_Quiver;
            this.owner.Shoot_Ratchet_Rotation_Handle.on_rotate += widget_ratchet_for_shoot.Synchronize_Dir;
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            widget_ratchet_for_shoot.Rotate_To_Output_Power_Per_Tick(true, out var rotation_power);

            if (widget_ammo_reload.Is_Holding(false))
            {
                owner.Try_Reloading();        //里面需要一个上弹cd检测
            }

            for(int i = 0;i< this.owner.ammo_slots.Count; i++)
            {
                widget_ammo_slot.Set_Index_Ammo_Slot_View(i, this.owner.ammo_slots[i].ammo_state ==1);
            }

            lever.transform.rotation = Quaternion.Euler(0, 0, owner.Shoot_Ratchet_Rotation_Handle.Dir_Angle);

            for (int i = 0; i < this.owner.ammo_slots.Count; i++)
            {
                widget_process_bars[i].Update_View((float)(this.owner.ammo_slots[i].filling_tick) / (float)(Quiver_Ammo_Slot.filling_ticks_need), this.owner.ammo_slots[i].ammo_state == 2);
            }

            ammoText.text = $"{owner.current_ammo} {owner.fire_logic_data.capacity}";
        }

        protected override void attach_highlightable()
        {
            autoWorkHighlight_reload.Add(widget_ammo_reload);
            autoWorkHighlight_shoot.Add(widget_ratchet_for_shoot);
        }


        public override void detach()
        {
            base.detach();
            owner.Shoot_Ratchet_Rotation_Handle.on_rotate -= widget_ratchet_for_shoot.Synchronize_Dir;
        }
    }
}
