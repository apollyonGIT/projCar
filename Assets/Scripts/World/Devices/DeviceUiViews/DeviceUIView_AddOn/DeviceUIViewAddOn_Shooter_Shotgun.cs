using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Shotgun : DeviceUIViewAddOn_Shooter
    {
        #region Const
        private const string SE_GUN_NO_AMMO = "se_ag_gun_no_ammo";

        private const bool IGNORE_POST_CAST = true; // 是否忽略后摇
        private const float CYLINDER_ROTATION_ANGLE = 90F; // 左轮枪弹仓旋转角

        private const int CYLINDER_ROTATION_TICKS = 10; // 弹仓旋转的刻度数
        private const float CYLINDER_ROTATION_TICKS_RECIPROCAL = 1F / CYLINDER_ROTATION_TICKS; // 弹仓旋转的刻度数的倒数，用于将其归一化
        #endregion

        public TextMeshProUGUI ammoSalvoText;
        public GameObject ignition_cap;

        public DevicePanelAttachment_Trigger_Hold widget_tp_for_reloading;
        public DevicePanelAttachment_Trigger_Press widget_tp_for_hammer;
        public DevicePanelAttachment_Trigger_Press widget_tp_for_hammer_locked;
        public DevicePanelAttachment_Trigger_Press widget_tp_for_shooting;

        // -------------------------------------------------------------------------------

        new protected Shooter_Shotgun owner;

        // -------------------------------------------------------------------------------

        private readonly DeviceUI_Component_Blinker blinker_for_ammo_loader = new();

        // ===============================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);

            widget_tp_for_shooting.Init(new() { shoot_without_parms });
            widget_tp_for_hammer.Init(new() { action_on_set_hammer });
        }

        protected override void attach_owner(Device device_owner)
        {
            base.attach_owner(device_owner);
            owner = device_owner as Shooter_Shotgun;
        }

        protected override void attach_highlightable()
        {
            autoWorkHighlight_reload.Add(widget_tp_for_reloading);
            autoWorkHighlight_shoot.Add(widget_tp_for_shooting);
            autoWorkHighlight_shoot.Add(widget_tp_for_hammer);
            autoWorkHighlight_shoot.Add(widget_tp_for_hammer_locked);
        }

        // -------------------------------------------------------------------------------

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            // Trigger: Per Tick Check
            if (widget_tp_for_reloading.Is_Holding(false))
                owner.UI_Controlled_Shotgun_Load_Ammo();


            // Update View by Owner
            ammoSalvoText.text = $"{owner.Loaded_Ammo}/20";
            ignition_cap.SetActive(owner.Gun_Hammer_Locked);
            widget_tp_for_hammer.GetComponent<Image>().enabled = !owner.Gun_Hammer_Locked;
            widget_tp_for_hammer_locked.GetComponent<Image>().enabled = owner.Gun_Hammer_Locked;


            // Set DevicePanelAttachment_Highlightable Status
            widget_tp_for_reloading.Highlighted = owner.current_ammo < owner.fire_logic_data.capacity;
            widget_tp_for_shooting.Highlighted = owner.Gun_Hammer_Locked;
            widget_tp_for_hammer.Highlighted = !owner.Gun_Hammer_Locked;
            widget_tp_for_hammer_locked.Highlighted = !owner.Gun_Hammer_Locked;
        }

        // ===============================================================================
        // 射击
        private void shoot_without_parms()
        {
            shoot(IGNORE_POST_CAST);
        }

        // -------------------------------------------------------------------------------

        private void action_on_set_hammer()
        {
            owner.Gun_Hammer_Locked = true;
        }

    }
}
