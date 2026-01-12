using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Revolver : DeviceUIViewAddOn_Shooter
    {
        #region Const
        private const string SE_GUN_NO_AMMO = "se_ag_gun_no_ammo";

        private const bool IGNORE_POST_CAST = true; // 是否忽略后摇
        private const float CYLINDER_ROTATION_ANGLE = 90F; // 左轮枪弹仓旋转角

        private const int CYLINDER_ROTATION_TICKS = 10; // 弹仓旋转的刻度数
        private const float CYLINDER_ROTATION_TICKS_RECIPROCAL = 1F / CYLINDER_ROTATION_TICKS; // 弹仓旋转的刻度数的倒数，用于将其归一化
        #endregion

        public RectTransform cylinder_rotation_center; // 弹仓旋转中心

        public Image bullet_silo;
        public Image bullet_reloading_indicator; // 弹仓装填指示器
        public GameObject bullet_reloading_indicator_auto;

        public DevicePanelAttachment_Ammo_Slot AmmoSlot;
        public DevicePanelAttachment_Trigger_Press trigger_for_shooting;
       // public DevicePanelAttachment_Draggable widget_for_drag_cylinder;
        public DevicePanelAttachment_Trigger_Press widget_trigger_for_reloading;
        public List<DevicePanelAttachment_Process_Bar> widget_process_bars;

        // -------------------------------------------------------------------------------

        new protected Shooter_Revolver owner; // 重写owner为Shooter_Revolver类型

        // -------------------------------------------------------------------------------

        private bool show_bullet_reloading_indicator = false;
        private int cylinder_rotation_ticks = CYLINDER_ROTATION_TICKS;
        private float cylinder_rotation_angle_current;
        private float cylinder_rotation_angle_last;

        private readonly DeviceUI_Component_Blinker blinker_for_ammo_loader = new();

        // ===============================================================================

        public override void attach(Device owner)
        {
            base.attach(owner);
            AmmoSlot.Init();
            trigger_for_shooting.Init(new() { shoot_without_parms });
     //       widget_for_drag_cylinder.Init();
     //       widget_for_drag_cylinder.Init_Draggable_Object_Position(this.owner.Cylinder.Track_Value);
            widget_trigger_for_reloading.Init(new() { action_on_reload_by_trigger_slot });

            set_cylinder_rotate_data(360F + this.owner.Current_Ammo_With_Fillings * 60F, false);
            AmmoSlot.Update_Ammo_Slot_View(this.owner.current_ammo);
        }

        protected override void attach_owner(Device device_owner)
        {
            base.attach_owner(device_owner);
            owner = device_owner as Shooter_Revolver;
            owner.Cylinder.on_silo_fully_installed_trigger += action_on_cylinder_fully_installed;
            owner.Cylinder.on_silo_fully_open_trigger += action_on_cylinder_fully_open;
            owner.Cylinder.on_silo_moved_on_track += action_on_cylinder_moved;
        }

        protected override void attach_highlightable()
        {
            autoWorkHighlight_reload.Add(widget_trigger_for_reloading);
            autoWorkHighlight_shoot.Add(trigger_for_shooting);
        }

        // -------------------------------------------------------------------------------

        public override void detach()
        {
            base.detach();
            owner.Cylinder.on_silo_fully_installed_trigger -= action_on_cylinder_fully_installed;
            owner.Cylinder.on_silo_fully_open_trigger -= action_on_cylinder_fully_open;
            owner.Cylinder.on_silo_moved_on_track -= action_on_cylinder_moved;
        }

        // -------------------------------------------------------------------------------

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            // Sync Data to Owner
  //          owner.Cylinder.Track_Value = widget_for_drag_cylinder.Get_Relative_Drag_Distance_01();


            // Update View by Panel Self 1
            if (cylinder_rotation_ticks > 0)
            {
                cylinder_rotation_ticks--;
                var silo_rotation_angle = Mathf.Lerp(cylinder_rotation_angle_current, cylinder_rotation_angle_last, cylinder_rotation_ticks * CYLINDER_ROTATION_TICKS_RECIPROCAL);
                bullet_silo.rectTransform.localRotation = Quaternion.Euler(0, 0, silo_rotation_angle);
            }


            // Update View by Owner
            trigger_for_shooting.Interactable = owner.Cylinder.Silo_Fully_Installed;

            if (owner.Current_Ammo_With_Fillings == 0)
                blinker_for_ammo_loader.update_blink(ref bullet_silo, Color.red);
            else
                bullet_silo.color = Color.white;

            cylinder_rotation_center.localRotation = Quaternion.Euler(0, 0, owner.Cylinder.Track_Value * CYLINDER_ROTATION_ANGLE);

            if (show_bullet_reloading_indicator != owner.Cylinder.Silo_Fully_Open)
                show_bullet_reloading_indicator = !show_bullet_reloading_indicator;

            var ammo = owner.Current_Ammo_With_Fillings < owner.fire_logic_data.capacity;
            bullet_reloading_indicator.gameObject.SetActive(show_bullet_reloading_indicator && ammo);
            if (ammo)
                bullet_reloading_indicator.rectTransform.anchoredPosition = AmmoSlot.Get_Ammo_Slot_Anchored_Position(owner.Current_Ammo_With_Fillings);
            AmmoSlot.Update_Ammo_Slot_View(owner.current_ammo);
            for (int i = 0; i < owner.fire_logic_data.capacity; i++)
            {
                var bullet_index = i + owner.current_ammo;
                widget_process_bars[i].Update_View(owner.Get_Filling_Process_01(i - owner.current_ammo), i >= owner.current_ammo && i < owner.Current_Ammo_With_Fillings);
            }


            // Update View by Panel Self 2
            bullet_reloading_indicator_auto.SetActive(widget_trigger_for_reloading.Auto);


            // Set DevicePanelAttachment_Highlightable Status
            trigger_for_shooting.Highlighted = owner.Cylinder.Silo_Fully_Installed && owner.current_ammo > 0;
        }

        // ===============================================================================

        /// <summary>
        /// 用于控制弹仓旋转。
        /// 会在初始化时、开火时、合上弹仓时调用
        /// </summary>
        /// <param name="target_angle"></param>
        private void set_cylinder_rotate_data(float target_angle, bool will_rotate)
        {
            cylinder_rotation_ticks = will_rotate ? CYLINDER_ROTATION_TICKS : 1;
            cylinder_rotation_angle_last = cylinder_rotation_angle_current;
            cylinder_rotation_angle_current = target_angle;
        }

        // ===============================================================================
        // 射击

        private void shoot_without_parms()
        {
            if (!owner.Cylinder.Silo_Fully_Installed)
            {
                Audio.AudioSystem.instance.PlayOneShot(SE_GUN_NO_AMMO);
                return; // 如果弹仓未关闭，则不射击
            }

            set_cylinder_rotate_data(cylinder_rotation_angle_current - 60f, true);
            //左轮枪特性，点击多快就能射多快，需要忽略后摇。
            shoot(IGNORE_POST_CAST);
        }

        // -------------------------------------------------------------------------------
        // 装填与重置弹仓

        private void action_on_reload_by_trigger_slot()
        {
            owner.Reloading_In_Progress = true;  // 只要玩家介入了弹巢操作，一定会将装填状态设置为true。需要NPC检查并重置为false。
            reload();
        }

        // -------------------------------------------------------------------------------

        private void action_on_cylinder_moved()
        {
     //       widget_for_drag_cylinder.Forced_Set_Relative_Drag_Distance_01(owner.Cylinder.Track_Value);
        }

        private void action_on_cylinder_fully_open()
        {
     //       widget_for_drag_cylinder.Forced_Drop_Drag();
     //       widget_for_drag_cylinder.Forced_Set_Relative_Drag_Distance_01(owner.Cylinder.Track_Value);
        }

        private void action_on_cylinder_fully_installed()
        {
       //     widget_for_drag_cylinder.Forced_Drop_Drag();
       //     widget_for_drag_cylinder.Forced_Set_Relative_Drag_Distance_01(owner.Cylinder.Track_Value);
            set_cylinder_rotate_data(360F + owner.Current_Ammo_With_Fillings * 60F, true);
        }

    }
}
