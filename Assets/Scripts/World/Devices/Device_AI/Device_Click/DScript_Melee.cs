using AutoCodes;
using System;
using System.Collections.Generic;
using UnityEngine;
using World.Enemys;

namespace World.Devices.Device_AI
{
    public class DScript_Melee : Device,IAttack_Device,IAttack_New
    {
        #region CONST
        private const float ATK_ERROR_DEGREE = 5F;

        protected const string ANIM_IDLE = "idle";
        private const string ANIM_ATTACK_1 = "attack_1";
        private const string ANIM_ATTACK_2 = "attack_2";
        protected const string ANIM_BROKEN = "idle";
        private const string COLLIDER_1 = "collider_1";
        private const string KEY_POINT_1 = "collider_1";

        private const int SHARPEN_INTERVAL_MAX = 480;
        #endregion

        // ========================================================================================

        public float attack_factor = 1f;

        // ----------------------------------------------------------------------------------------

        protected melee_logic melee_logic_data;
        protected bool post_cast_finished = true;  //判定攻击后摇是否结束；

        // ----------------------------------------------------------------------------------------

        protected enum Device_FSM_Melee
        {
            idle,
            attacking,
            repair,
            broken,
        }
        protected Device_FSM_Melee fsm;

        private float distance_can_attack;

        protected float rotation_speed_fast;
        protected float rotation_speed_slow;

        private bool can_blaze;  //按住鼠标可以连续攻击

        private int attack_event_num; //有些武器只有1个攻击动画
        protected string _atk_anim_name;

        private int _sharpen_interval;
        private int _sharpen_last_ticks;

        public float sharpness_current;

        public float attack_job_process;

        public float charge_process;

        protected Action action_on_hit_enemy;

        // ----------------------------------------------------------------------------------------

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;
        #endregion


        // ========================================================================================

        #region InitData & Update

        public override void InitData(device_all rc)
        {

            device_type = DeviceType.melee;

            rotation_speed_fast = rc.rotate_speed.Item1;
            rotation_speed_slow = rc.rotate_speed.Item2;

            melee_logics.TryGetValue(rc.melee_logic.ToString(), out melee_logic_data);

            can_blaze = melee_logic_data.can_blaze;
            attack_event_num = melee_logic_data.atk_event_num;

            distance_can_attack = melee_logic_data.hit_box_offset + rc.basic_range.Item2;
            sharpness_current = 1;


            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATE, Vector2.up);

            base.InitData(rc);

            var anim_1_col_turn_on_time = melee_logic_data.hit_period.Item1;
            var anim_1_col_turn_off_time = melee_logic_data.hit_period.Item2;
            var anim_2_col_turn_on_time = melee_logic_data.hit_period_2 == null ? anim_1_col_turn_on_time : melee_logic_data.hit_period_2.Value.Item1;
            var anim_2_col_turn_off_time = melee_logic_data.hit_period_2 == null ? anim_1_col_turn_off_time : melee_logic_data.hit_period_2.Value.Item2;

            #region Anim_Event_Attack_1
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_1,
                percent = anim_1_col_turn_on_time,
                anim_event = (Device d) => open_collider(d)
            });

            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_1,
                percent = anim_1_col_turn_off_time,
                anim_event = (Device d) =>
                {
                    d.CloseCollider(COLLIDER_1);
                    post_cast_finished = true;
                }
            });
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_1,
                percent = 1,
                anim_event = (Device d) => {
                    charge_process = 0;
                    FSM_change_to(Device_FSM_Melee.idle);
                }
            });
            #endregion

            #region Anim_Event_Attack_2
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_2,
                percent = anim_2_col_turn_on_time,
                anim_event = (Device d) => open_collider(d)
            });
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_2,
                percent = anim_2_col_turn_off_time,
                anim_event = (Device d) =>
                {
                    d.CloseCollider(COLLIDER_1);
                    post_cast_finished = true;
                }
            });
            anim_events.Add(new AnimEvent()
            {
                anim_name = ANIM_ATTACK_2,
                percent = 1,
                anim_event = (Device d) => {
                    charge_process = 0;
                    FSM_change_to(Device_FSM_Melee.idle);
                }
            });
        }
            #endregion

            #region Attack_Subfunc
            protected virtual void open_collider(Device d)
            {
                DevicePlayAudio(melee_logic_data.SE_attack_begin);

                d.OpenCollider(COLLIDER_1, (ITarget t) =>
                {
                    var attack_datas = ExecDmg(t, melee_logic_data.damage);

                    foreach (var attack_data in attack_datas)
                    {
                        attack_data.calc_device_coef(this);

                        t.hurt(this, attack_data, out var dmg_data);
                        BattleContext.instance.ChangeDmg(this, dmg_data.dmg);
                        t.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);

                        BattleContext.instance.melee_damage_event?.Invoke(dmg_data,this,t);
                    }

                    BattleContext.instance.melee_attack_event?.Invoke(this, t);

                    float final_ft = melee_logic_data.knockback_ft;

                    if (t.hp <= 0)
                    {
                        kill_enemy_action?.Invoke(t);
                    }

                    t.impact(WorldEnum.impact_source_type.melee, (Vector2)key_points[KEY_POINT_1].position, BattleUtility.get_target_colllider_pos(t), final_ft);

                    sharpness_current = Mathf.Max(sharpness_current - melee_logic_data.sharpness_loss, melee_logic_data.sharpness_min);

                    action_on_hit_enemy?.Invoke();
                });
            }
            #endregion
        // ----------------------------------------------------------------------------------------

        public override void Start()
        {
            base.Start();
            CloseCollider(COLLIDER_1);
            FSM_change_to(Device_FSM_Melee.idle);
        }


        public override void Disable()
        {
            CloseCollider(COLLIDER_1);
            base.Disable();
        }

        #endregion

        // ========================================================================================

        #region Condition: Can Attack

        protected virtual bool can_auto_attack()
        {
            return can_attack_check_cd()
                && can_attack_check_post_cast()
                && can_attack_check_state()
                && can_attakc_check_error_angle()
                && can_attack_check_distance();
        }

        protected virtual bool can_manual_attack()
        {
            return can_attack_check_state();
        }

        // ----------------------------------------------------------------------------------------
        protected bool can_attack_check_cd()
        {
            return true;
        }

        protected bool can_attack_check_post_cast()
        {
            return post_cast_finished;
        }

        protected virtual bool can_attack_check_distance()
        {
            if (target_list.Count == 0)
                return false;
            return BattleUtility.get_v2_to_target_collider_pos(target_list[0], position).magnitude <= distance_can_attack;
        }

        protected bool can_attakc_check_error_angle()
        {
            if (target_list.Count == 0)
                return false;

            Vector2 target_v2 = BattleUtility.get_v2_to_target_collider_pos(target_list[0], position);
            var current_v2 = bones_direction[BONE_FOR_ROTATE];
            var delta_deg = Vector2.Angle(current_v2, target_v2);
            return delta_deg <= ATK_ERROR_DEGREE;
        }

        protected bool can_attack_check_state()
        {
            return fsm == Device_FSM_Melee.idle && state != DeviceState.stupor;
        }

        #endregion

        // ========================================================================================

        #region tick() & FSM_Change_to()

        public override void tick()
        {
            if (!is_validate && fsm != Device_FSM_Melee.broken)       //坏了
                FSM_change_to(Device_FSM_Melee.broken);

            if (state != DeviceState.stupor)
            {
                switch (fsm)
                {
                    case Device_FSM_Melee.idle:
                        idle_state_tick();
                        break;
                    case Device_FSM_Melee.attacking:
                        attack_state_tick();
                        break;
                    case Device_FSM_Melee.repair:
                        repair_state_tick();
                        break;
                    case Device_FSM_Melee.broken:
                        target_list.Clear();

                        if (is_validate)
                            FSM_change_to(Device_FSM_Melee.idle);
                        break;

                    default:
                        break;
                }
            }
            if (_sharpen_last_ticks > 0 && fsm != Device_FSM_Melee.broken)
            {
                _sharpen_last_ticks--;
                DeviceBehavior_Melee_Sharpen(1);
            }

            base.tick();
        }

        protected virtual void idle_state_tick()
        {
            DeviceBehavior_Select_Target();
            DeviceBehavior_Rotate_To_Target();
        }

        protected virtual void attack_state_tick()
        {
            DeviceBehavior_Select_Target();
            DeviceBehavior_Rotate_To_Target();
        }

        protected virtual void repair_state_tick()
        {

        }

        protected  virtual void FSM_change_to(Device_FSM_Melee target_fsm)
        {
            fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_FSM_Melee.idle:
                    ChangeAnim(ANIM_IDLE, true);
                    post_cast_finished = true;
                    rotate_speed = rotation_speed_fast;
                    break;
                case Device_FSM_Melee.attacking:
                    ChangeAnim(_atk_anim_name, false);
                    post_cast_finished = false;
                    rotate_speed = rotation_speed_slow;
                    break;
                case Device_FSM_Melee.broken:
                    ChangeAnim(ANIM_BROKEN, true);
                    post_cast_finished = true;
                    rotate_speed = rotation_speed_fast;
                    break;
                default:
                    break;
            }
        }

        #endregion

        // ========================================================================================

        #region Unsorted Fundamental Funcs


        // ========================================================================================

        protected override bool target_in_radius(ITarget t)
        {
            var tp = BattleUtility.get_target_colllider_pos(t);
            if (WorldContext.instance.is_need_reset)
            {
                tp -= new Vector2(WorldContext.instance.reset_dis, 0);
            }
            var t_distance = (tp - position).magnitude;
            // 相对于基类，移除了对 desc.basic_range.Item1 的判断
            return t_distance <= desc.basic_range.Item2;
        }

        protected override bool try_get_target()
        {
            var target = try_select_nearest_target();

            if (target != null)
            {
                if (target is Enemy enemy)
                    enemy.Select(true);
                target_list.Add(target);
            }

            // 这里即使返回null也是可以接受的，无需排除此结果
            return target != null;

            ITarget try_select_nearest_target()
            {
                // 以槽位位置为中心寻找半径内的目标，返回其中最近的目标。最近 = 到自身当前位置的距离最近
                var ts = BattleUtility.select_all_target_in_circle(position, desc.basic_range.Item2, faction, (ITarget ts) =>
                {
                    return target_can_be_selected(ts) && !target_list.Contains(ts) && !outrange_targets.ContainsKey(ts);
                });

                if (ts == null || ts.Count == 0)
                    return null;

                ts.Sort((ITarget t1, ITarget t2) =>
                {
                    if (BattleUtility.get_v2_to_target_collider_pos(t1, position).magnitude > BattleUtility.get_v2_to_target_collider_pos(t2, position).magnitude)
                        return 1;
                    else if (BattleUtility.get_v2_to_target_collider_pos(t1, position).magnitude == BattleUtility.get_v2_to_target_collider_pos(t2, position).magnitude)
                        return 0;
                    return -1;
                });
                return ts[0];
            }
        }

        // ========================================================================================
        // 伤害计算

        public override List<Attack_Data> ExecDmg(ITarget t, Dictionary<string, int> dmg)
        {
            battle_datas.TryGetValue(desc.id.ToString(), out var battle_data);
            List<Attack_Data> attack_datas = new();
            foreach (var d in dmg)
            {
                Attack_Data attack_data = new()
                {
                    atk = Mathf.CeilToInt((d.Value) * get_sharpness_modified_dmg_coef_by_stage(get_sharpness_stage()) * get_charge_modified_dmg()),
                    critical_chance = battle_data.critical_chance + BattleContext.instance.global_critical_chance,
                    critical_dmg_rate = battle_data.critical_rate + BattleContext.instance.global_critical_chance,
                    armor_piercing = battle_data.armor_piercing + BattleContext.instance.global_armor_piercing,
                    diy_atk_str = d.Key,
                };

                attack_datas.Add(attack_data);
            }

            return attack_datas;
        }

        // ========================================================================================
        // Sharpness相关

        protected float get_sharpness_modified_dmg_coef_by_stage(int stage_index)
        {
            var sharpness_dmg_coef_range = melee_logic_data.sharpness_dmg_range;
            if (sharpness_dmg_coef_range == null)
                return 1f;
            if (stage_index >= 0 && stage_index < sharpness_dmg_coef_range.Count)
                return sharpness_dmg_coef_range[stage_index];
            return 1f;
        }

        private float get_charge_modified_dmg()
        {
            return (1 - melee_logic_data.attack_charge_weight + melee_logic_data.attack_charge_weight * charge_process) ;
        }

        protected int get_sharpness_stage()
        {
            var sharpness_range = melee_logic_data.sharpness_range;

            if (sharpness_range.Count == 0)
                return -1;

            for (int i = 0; i < sharpness_range.Count; i++)
                if (sharpness_current * 100 <= sharpness_range[i])
                    return i;

            return sharpness_range.Count;
        }

        #endregion

        // ========================================================================================

        #region DeviceBehavior(Select Target, Rorate to Target, Try Attack, Sharpen,Charge)

        protected void DeviceBehavior_Select_Target()
        {
            if (target_list.Count == 0)
                try_get_target();
        }

        //-----------------------------------------------------------------------------------------

        protected void DeviceBehavior_Rotate_To_Target()
        {
            rotate_bone_to_target(BONE_FOR_ROTATE);
        }

        //-----------------------------------------------------------------------------------------

        protected virtual bool DeviceBehavior_Melee_Try_Attack(bool ui_manual_atk, string atk_anim, Action action_on_hit_enemy = null)
        {
            var can_atk = ui_manual_atk ? can_manual_attack() : can_auto_attack();
            if (can_atk)
            {
                _atk_anim_name = atk_anim;
                this.action_on_hit_enemy = action_on_hit_enemy;
                FSM_change_to(Device_FSM_Melee.attacking);
            }
            return can_atk;
        }


        //-----------------------------------------------------------------------------------------

        protected bool DeviceBehavior_Melee_Try_Charge()
        {
            if (fsm != Device_FSM_Melee.idle)
                return false;

            charge_process += melee_logic_data.attack_charge_speed;
            charge_process = Mathf.Min(1, charge_process);
            return true;
        }

        //-----------------------------------------------------------------------------------------

        protected void DeviceBehavior_Melee_Sharpen(float effect)
        {
            if(fsm == Device_FSM_Melee.broken)
                return;

            sharpness_current = Mathf.Min(1,sharpness_current + melee_logic_data.sharpness_recover);
        }

        #endregion

        // ========================================================================================

        #region NPC Auto Work: AutoAttack

        public virtual void TryToAutoAttack()
        {
            if(fsm!= Device_FSM_Melee.idle)
            {
                return;
            }  
            if(attack_job_process < 1)
            {
                attack_job_process += melee_logic_data.job_speed[0];
                return;
            }

            if(Auto_Attack_Job_Content())
            {
                attack_job_process = 0;
            }
        }

        /// <summary>
        /// 具体到各个设备上，自动攻击可能并不只包括“攻击”这一个动作，而是还含有其他动作，因此需要根据具体情况重载。
        /// </summary>
        /// <returns></returns>
        protected virtual bool Auto_Attack_Job_Content()
        {
            DeviceBehavior_Melee_Try_Attack(false, "attack_1", null);

            return true;
        }
        #endregion

        // ========================================================================================

        #region UI Info & UI Controlled Actions

        public virtual bool CanAttack()
        {
            return can_manual_attack();
        }

        public virtual void Try_Attacking()
        {
            DeviceBehavior_Melee_Try_Attack(true, "attack_1", null);
        }

        public interface ISharpNew
        {
        }
        #endregion

    }
}
