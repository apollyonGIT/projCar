using AutoCodes;
using System.Collections.Generic;
using UnityEngine;
using World.Projectiles;

namespace World.Devices.Device_AI
{
    public class Shield_Generator :Device,IShield, IAttack_Device
    {
        #region CONST
        private const string ANIM_IDLE = "idle";
        private const string ANIM_BLOCK_ATK = "attack_1";
        private const string ANIM_CHARGING_FINISHED = "ready";
        private const string ANIM_BROKEN = "idle";
        private const string COLLIDER_FOR_ATK = "collider_1";
        private const float PROJECTILE_REFLECT_COEF = 1.1F;
        #endregion

        #region IShield
        public float ShieldEnergy_Current { get; set; }
        public float ShieldEnergy_Max { get; set; }
        public float ShieldEnergy_Recover_1 { get; set; }
        public float ShieldEnergy_Recover_2 { get; set; }
        public int ShieldEnergy_FreezeTick_Current { get; set; }
        public int ShieldEnergy_FreezeTick_Max { get; set; }
        public float ShieldEnergy_Deduct_By_Blocking { get; set; }
        public int Shield_Blocking_Interval_Current { get; set; }
        public int Shield_Blocking_Interval_Max { get; set; }
        public float Def_Range { get; set; }
        public Vector2 Def_Dir { get; set; }
        public Vector2 Shield_Dir => bones_direction[BONE_FOR_ROTATE];
        #endregion

        #region shield_block
        private float atk_ft;
        private bool shield_can_block_projectile => fsm != Device_FSM_Shield_Generator.broken;
        private bool shield_can_block_enemy => fsm == Device_FSM_Shield_Generator.defending && Shield_Blocking_Interval_Current <= 0;

        private string SE_block;
        #endregion

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;
        #endregion

        private enum Device_FSM_Shield_Generator
        {
            idle,
            defending,
            broken,
        }

        private Device_FSM_Shield_Generator fsm;

        public override void InitData(device_all rc)
        {
            shield_logics.TryGetValue(rc.shield_logic.ToString(), out var record);

            Def_Range = record.def_range;

            atk_ft = record.knockback_ft;
            Shield_Blocking_Interval_Max = record.cd;

            ShieldEnergy_Current = record.energy_max;
            ShieldEnergy_Max = record.energy_max;
            ShieldEnergy_Recover_1 = record.energy_recover.Item1;
            ShieldEnergy_FreezeTick_Max = record.energy_freezetick;
            ShieldEnergy_Recover_2 = record.energy_recover.Item2;
            ShieldEnergy_Deduct_By_Blocking = record.energy_reduce_by_block;

            SE_block = record.SE_block;

            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATE, Vector2.right);

            base.InitData(rc);

            #region AnimEvent
            var block_atk_anim_back_to_idle = new AnimEvent()
            {
                anim_name = ANIM_BLOCK_ATK,
                percent = 1f,
                anim_event = (Device d) => ChangeAnim(ANIM_IDLE, true)
            };

            var charge_ready_anim_back_to_idle = new AnimEvent()
            {
                anim_name = ANIM_CHARGING_FINISHED,
                percent = 1f,
                anim_event = (Device d) => ChangeAnim(ANIM_IDLE, true)
            };
            #endregion

            anim_events.Add(block_atk_anim_back_to_idle);
            anim_events.Add(charge_ready_anim_back_to_idle);
        }

        public override void Start()
        {
            base.Start();
            FSM_change_to(Device_FSM_Shield_Generator.idle);
        }

        private void FSM_change_to(Device_FSM_Shield_Generator target_fsm)
        {
            fsm = target_fsm;
            switch (fsm)
            {
                case Device_FSM_Shield_Generator.idle:
                    ChangeAnim(ANIM_IDLE, true);
                    break;
                case Device_FSM_Shield_Generator.defending:

                    OpenCollider(COLLIDER_FOR_ATK, (ITarget t) =>
                    {
                        if (!shield_can_block_enemy)
                            return;

                        trigger_defend(true);
                        float final_atk_ft = atk_ft;

                        shield_logics.TryGetValue(desc.shield_logic.ToString(), out var logic);
                        var attack_datas = ExecDmg(t, logic.damage);
                        foreach (var attack_data in attack_datas)
                        {
                            attack_data.calc_device_coef(this); //如果需要计算设备的参数修正，就调用一下这个函数

                            t.hurt(this, attack_data, out var dmg_data);
                            BattleContext.instance.ChangeDmg(this, dmg_data.dmg);

                            t.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);
                        }

                        if (t.hp <= 0)
                            kill_enemy_action?.Invoke(t);

                        t.impact(WorldEnum.impact_source_type.melee, Vector2.zero, Shield_Dir, final_atk_ft);
                    });
                    ChangeAnim(ANIM_IDLE, true);
                    break;
                case Device_FSM_Shield_Generator.broken:
                    ChangeAnim(ANIM_BROKEN, true);
                    break;  
            }
        }

        public override void tick()
        {
            if (!is_validate && fsm != Device_FSM_Shield_Generator.broken)
                FSM_change_to(Device_FSM_Shield_Generator.broken);

            switch (fsm)
            {
                case Device_FSM_Shield_Generator.idle:
                    pertick_check_and_try_recover(ShieldEnergy_Recover_1);
                    pertick_check_and_try_reload();
                    break;
                case Device_FSM_Shield_Generator.defending:
                    pertick_check_and_try_recover(ShieldEnergy_Recover_2);
                    pertick_check_and_try_reload();

                    if (ShieldEnergy_Current <= 0)
                    {
                        FSM_change_to(Device_FSM_Shield_Generator.idle);      //没能量就休息
                        ShieldEnergy_FreezeTick_Current = ShieldEnergy_FreezeTick_Max;
                    }
                    break;
                case Device_FSM_Shield_Generator.broken:
                    if (is_validate)
                        FSM_change_to(Device_FSM_Shield_Generator.idle);
                    break;
                default:
                    break;
            }

            base.tick();
        }

        private void pertick_check_and_try_recover(float recover_value)
        {
            if (ShieldEnergy_FreezeTick_Current > 0)
                ShieldEnergy_FreezeTick_Current--;
            else if (ShieldEnergy_Current < ShieldEnergy_Max)
            {
                ShieldEnergy_Current += recover_value;
                if (ShieldEnergy_Current > ShieldEnergy_Max)
                    ShieldEnergy_Current = ShieldEnergy_Max;
            }
        }

        private void pertick_check_and_try_reload()
        {
            if (Shield_Blocking_Interval_Current > 0)
                if (--Shield_Blocking_Interval_Current <= 0)
                    ChangeAnim(ANIM_CHARGING_FINISHED, false);
        }

        private void trigger_defend(bool reset_interval)
        {
            ChangeAnim(ANIM_BLOCK_ATK, false);
            if (fsm == Device_FSM_Shield_Generator.defending)
                ShieldEnergy_Current -= ShieldEnergy_Deduct_By_Blocking;
            if (reset_interval)
                Shield_Blocking_Interval_Current = Shield_Blocking_Interval_Max;
            Audio.AudioSystem.instance.PlayOneShot(SE_block);
        }

        bool IShield.Try_Rebound_Projectile(Projectile proj, Vector2 proj_vel)
        {
            if (!shield_can_block_projectile)
                return false;

            var v_new = Vector2.Reflect(proj_vel - velocity, -Shield_Dir) * PROJECTILE_REFLECT_COEF + velocity;
            proj.ResetProjectile(v_new, Shield_Dir, faction, MovementStatus.normal);
            trigger_defend(false);

            return true;
        }

        public void Raise_Shield()
        {
            FSM_change_to(Device_FSM_Shield_Generator.defending);
        }
    }
}
