using AutoCodes;
using UnityEngine;
using World.Audio;
using World.Enemys;
using System.Collections.Generic;
using System;
using System.Linq;

namespace World.Devices.Device_AI
{
    public class BasicMelee_Click : Device, IAttack, ISharp, IAttack_Device
    {

        #region Data

        #region CONST
        private const string BONE_FOR_ROTATION = "roll_control";
        private const float ATK_ERROR_DEGREE = 5F;

        private const string ANIM_IDLE = "idle";
        private const string ANIM_ATTACK_1 = "attack_1";
        private const string ANIM_ATTACK_2 = "attack_2";
        private const string ANIM_BROKEN = "idle";
        private const string COLLIDER_1 = "collider_1";
        private const string KEY_POINT_1 = "collider_1";

        private const int SHARPEN_INTERVAL_MAX = 480;
        #endregion

        // ========================================================================================

        public float attack_factor = 1f;

        public Action update_panel_view_on_attack;  //更新面板视图的回调。可以用于更新锋利度等信息。在面板控制脚本的attach阶段手动添加，并在detach阶段卸载。

        // ----------------------------------------------------------------------------------------

        protected melee_logic melee_logic;
        protected bool post_cast_finished = true;  //判定攻击后摇是否结束；

        // ----------------------------------------------------------------------------------------

        private enum Device_FSM_Melee
        {
            idle,
            attacking,
            broken,
        }
        private Device_FSM_Melee fsm;

        private Dictionary<string,int> melee_atk_dmg;
        private float basic_knockback;

        private float hit_box_offset;
        private float distance_can_attack;

        private float rotation_speed_fast;
        private float rotation_speed_slow;

        private string SE_grinding;

        private bool can_blaze;  //按住鼠标可以连续攻击

        private int attack_event_num; //有些武器只有1个攻击动画
        private string _atk_anim_name;

        private List<float> sharpness_range = new();
        private List<float> sharpness_dmg_coef_range = new();

        private List<string> atk_horizontal_anim = new();
        private List<string> atk_vertical_anim = new();
        private bool _ui_atk_can_horizontal;
        private bool _ui_atk_can_vertical;

        private int _sharpen_interval;
        private int _sharpen_last_ticks;

        private Action action_on_hit_enemy;

        // ----------------------------------------------------------------------------------------

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;

        #endregion

        #region IAttack
        public float Damage_Increase { get; set; }
        public float Knockback_Increase { get; set; }
        public int Attack_Interval { get; set; }
        public int Current_Interval { get; set; }
        public float Attack_Interval_Factor { get; set; }
        #endregion

        #region ISharp
        public float Sharpness_Current { get; set; }
        public float Sharpness_Min { get; set; }
        public float Sharpness_Loss { get; set; }
        public float Sharpness_Recover { get; set; }
        #endregion

        #endregion

        // ========================================================================================

        #region InitData & Update

        public override void InitData(device_all rc)
        {

            device_type = DeviceType.melee;

            rotation_speed_fast = rc.rotate_speed.Item1;
            rotation_speed_slow = rc.rotate_speed.Item2;

            melee_logics.TryGetValue(rc.melee_logic.ToString(), out melee_logic);

            melee_atk_dmg = melee_logic.damage;
            can_blaze = melee_logic.can_blaze;
            attack_event_num = melee_logic.atk_event_num;
            atk_horizontal_anim = melee_logic.ui_atk_horizontal_anim;
            atk_vertical_anim = melee_logic.ui_atk_vertical_anim;
            _ui_atk_can_horizontal = atk_horizontal_anim == null ? false : atk_horizontal_anim.Count > 0;
            _ui_atk_can_vertical = atk_vertical_anim == null ? false : atk_vertical_anim.Count > 0;
            basic_knockback = melee_logic.knockback_ft;
            hit_box_offset = melee_logic.hit_box_offset;
            distance_can_attack = hit_box_offset + rc.basic_range.Item2;
            SE_grinding = melee_logic.SE_grinding;

            Attack_Interval_Factor = 0f;
            Current_Interval = melee_logic.cd;
            Attack_Interval = (int)(melee_logic.cd * (1 + Attack_Interval_Factor));

            Sharpness_Current = (int)(100 * (1 + BattleContext.instance.melee_sharpness_add / 1000f));
            Sharpness_Min = melee_logic.sharpness_min;
            Sharpness_Loss = melee_logic.sharpness_loss;
            Sharpness_Recover = melee_logic.sharpness_recover;

            sharpness_range = melee_logic.sharpness_range;
            sharpness_dmg_coef_range = melee_logic.sharpness_dmg_range;

            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATION, Vector2.up);

            base.InitData(rc);

            var anim_1_col_turn_on_time = melee_logic.hit_period.Item1;
            var anim_1_col_turn_off_time = melee_logic.hit_period.Item2;
            var anim_2_col_turn_on_time = melee_logic.hit_period_2 == null ? anim_1_col_turn_on_time : melee_logic.hit_period_2.Value.Item1;
            var anim_2_col_turn_off_time = melee_logic.hit_period_2 == null ? anim_1_col_turn_off_time : melee_logic.hit_period_2.Value.Item2;

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
                anim_event = (Device d) => FSM_change_to(Device_FSM_Melee.idle)
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
                anim_event = (Device d) => FSM_change_to(Device_FSM_Melee.idle)
            });

            #endregion

            #region Attack_Subfunc
            void open_collider(Device d)
            {
                DevicePlayAudio(melee_logic.SE_attack_begin);

                d.OpenCollider(COLLIDER_1, (ITarget t) =>
                {
                    var attack_datas = ExecDmg(t, melee_atk_dmg);

                    foreach(var attack_data in attack_datas)
                    {
                        attack_data.calc_device_coef(this);

                        t.hurt(this, attack_data, out var dmg_data);
                        BattleContext.instance.ChangeDmg(this, dmg_data.dmg);
                        t.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);
                    }


                    //int final_dmg = (int)((melee_atk_dmg + Damage_Increase) * get_sharpness_modified_dmg_coef_by_stage(get_sharpness_stage()));
                    float final_ft = (basic_knockback + Knockback_Increase);

                    if (t.hp <= 0)
                    {
                        kill_enemy_action?.Invoke(t);
                    }

                    t.impact(WorldEnum.impact_source_type.melee, (Vector2)key_points[KEY_POINT_1].position, BattleUtility.get_target_colllider_pos(t), final_ft);

                    Sharpness_Current = Mathf.Max(Sharpness_Current - Sharpness_Loss / (1 + BattleContext.instance.melee_sharpness_durability / 1000f), Sharpness_Min);

                    action_on_hit_enemy?.Invoke();
                });
            }
            #endregion
        }

        // ----------------------------------------------------------------------------------------

        public override void Start()
        {
            base.Start();
            FSM_change_to(Device_FSM_Melee.idle);
        }

        public override void UpdateData()
        {
            melee_logics.TryGetValue(desc.melee_logic.ToString(), out var logic);
            Attack_Interval = (int)(logic.cd * (1 + Attack_Interval_Factor));
            base.UpdateData();
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
            return Current_Interval <= 0;
        }

        protected bool can_attack_check_post_cast()
        {
            return post_cast_finished;
        }

        protected virtual bool can_attack_check_distance()
        {
            return BattleUtility.get_v2_to_target_collider_pos(target_list[0], position).magnitude <= distance_can_attack;
        }

        protected bool can_attakc_check_error_angle()
        {
            if (target_list.Count == 0)
                return false;

            Vector2 target_v2 = BattleUtility.get_v2_to_target_collider_pos(target_list[0], position);
            var current_v2 = bones_direction[BONE_FOR_ROTATION];
            var delta_deg = Vector2.Angle(current_v2, target_v2);
            return delta_deg <= ATK_ERROR_DEGREE;
        }

        protected bool can_attack_check_state()
        {
            return fsm != Device_FSM_Melee.broken && state != DeviceState.stupor;
        }

        #endregion

        // ========================================================================================

        #region tick() & FSM_Change_to()

        public override void tick()
        {
            if (!is_validate && fsm != Device_FSM_Melee.broken)       //坏了
                FSM_change_to(Device_FSM_Melee.broken);

            if(state!= DeviceState.stupor)
            {
                switch (fsm)
                {
                    case Device_FSM_Melee.idle:
                        DeviceBehavior_Select_Target();
                        DeviceBehavior_Rotate_To_Target();
                        break;
                    case Device_FSM_Melee.attacking:
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

        private void FSM_change_to(Device_FSM_Melee target_fsm)
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

        public void ChangeFactor(float f)
        {
            attack_factor = f;
            Current_Interval = (int)(Current_Interval / attack_factor);

            foreach (var view in views)
                view.notify_change_anim_speed(attack_factor);
        }

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
                    atk = Mathf.CeilToInt((d.Value + Damage_Increase) * get_sharpness_modified_dmg_coef_by_stage(get_sharpness_stage())),
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

        private float get_sharpness_modified_dmg_coef_by_stage(int stage_index)
        {
            if (sharpness_dmg_coef_range == null)
                return 1f;
            if (stage_index >= 0 && stage_index < sharpness_dmg_coef_range.Count)
                return sharpness_dmg_coef_range[stage_index];
            return 1f;
        }

        private int get_sharpness_stage()
        {
            if (sharpness_range.Count == 0)
                return -1;

            for (int i = 0; i < sharpness_range.Count; i++)
                if (Sharpness_Current <= sharpness_range[i])
                    return i;

            return sharpness_range.Count;
        }

        #endregion

        // ========================================================================================

        #region DeviceBehavior(Select Target, Rorate to Target, Try Attack, Sharpen)

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

        protected bool DeviceBehavior_Melee_Try_Attack(bool ui_manual_atk, string atk_anim, Action action_on_hit_enemy = null)
        {
            var can_atk = ui_manual_atk ? can_manual_attack() : can_auto_attack();
            if (can_atk)
            {
                _atk_anim_name = atk_anim;
                this.action_on_hit_enemy = action_on_hit_enemy;
                FSM_change_to(Device_FSM_Melee.attacking);
                if (!ui_manual_atk)
                    Current_Interval = Attack_Interval;
            }
            return can_atk;
        }

        //-----------------------------------------------------------------------------------------

        protected void DeviceBehavior_Melee_Sharpen(float effect)
        {
            var sharpness_max = (int)(100 * (1 + BattleContext.instance.melee_sharpness_add / 1000f));
            var sharpness_recover = effect * Sharpness_Recover * (1 + BattleContext.instance.melee_sharpness_recover / 1000f);
            Sharpness_Current = Mathf.Min(Sharpness_Current + sharpness_recover, sharpness_max);
        }

        #endregion

        // ========================================================================================

        #region NPC Auto Work: AutoAttack & AutoSharp

        void IAttack.TryToAutoAttack()
        {
            if (Current_Interval > 0)
            {
                Current_Interval--;
                return; // 如果还没到攻击时间，则不攻击
            }

            if (Auto_Attack_Job_Content())
                Current_Interval = Attack_Interval;
        }

        /// <summary>
        /// 具体到各个设备上，自动攻击可能并不只包括“攻击”这一个动作，而是还含有其他动作，因此需要根据具体情况重载。
        /// </summary>
        /// <returns></returns>
        protected virtual bool Auto_Attack_Job_Content()
        {
            List<string> anims;
            if (atk_horizontal_anim == null)
                anims = atk_vertical_anim;
            else
            {
                if (atk_vertical_anim != null)
                    anims = atk_horizontal_anim.Concat(atk_vertical_anim).ToList();
                else
                    anims = atk_horizontal_anim;
            }

            var r = UnityEngine.Random.Range(0, anims.Count);
            return DeviceBehavior_Melee_Try_Attack(false, anims[r]);
        }

        //-----------------------------------------------------------------------------------------

        void ISharp.TryToAutoSharp()
        {
            if (_sharpen_interval > 0)
            {
                _sharpen_interval--;
                return; // 如果还没到磨刀时间，什么都不做
            }

            if (Auto_Sharpen_Job_Content())
                _sharpen_interval = SHARPEN_INTERVAL_MAX;
        }

        protected virtual bool Auto_Sharpen_Job_Content()
        {
            _sharpen_last_ticks = 120;
            return true;
        }

        #endregion

        // ========================================================================================

        #region UI Info & UI Controlled Actions

        public bool UI_Atk_Can_Horizontal { get { return _ui_atk_can_horizontal; } }

        public bool UI_Atk_Can_Vertical { get { return _ui_atk_can_vertical; } }

        public bool UI_Info_Is_Attacking()
        {
            return fsm == Device_FSM_Melee.attacking && !can_attack_check_post_cast();
        }

        public int UI_Info_Get_Sharpness_Stage()
        {
            return get_sharpness_stage();
        }

        public string UI_Info_Get_Dmg_Text_By_Sharpness_Stage(int stage_index)
        {
            if (sharpness_dmg_coef_range == null)
                return "--";
            return $"{get_sharpness_modified_dmg_coef_by_stage(stage_index) * 100:f0}";
        }

        public void UI_Controlled_Attack(bool is_horizontal, Action action_on_hit_enemy = null)
        {
            string atk_anim = is_horizontal ?
                atk_horizontal_anim[UnityEngine.Random.Range(0, atk_horizontal_anim.Count)] :
                atk_vertical_anim[UnityEngine.Random.Range(0, atk_vertical_anim.Count)];

            DeviceBehavior_Melee_Try_Attack(true, atk_anim, action_on_hit_enemy);
        }

        public void UI_Controlled_Sharpen(float effect)
        {
            if (effect > 0)
            {
                AudioSystem.instance.PlayClip(SE_grinding, false);
                DeviceBehavior_Melee_Sharpen(effect);
            }
            else
                AudioSystem.instance.StopClip(SE_grinding);
        }

        #endregion

    }
}
