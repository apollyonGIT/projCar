using System.Collections.Generic;
using UnityEngine;
using AutoCodes;
using System;


namespace World.Devices.Device_AI
{
    public class Shooter_Revolver : DScript_Shooter
    {
        #region Const
        private const bool AUTO_READY_TO_LOAD = true; // 是否自动打开弹巢
        private const int BULLET_FILLING_TICKS_MAX = Commons.Config.PHYSICS_TICKS_PER_SECOND;
        #endregion

         public Device_Attachment_Tracked_Silo Cylinder = new();

        // -------------------------------------------------------------------------------

        private readonly List<int> filling_bullets = new();

        private bool _reloading_in_progress = false; // 工作：装填状态

        // ===============================================================================

        public override void InitData(device_all rc)
        {
            base.InitData(rc);
            Cylinder.on_silo_fully_installed_trigger = filling_bullets.Clear;
        }

        // ===============================================================================

        protected override void tick_after_fsm_while_unbroken()
        {
            base.tick_after_fsm_while_unbroken();
            if (Reloading_In_Progress && Cylinder.Silo_Fully_Installed && current_ammo == fire_logic_data.capacity)
                Reloading_In_Progress = false;

            if (filling_bullets != null)
                for (int i = 0; i < filling_bullets.Count; i++)
                    filling_bullets[i]++;

            check_filling_bullet_finished();
        }

        // ===============================================================================

        private void check_filling_bullet_finished()
        {
            if (filling_bullets == null || filling_bullets.Count == 0)
                return;

            if (filling_bullets[0] >= BULLET_FILLING_TICKS_MAX)
            {
                filling_bullets.RemoveAt(0);
                current_ammo++;
                check_filling_bullet_finished();
            }
        }

        // ===============================================================================

        protected override bool can_auto_shoot()
        {
            return base.can_auto_shoot() && can_shoot_check_cylinder();
        }

        // -------------------------------------------------------------------------------

        private bool can_shoot_check_cylinder()
        {
            return Cylinder.Silo_Fully_Installed;
        }

        // ===============================================================================

        protected override bool can_manual_reload()
        {
            return base.can_manual_reload() && can_reload_check_cylinder();
        }

        // -------------------------------------------------------------------------------

        protected override bool can_reload_check_ammo()
        {
            return current_ammo + filling_bullets.Count <fire_logic_data.capacity;
        }

        private bool can_reload_check_cylinder()
        {
            return Cylinder.Silo_Fully_Open;
        }

        // ===============================================================================

        protected override bool DeviceBehavior_Shooter_Try_Shoot(bool ui_manual_shoot, bool ignore_post_cast = false, Action shoot_finished = null)
        {
            return base.DeviceBehavior_Shooter_Try_Shoot(ui_manual_shoot, ignore_post_cast, action_on_finish_shoot);
        }

        // -------------------------------------------------------------------------------

        protected override bool DeviceBehavior_Shooter_Try_Reload(bool manual_instantly_reloading)
        {
            var can_reload = manual_instantly_reloading ? can_manual_reload() : can_auto_reload();
            if (can_reload)
                fill_bullet_begin((int)(fire_logic_data.reload_num > 0 ? fire_logic_data.reload_num : Mathf.Max(fire_logic_data.capacity - current_ammo, 0)));
            return can_reload;

            void fill_bullet_begin(int count)
            {
                if (count <= 0)
                    return;

                filling_bullets.Add(0);
                count--;
                fill_bullet_begin(count);
            }
        }

        // ===============================================================================

        protected override bool Auto_Reload_Job_Content()
        {
            if (current_ammo == 0 && !Cylinder.Silo_Fully_Open)
            {
                Cylinder.Track_Value = 1;
                _reloading_in_progress = true;
                return true; // 工作：确保打开弹巢
            }

            if (current_ammo < fire_logic_data.capacity && Cylinder.Silo_Fully_Open)
            {
                DeviceBehavior_Shooter_Try_Reload(true);
                _reloading_in_progress = true;
                return true; // 工作：装填一发子弹
            }

            if (current_ammo == fire_logic_data.capacity)
            {
                if (!Cylinder.Silo_Fully_Installed)
                    Cylinder.Track_Value = 0f;// 工作：尝试关闭弹巢
                else
                    _reloading_in_progress = false;// 工作：退出装填状态
                return true;
            }

            return false; // 没有装填
        }

        public override void TryToAutoAttack()
        {
            if (_reloading_in_progress)
                return; // 如果正在重装，则不射击

            base.TryToAutoAttack();
        }

        // ===============================================================================

        private void action_on_finish_shoot()
        {
            if (AUTO_READY_TO_LOAD && current_ammo == 0)
                Cylinder.Track_Value = 1f;
        }

        // ===============================================================================
        // For UI Info

        /// <summary>
        /// 用于判断当前是否正在装填中。
        /// 如果正在装填中，则不能射击。
        /// 当NPC开启装填流程后，或玩家手动操作弹巢之后，设置为true。
        /// </summary>
        public bool Reloading_In_Progress
        {
            get { return _reloading_in_progress; }
            set { _reloading_in_progress = value; }
        }

        public int Current_Ammo_With_Fillings
        {
            get { return current_ammo + filling_bullets.Count; }
        }

        public float Get_Filling_Process_01(int index)
        {
            if (index < 0 || index >= filling_bullets.Count)
                return 0;

            return (float)filling_bullets[index] / BULLET_FILLING_TICKS_MAX;
        }

    }
}