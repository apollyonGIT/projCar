using AutoCodes;
using System;
using UnityEngine;


namespace World.Devices.Device_AI
{
    public class Shooter_Shotgun : DScript_Shooter
    {
        #region CONST
        private const int LOAD_INTERVAL = 20;
        #endregion

        // -------------------------------------------------------------------------------

        protected override int shoot_salvo => _loaded_ammo;

        // -------------------------------------------------------------------------------

        private int loaded_ammo_max;
        private int load_ammo_interval;

        private bool _gun_hammer_locked; // 击锤是否已就绪
        private int _loaded_ammo;

        // ===============================================================================

        public override void InitData(device_all rc)
        {
            base.InitData(rc);
            fire_logics.TryGetValue(rc.fire_logic.ToString(), out var record);
            loaded_ammo_max = (int)record.salvo;
            _loaded_ammo = loaded_ammo_max;
        }

        // ===============================================================================

        protected override bool DeviceBehavior_Shooter_Try_Shoot(bool ui_manual_shoot, bool ignore_post_cast = false, Action shoot_finished = null)
        {
            if (!Gun_Hammer_Locked)
                return false;

            return base.DeviceBehavior_Shooter_Try_Shoot(ui_manual_shoot, ignore_post_cast, action_on_shoot_finished);
        }

        // -------------------------------------------------------------------------------

        private bool DeviceBehavior_Shotgun_Load_Ammo()
        {
            if (--load_ammo_interval <= 0)
            {
                load_ammo_interval = LOAD_INTERVAL;
                Loaded_Ammo++;
                return true;
            }
            return false;
        }

        // ===============================================================================

        protected override bool Auto_Attack_Job_Content()
        {
            if (!Gun_Hammer_Locked)
            {
                Gun_Hammer_Locked = true;
                return true;
            }

            return base.Auto_Attack_Job_Content();
        }

        // -------------------------------------------------------------------------------

        protected override bool Auto_Reload_Job_Content()
        {
            if (Loaded_Ammo < loaded_ammo_max)
            {
                Loaded_Ammo++;
                return true;
            }

            return false;
        }

        // ===============================================================================

        private void action_on_shoot_finished()
        {
            Loaded_Ammo = 0;
            Gun_Hammer_Locked = false;
        }

        // ===============================================================================

        // Public for UI Panel
        public bool Gun_Hammer_Locked
        {
            get { return _gun_hammer_locked; }
            set { _gun_hammer_locked = value; }
        }

        public int Loaded_Ammo
        {
            get { return _loaded_ammo; }
            set
            {
                _loaded_ammo = Mathf.Clamp(value, 0, loaded_ammo_max);
                if (value > 0 && current_ammo < (int)fire_logic_data.capacity)
                    current_ammo = (int)fire_logic_data.capacity;
            }
        }

        // -------------------------------------------------------------------------------

        public void UI_Controlled_Shotgun_Load_Ammo()
        {
            DeviceBehavior_Shotgun_Load_Ammo();
        }

    }
}

