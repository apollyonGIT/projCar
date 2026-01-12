using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Gatlin : DeviceUIViewAddOn_Shooter
    {
        #region Const
        private const int RELOADER_BLINK_TICKS_MAX = 90; //装填时的最大闪烁时长
        #endregion

        public TextMeshProUGUI ammoText;
        public Image barrel;
        public DevicePanelAttachment_Reload_Ammo_Box reload_ammo_box;
        public DevicePanelAttachment_Reload_Ammo_Box ammo_box_view_for_auto_indicator;
        public DevicePanelAttachment_Ammo_Belt ammo_belt;
        public DevicePanelAttachment_Ratchet_Crank widget_ratchet;

        // -------------------------------------------------------------------------------

        new protected Shooter_Gatlin owner;

        // -------------------------------------------------------------------------------

        private readonly DeviceUI_Component_Blinker reloader_blinker = new();
        private int reloader_blinker_ticks; // 用于闪烁的计时器

        // ===============================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);
            reload_ammo_box.Init(new() { reload });
            ammo_belt.Init();
            widget_ratchet.Pre_Init_Ratchet_Data(this.owner.ratchetShoot);
            widget_ratchet.Init();
        }

        protected override void attach_owner(Device device_owner)
        {
            base.attach_owner(device_owner);
            owner = device_owner as Shooter_Gatlin;
            owner.ammoBox.on_reload_banned += blink_on_reload_banned;
            owner.ratchetShoot.on_rotate += widget_ratchet.Synchronize_Dir;
        }

        protected override void attach_highlightable()
        {
            //autoWorkHighlight_reload.Add(reload_ammo_box);
            autoWorkHighlight_shoot.Add(widget_ratchet);
        }

        // -------------------------------------------------------------------------------

        public override void detach()
        {
            base.detach();
            owner.ammoBox.on_reload_banned -= blink_on_reload_banned;
            owner.ratchetShoot.on_rotate -= widget_ratchet.Synchronize_Dir;
        }

        // -------------------------------------------------------------------------------

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            // Trigger: Per Tick Check
            if (widget_ratchet.Rotate_To_Output_Power_Per_Tick(true, out var rotation_power))
                owner.UI_Controlled_Speed_Up_Spining(rotation_power);


            // Update View by Owner
            barrel.transform.rotation = Quaternion.Euler(0, 0, owner.Barrel_Rotation_Angle);
            ammoText.text = $"{owner.current_ammo} {owner.fire_logic_data.capacity}";
            ammo_belt.Update_Ammo_Belt_View(owner.current_ammo, (int)owner.fire_logic_data.capacity);
            var ownerAmmoBoxStage = owner.ammoBox.GetAmmoBoxStage(out var reloadCountRemaining);
            reload_ammo_box.Update_View(ownerAmmoBoxStage, reloadCountRemaining);
            if (ammo_box_view_for_auto_indicator.gameObject.activeSelf)
                ammo_box_view_for_auto_indicator.Update_View(ownerAmmoBoxStage, reloadCountRemaining);


            // Update View by Panel Self 
            if (reloader_blinker_ticks > 0)
            {
                reloader_blinker_ticks--;
                reloader_blinker.update_blink(ref barrel, Color.red);
            }
            else if (reloader_blinker_ticks == 0)
            {
                reloader_blinker_ticks--;
                barrel.color = Color.white;
            }


            // Set DevicePanelAttachment_Highlightable Status
            widget_ratchet.Highlighted = owner.current_ammo > 0;
        }

        // ===============================================================================

        private void blink_on_reload_banned()
        {
            reloader_blinker_ticks = RELOADER_BLINK_TICKS_MAX;
        }

    }
}
