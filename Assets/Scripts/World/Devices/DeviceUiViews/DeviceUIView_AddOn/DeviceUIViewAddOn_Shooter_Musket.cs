using UnityEngine;
using World.Cubicles;
using World.Devices.Device_AI;


namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Musket : DeviceUIViewAddOn_Shooter
    {
        #region Const
        private const bool IGNORE_POST_CAST = true; // 是否忽略后摇；手操枪就是忽略后摇，自动枪械就不是忽略后摇。
        private const float CLIP_ROTATION_ANGLE = -53.1301F;
        #endregion

        public GameObject autoWorkIndicator_shoot_2;

        public RectTransform clip_rotation_center;
        public RectTransform clip_rotation_self;

        public DevicePanelAttachment_Draggable ammo_clip_on_track;  // 0 = attached_to_shoot, 1 = detached_to_reload
        public DevicePanelAttachment_Ammo_Slot ammo_slot;
        public DevicePanelAttachment_Ammo_Slot ammo_slot_ui_dot;
        public DevicePanelAttachment_Trigger_Press trigger_for_shooting;
        public DevicePanelAttachment_Draggable_Spring widget_gun_bolt;
        public DevicePanelAttachment_Shell_Throwing_Window shell_throwing_window;

        // -------------------------------------------------------------------------------

        new protected Shooter_AutoGun_Rifle owner;

        // -------------------------------------------------------------------------------

        private readonly DeviceUI_Component_Blinker blinker_for_ammo_loader = new();

        // ===============================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);
            ammo_slot.Init();
            ammo_slot_ui_dot.Init();
            ammo_clip_on_track.Init();
            ammo_clip_on_track.Forced_Set_Relative_Drag_Distance_01(this.owner.Clip.Track_Value);
            trigger_for_shooting.Init(new() { action_shoot_on_trigger_pressed });
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

            // Update View by Owner 1
            if (shell_throwing_window.Has_Alive_Throwing_Shell())
                shell_throwing_window.Per_Tick_For_Throwing_Shells();
            shell_throwing_window.Update_Window_Ammo_View(owner.Get_Barrel());
            ammo_slot.Update_Ammo_Slot_View(owner.current_ammo);
            ammo_slot_ui_dot.Update_Ammo_Slot_View(owner.Clip.Silo_Fully_Installed ? owner.current_ammo : 0);


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
        private void action_shoot_on_trigger_pressed()
        {
            shoot(IGNORE_POST_CAST);
        }

    }
}
