using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.Device_AI;
using World.Devices.DeviceUiViews.Attachments;

namespace World.Devices.DeviceUiViews
{
    public class DeviceUIViewAddOn_Shooter_Revolver_XY: DeviceUIViewAddOn_Shooter
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

        //public DevicePanelAttachment_Ammo_Slot_XY AmmoSlot;

        public DevicePanelAttachment_Trigger_Press trigger_for_shooting;

        public List<DevicePanelAttachment_Process_Bar> widget_process_bars;

        public List<DevicePanelAttachment_Trigger_Press> widget_triggers_for_reloading;

        public List<DevicePanelAttachment_Animator> bullets_animator;

        public List<GameObject> slots = new();

        // -------------------------------------------------------------------------------

        new protected DScript_Shooter_Revolver_XY owner;
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
            //AmmoSlot.Init();
            trigger_for_shooting.Init(new() { shoot_without_parms });

            for (int i = 0; i < widget_triggers_for_reloading.Count; i++)
            {
                var index = i;
                widget_triggers_for_reloading[i].Init(new() { () => { trigger_event(index); } });
            }

            foreach(var bullet in bullets_animator)
            {
                bullet.Init();
            }

            set_cylinder_rotate_data(-this.owner.current_index * 60F, false);
            //AmmoSlot.Update_Ammo_Slot_View(this.owner.current_ammo);
        }

        private void trigger_event(int index)
        {
            var ammo = owner.GetSlot(index);
            if (ammo.slot_state == AmmoBullet_XY.SlotState.empty)
            {
                owner.ReloadSlot(index);
            }
            if(ammo.slot_state == AmmoBullet_XY.SlotState.already_shot)
            {
                owner.ClearSlot(index);

                //播放退弹动画
            }
        }



        protected override void attach_owner(Device device_owner)
        {
            base.attach_owner(device_owner);
            owner = device_owner as DScript_Shooter_Revolver_XY;
        }

        protected override void attach_highlightable()
        {

            foreach (var reloading in widget_triggers_for_reloading)
            {
                autoWorkHighlight_reload.Add(reloading);
            }
            autoWorkHighlight_shoot.Add(trigger_for_shooting);
        }

        // -------------------------------------------------------------------------------


        private int state2index(AmmoBullet_XY.SlotState slot_state)
        {
            switch (slot_state)
            {
                case AmmoBullet_XY.SlotState.empty:
                    return -1;
                case AmmoBullet_XY.SlotState.filling:
                    return -1;
                case AmmoBullet_XY.SlotState.ready_to_shoot:
                    return 0;
                case AmmoBullet_XY.SlotState.already_shot:
                    return 1;
            }
            return -1;
        }
        public override void notify_on_tick()
        {
            base.notify_on_tick();
            
            var ammo_list = owner.ammo_bullets;

            foreach(var bullet_anim in bullets_animator)
            {
                bullet_anim.transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            foreach(var slot in slots)
            {
                slot.transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            // Sync Data to Owner
            //          owner.Cylinder.Track_Value = widget_for_drag_cylinder.Get_Relative_Drag_Distance_01();


            // Update View by Panel Self 1
            if (cylinder_rotation_ticks > 0)
            {
                cylinder_rotation_ticks--;
                var silo_rotation_angle = Mathf.Lerp(cylinder_rotation_angle_current, cylinder_rotation_angle_last, cylinder_rotation_ticks * CYLINDER_ROTATION_TICKS_RECIPROCAL);
                if (cylinder_rotation_angle_current ==0)
                {
                    silo_rotation_angle = Mathf.Lerp(cylinder_rotation_angle_current - 360f, cylinder_rotation_angle_last, cylinder_rotation_ticks * CYLINDER_ROTATION_TICKS_RECIPROCAL);
                }
                bullet_silo.rectTransform.localRotation = Quaternion.Euler(0, 0, silo_rotation_angle);
            }


            // Update View by Owner
            trigger_for_shooting.Interactable = owner.CanShoot();

            if (owner.GetAmmoCountIncludeFilling() == 0)
                blinker_for_ammo_loader.update_blink(ref bullet_silo, Color.red);
            else
                bullet_silo.color = Color.white;

            for (int i = 0; i < owner.fire_logic_data.capacity; i++)
            {
                var sprite_index = state2index(ammo_list[i].slot_state);
                

//AmmoSlot.Set_Index_Ammo_Slot_View(i, sprite_index);
                widget_process_bars[i].Update_View(ammo_list[i].reload_process, ammo_list[i].slot_state == AmmoBullet_XY.SlotState.filling);
            }


            // Update View by Panel Self 2

            if (owner.CanReload() || owner.Reloading())
            {
                for (int i = 0; i < owner.ammo_bullets.Count; i++)
                {
                    if (ammo_list[i].slot_state == AmmoBullet_XY.SlotState.empty)
                    {
                        widget_triggers_for_reloading[i].Highlighted = true;
                    }
                    else
                    {
                        widget_triggers_for_reloading[i].Highlighted = false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < owner.ammo_bullets.Count; i++)
                {
                    widget_triggers_for_reloading[i].Highlighted = false;
                }
            }
            // Set DevicePanelAttachment_Highlightable Status
            trigger_for_shooting.Highlighted = owner.CanShoot();

            if(cylinder_rotation_angle_current != -owner.current_index * 60f)
            {
                set_cylinder_rotate_data(-owner.current_index * 60f, true);
            }

            foreach(var anim in bullets_animator)
            {
                anim.tick();
            }
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
            if (!owner.CanShoot())
            {
                Audio.AudioSystem.instance.PlayOneShot(SE_GUN_NO_AMMO);
                return; // 如果弹仓未关闭，则不射击
            }

            //set_cylinder_rotate_data(cylinder_rotation_angle_current - 60f, true);
            //左轮枪特性，点击多快就能射多快，需要忽略后摇。
            shoot(IGNORE_POST_CAST);
        }

        // -------------------------------------------------------------------------------
        // 装填与重置弹仓



        public override void trigger_event(params object[] args)
        {
            string trigger_name = args[0] as string;
            int index = (int)args[1];

            bullets_animator[index].SetTrigger(trigger_name);
        }
    }
}
