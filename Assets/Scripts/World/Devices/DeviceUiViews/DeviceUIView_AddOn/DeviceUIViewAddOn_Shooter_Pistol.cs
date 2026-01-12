using TMPro;
using UnityEngine;
using World.Cubicles;
using World.Devices.Device_AI;


namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Pistol : DeviceUIViewAddOn_Shooter
    {
        #region Const
        private const bool IGNORE_POST_CAST = true; // 是否忽略后摇
        private const float HAMMER_ROTATION_ANGLE = -45F;
        private const int HAMMER_ROTATION_TICKS = 3; // 击锤旋转的计时器
        #endregion

        public TextMeshProUGUI ammoText;
        public RectTransform hammer_rotate_center;

        public GameObject clip_on_track_A;
        public GameObject clip_on_track_B;

        public DevicePanelAttachment_Draggable ammo_clip_track_A;
        public DevicePanelAttachment_Draggable ammo_clip_track_B;
        public DevicePanelAttachment_Trigger_Press trigger_for_shooting;
        public DevicePanelAttachment_Draggable_Spring widget_gun_bolt;
        public DevicePanelAttachment_Shell_Throwing_Window shell_throwing_window;

        // -------------------------------------------------------------------------------

        new protected Shooter_AutoGun_Pistol owner;

        // -------------------------------------------------------------------------------

        private int hammer_rotation_ticks = 0; // 用于击锤旋转的计时器
        private DevicePanelAttachment_Draggable active_clip => owner.Dual_Track_Clip.On_Track_A ? ammo_clip_track_A : ammo_clip_track_B;

        private readonly DeviceUI_Component_Blinker blinker_for_ammo_loader = new();

        // ===============================================================================

        public override void attach(Device device_owner)
        {
            base.attach(device_owner);
            ammo_clip_track_A.Init();
            ammo_clip_track_B.Init();
            active_clip.Forced_Set_Relative_Drag_Distance_01(owner.Dual_Track_Clip.Track_Value);
            trigger_for_shooting.Init(new() { action_shoot_on_trigger_pressed });
            widget_gun_bolt.Init(null);
            shell_throwing_window.Init();

            clip_on_track_A.SetActive(owner.Dual_Track_Clip.On_Track_A);
            clip_on_track_B.SetActive(!owner.Dual_Track_Clip.On_Track_A);
        }

        protected override void attach_owner(Device device_owner)
        {
            base.attach_owner(device_owner);
            owner = device_owner as Shooter_AutoGun_Pistol;
            owner.ui_panel_on_spring_triggered = action_on_gun_bolt_triggered;
            owner.ui_panel_on_spring_moved = action_on_gun_bolt_moved;
            owner.Dual_Track_Clip.on_silo_moved_on_track = action_on_clip_moved;
            owner.Dual_Track_Clip.on_silo_fully_installed_trigger = action_on_clip_triggered;
            owner.Dual_Track_Clip.on_silo_track_switched_trigger = action_on_clip_triggered;
        }

        protected override void attach_highlightable()
        {
            autoWorkHighlight_reload.Add(ammo_clip_track_A);
            autoWorkHighlight_reload.Add(ammo_clip_track_B);
            autoWorkHighlight_shoot.Add(trigger_for_shooting);
            autoWorkHighlight_shoot.Add(widget_gun_bolt);
        }

        // -------------------------------------------------------------------------------

        public override void detach()
        {
            base.detach();
            owner.ui_panel_on_spring_triggered -= action_on_gun_bolt_triggered;
            owner.ui_panel_on_spring_moved -= action_on_gun_bolt_moved;
            owner.Dual_Track_Clip.on_silo_moved_on_track -= action_on_clip_moved;
            owner.Dual_Track_Clip.on_silo_fully_installed_trigger -= action_on_clip_triggered;
            owner.Dual_Track_Clip.on_silo_track_switched_trigger -= action_on_clip_triggered;
        }

        // -------------------------------------------------------------------------------

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            // Update View by Owner 1
            if (shell_throwing_window.Has_Alive_Throwing_Shell())
                shell_throwing_window.Per_Tick_For_Throwing_Shells();
            ammoText.text = $"{owner.current_ammo} {owner.fire_logic_data.capacity}";
            shell_throwing_window.Update_Window_Ammo_View(owner.Get_Barrel());


            // Sync Data to Owner
            owner.Dual_Track_Clip.Track_Value = active_clip.Get_Relative_Drag_Distance_01();
            owner.Triggerable_Spring.Spring_Value_01 = widget_gun_bolt.Get_Relative_Drag_Distance_01();


            // Update View by Owner 2
            float hammer_rotation_coef;
            if (owner.Gun_Hammer_Locked)
                hammer_rotation_coef = 1;
            else if (hammer_rotation_ticks <= 0)
                hammer_rotation_coef = owner.Triggerable_Spring.Spring_Value_01;
            else
            {
                hammer_rotation_ticks--;
                hammer_rotation_coef = Mathf.Max(owner.Triggerable_Spring.Spring_Value_01, (float)hammer_rotation_ticks / HAMMER_ROTATION_TICKS);
            }
            hammer_rotate_center.localRotation = Quaternion.Euler(0, 0, hammer_rotation_coef * HAMMER_ROTATION_ANGLE);
            trigger_for_shooting.Interactable = owner.Gun_Hammer_Locked || owner.Dual_Track_Clip.Silo_Fully_Installed;


            // Set DevicePanelAttachment_Highlightable Status
            trigger_for_shooting.Highlighted = owner.Gun_Hammer_Locked && Barrel_Bullet_Loaded;
            widget_gun_bolt.Highlighted = owner.Dual_Track_Clip.Silo_Fully_Installed && owner.current_ammo > 0 && !Barrel_Bullet_Loaded;
            var need_reload = owner.Dual_Track_Clip.Silo_Fully_Installed && owner.current_ammo <= 0;
            var need_move = !owner.Dual_Track_Clip.Silo_Fully_Installed && !owner.Dual_Track_Clip.Silo_Fully_Open;
            var need_install = owner.Dual_Track_Clip.Silo_Fully_Open && owner.current_ammo >= owner.fire_logic_data.capacity;
            active_clip.Highlighted = need_reload || need_move || need_install;
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
            active_clip.Forced_Set_Relative_Drag_Distance_01(owner.Dual_Track_Clip.Track_Value);

            clip_on_track_A.SetActive(owner.Dual_Track_Clip.On_Track_A);
            clip_on_track_B.SetActive(!owner.Dual_Track_Clip.On_Track_A);
        }

        private void action_on_clip_triggered()
        {
            active_clip.Forced_Drop_Drag();
            active_clip.Forced_Set_Relative_Drag_Distance_01(owner.Dual_Track_Clip.Track_Value);

            clip_on_track_A.SetActive(owner.Dual_Track_Clip.On_Track_A);
            clip_on_track_B.SetActive(!owner.Dual_Track_Clip.On_Track_A);
        }

        private void action_shoot_on_trigger_pressed()
        {
            shoot(IGNORE_POST_CAST, owner.Action_On_Shoot);
            if (owner.Gun_Hammer_Locked)
                hammer_rotation_ticks = HAMMER_ROTATION_TICKS;
            owner.Gun_Hammer_Locked = false; // 按下扳机后，击锤解锁
        }

    }
}
