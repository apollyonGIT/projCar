using AutoCodes;
using Commons;
using UnityEngine;

namespace World.Devices.Device_AI
{
    public class LaserGun :NewBasicShooter,IEnergy
    {
        private float width = 0.5f; //激光宽度
        private float length = 10f; //激光长度
        
        private float energy_Current;
        public float Current_Energy
        {
            get 
            {
                return energy_Current;
            }

            set
            {
                energy_Current = Mathf.Clamp(value, 0, Max_Energy);
            }
        }
        
        public float Default_Energy { get ;set; }
        public float Max_Energy { get;set; }
        public float Energy_Recover { get; set ; } 
        public float Energy_Recover_Factor { get; set; }
        public int Energy_FreezeTick_Current { get; set; }
        public int Energy_FreezeTick_Max { get; set; }

        private bool can_shoot_check_energy => Current_Energy > 0;

       

        public override void InitData(device_all rc)
        {

            fire_logics.TryGetValue(rc.fire_logic.ToString(), out var record);

            Attack_Interval = record.cd;

            Reload_Speed = record.reload_speed * (1 + Reload_Speed_Factor);

            se_reloaded = record.SE_reloaded;

            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATE, Vector2.right);

            Max_Energy = 100 + BattleContext.instance.energy_capacity_add;
            Current_Energy = Max_Energy;
            Energy_Recover_Factor = 1 + BattleContext.instance.energy_recover_add;
            Energy_Recover = 10f * (Energy_Recover_Factor);
            Energy_FreezeTick_Max = 60;
            Energy_FreezeTick_Current = 60;

            InitBaseData(rc);
        }

        public override void UpdateData()
        {
            base.UpdateData();
            Max_Energy = 100 + BattleContext.instance.energy_capacity_add;
            Energy_Recover_Factor = 1 + BattleContext.instance.energy_recover_add;
            Energy_Recover = 10f * (Energy_Recover_Factor);
        }

        public override void tick()     //shoot状态下 每隔attack interval进行一次 伤害检测
        {
            if (!is_validate && fsm != NewDevice_FSM_Shooter.broken)
                FSM_change_to(NewDevice_FSM_Shooter.broken);

            if (fsm != NewDevice_FSM_Shooter.broken && is_validate && player_oper && state!= DeviceState.stupor && state!= DeviceState.bind)
                Aiming();

            switch (fsm)
            {
                case NewDevice_FSM_Shooter.idle:
                    if(Energy_FreezeTick_Current > 0)
                    {
                        Energy_FreezeTick_Current--;
                    }
                    else
                    {
                        Current_Energy += Energy_Recover * Config.PHYSICS_TICK_DELTA_TIME;
                    }
                    break;
                case NewDevice_FSM_Shooter.shoot:
                    if(Current_Interval > 0)
                    {
                        Current_Interval--;
                    }
                    else
                    {
                        if (can_shoot_check_energy)
                        {
                            fire_logics.TryGetValue(desc.fire_logic.ToString(), out var logic);
                            Vector2 sp = key_points[logic.bone_name].position;
                            var dir = bones_direction[BONE_FOR_ROTATE];

                            var bv = new Vector2(1 / dir.x, -1 / dir.y).normalized;
                            if(dir.x == 0)
                            {
                                bv = new Vector2(1, 0);
                            }
                            if(dir.y == 0)
                            {
                                bv = new Vector2(0, 1);
                            }

                            var p1 = sp + bv * width;
                            var p2 = sp - bv * width + dir * length;


                            var targets = BattleUtility.select_all_target_in_rect(p1,p2,faction);
                            foreach(var t in targets)
                            {
                                var attack_datas = ExecDmg(t, logic.damage);

                                foreach(var attack_data in attack_datas)
                                {
                                    attack_data.calc_device_coef(this);
                                    t.hurt(this, attack_data, out var dmg_data);
                                    BattleContext.instance.ChangeDmg(this, dmg_data.dmg);
                                    t.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg); 
                                }

                                if (t.hp <= 0)
                                {
                                    kill_enemy_action?.Invoke(t);
                                }
                            }
                        }
                        Current_Interval = Attack_Interval;

                        Current_Energy -= 20 * Config.PHYSICS_TICK_DELTA_TIME * Attack_Interval;

                        if(Current_Energy == 0)
                        {
                            FSM_change_to(NewDevice_FSM_Shooter.idle);
                            Energy_FreezeTick_Current = Energy_FreezeTick_Max;
                        }
                    }
                    break;
            }

            BaseTick();
        }

        protected override bool Fire()
        {
            if (can_shoot_check_energy)
            {
                if(fsm == NewDevice_FSM_Shooter.idle)
                {
                    FSM_change_to(NewDevice_FSM_Shooter.shoot);
                    return true;
                }
            }
            return false;
        }

        public void TryToAccelerate()
        {
            Current_Energy += Energy_Recover * Config.PHYSICS_TICK_DELTA_TIME;
        }
    }
}
