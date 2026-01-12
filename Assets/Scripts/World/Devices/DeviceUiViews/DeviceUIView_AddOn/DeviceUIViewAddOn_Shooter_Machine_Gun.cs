using TMPro;
using UnityEngine;
using World.Cubicles;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Machine_Gun : DeviceUIViewAddOn_Shooter
    {
        #region Const
        private const bool IGNORE_POST_CAST = false; // 是否忽略后摇
        private const float CLIP_ROTATION_ANGLE = 53.1301F;
        #endregion

        public TextMeshProUGUI ammoText;
        public RectTransform clip_rotation_center;
        public RectTransform clip_rotation_self;

        public DevicePanelAttachment_Draggable ammo_clip_on_track;  // 0 = attached_to_shoot, 1 = detached_to_reload
        public DevicePanelAttachment_Trigger_Hold trigger_for_shooting;
        public DevicePanelAttachment_Draggable_Spring widget_gun_bolt;
        public DevicePanelAttachment_Shell_Throwing_Window shell_throwing_window;

        // -------------------------------------------------------------------------------

        new protected Shooter_AutoGun_Rifle owner;

        // -------------------------------------------------------------------------------

        private readonly DeviceUI_Component_Blinker blinker_for_ammo_loader = new();

        // ===============================================================================

        public override void attach(Device device_owner)
        {
            base.attach(device_owner);
            ammo_clip_on_track.Init();
            ammo_clip_on_track.Forced_Set_Relative_Drag_Distance_01(owner.Clip.Track_Value);
            widget_gun_bolt.Init(null);
            shell_throwing_window.Init();
        }

        protected override void attach_owner(Device device_owner)
        {
            base.attach_owner(device_owner);
            owner = device_owner as Shooter_AutoGun_Rifle;
            owner.ui_panel_on_spring_triggered = action_on_gun_bolt_triggered;
            owner.ui_panel_on_spring_moved = action_on_gun_bolt_moved;
            owner.Clip.on_silo_moved_on_track = action_on_clip_moved;
            owner.Clip.on_silo_fully_installed_trigger = action_on_clip_triggered;
            owner.Clip.on_silo_fully_open_trigger = action_on_clip_triggered;
        }

        protected override void attach_highlightable()
        {
            autoWorkHighlight_reload.Add(ammo_clip_on_track);
            autoWorkHighlight_shoot.Add(trigger_for_shooting);
            autoWorkHighlight_shoot.Add(widget_gun_bolt);
        }

        // -------------------------------------------------------------------------------

        public override void detach()
        {
            base.detach();
            owner.ui_panel_on_spring_triggered -= action_on_gun_bolt_triggered;
            owner.ui_panel_on_spring_moved -= action_on_gun_bolt_moved;
            owner.Clip.on_silo_moved_on_track -= action_on_clip_moved;
            owner.Clip.on_silo_fully_installed_trigger -= action_on_clip_triggered;
            owner.Clip.on_silo_fully_open_trigger -= action_on_clip_triggered;
        }

        // -------------------------------------------------------------------------------

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            // Trigger: Per Tick Check
            if (trigger_for_shooting.Is_Holding(false))
                shoot(IGNORE_POST_CAST, owner.Action_On_Shoot);


            // Update View by Owner 1
            if (shell_throwing_window.Has_Alive_Throwing_Shell())
                shell_throwing_window.Per_Tick_For_Throwing_Shells();
            ammoText.text = $"{owner.current_ammo} {owner.fire_logic_data.capacity}";
            shell_throwing_window.Update_Window_Ammo_View(owner.Get_Barrel());


            // Sync Data to Owner
            owner.Clip.Track_Value = ammo_clip_on_track.Get_Relative_Drag_Distance_01();
            owner.Triggerable_Spring.Spring_Value_01 = widget_gun_bolt.Get_Relative_Drag_Distance_01();


            // Update View by Owner 2
            var rotation_z = owner.Clip.Track_Value * CLIP_ROTATION_ANGLE;
            clip_rotation_center.localRotation = Quaternion.Euler(0, 0, rotation_z);
            clip_rotation_self.localRotation = Quaternion.Euler(0, 0, -rotation_z);


            // Set DevicePanelAttachment_Highlightable Status
            trigger_for_shooting.Highlighted = owner.Gun_Hammer_Locked && Barrel_Bullet_Loaded;
            widget_gun_bolt.Highlighted = owner.Clip.Silo_Fully_Installed && owner.current_ammo > 0 && !Barrel_Bullet_Loaded;
            var need_reload = owner.Clip.Silo_Fully_Installed && owner.current_ammo <= 0;
            var need_move = !owner.Clip.Silo_Fully_Installed && !owner.Clip.Silo_Fully_Open;
            var need_install = owner.Clip.Silo_Fully_Open && owner.current_ammo >= owner.fire_logic_data.capacity;
            ammo_clip_on_track.Highlighted = need_reload || need_move || need_install;
        }

        // ===============================================================================

        /// <summary>
        /// 枪栓锁定时的回调函数。
        /// </summary>
        private void action_on_gun_bolt_triggered()
        {
            shell_throwing_window.New_Bullet_Loaded(owner.Get_Barrel());
        }

        private void action_on_gun_bolt_moved()
        {
            widget_gun_bolt.Forced_Set_Relative_Drag_Distance_01(owner.Triggerable_Spring.Spring_Value_01);
        }

        private void action_on_clip_moved()
        {
            ammo_clip_on_track.Forced_Set_Relative_Drag_Distance_01(owner.Clip.Track_Value);
        }

        private void action_on_clip_triggered()
        {
            ammo_clip_on_track.Forced_Drop_Drag();
            ammo_clip_on_track.Forced_Set_Relative_Drag_Distance_01(owner.Clip.Track_Value);
        }

    }
}
