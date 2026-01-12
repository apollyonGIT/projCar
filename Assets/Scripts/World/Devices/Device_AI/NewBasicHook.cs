using AutoCodes;
using Commons;
using UnityEngine;
using World.Enemys.BT;
using World.Enemys;
using World.Helpers;
using System.Collections.Generic;

namespace World.Devices.Device_AI
{
    public class NewBasicHook : Device, IAttack, IRecycle, IAttack_Device,IRotate
    {
        #region Const
        private const string BONE_FOR_ROTATION = "roll_control";
        private const string ANIM_IDLE = "idle";
        private const string ANIM_ATTACK = "shoot";
        private const string ANIM_HOOKING = "hanger_fired";
        private const string ANIM_BROKEN = "idle";
        private const string COLLIDER_1 = "collider_1";
        private const string KEY_POINT_NAME_FOR_MUZZEL = "proj_muzzel";
        private const float MAX_ROPE_DISTANCE = 10F;
        private const float SHOOT_ERROR_DEGREE = 5F;
        private const float AIMING_Y_OFFSET = 0.35F;

        private const float SPRING_TIGHTNESS_DECAY_ON_SHOOT = 0.3F;
        private const float SPRING_TIGHTNESS_DECAY_WHILE_HOOKED = 0.1F * Config.PHYSICS_TICK_DELTA_TIME;
        private const float SPRING_TIGHTNESS_ADD_BY_ROTATION = 0.0003F;
        private const float SPRING_TIGHTNESS_THRESHOLD_FOR_SHOOTING = 0.5F;
        private const float SPRING_TIGHTNESS_INIT = 1F;

        #endregion

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;
        #endregion

        public float turn_angle => rotate_angle;
        public void Rotate(float angle)
        {
            Rotate_Reel_To_Tighten_Spring(angle);
        }

        private float move_speed_shooting;
        private float move_speed_hooked;
        private float move_speed_reeling_in;

        public Vector2 hook_position = Vector2.zero;
        Vector2 muzzel_pos;
        private Vector2 hook_v;

        private Dictionary<string,int> dmg_init;
        private Dictionary<string,int> dmg_by_period;
        private int dmg_interval_max;
        private int dmg_interval_current;

        public float rope_current_length;
        public float rope_limit_length;

        private float rope_elasticity;
        private float rope_breaking_overlength;

        public int rope_current_hp;
        public int rope_max_hp = 5;

        private float attack_range_max;
        private Vector2 self_traction = Vector2.zero;          //都做v2 处理，int时只处理x
        private Vector2 opposing_traction;

        public float rotate_angle = 0;

        public enum Device_FSM_Hook
        {
            Idle,
            Ready_to_Shoot,
            Shooting,
            Shooting_reeling_in, //空军，快速收杆
            Hooked,
            Breaking_reeling_in, //崩线，快速收杆
            Broken,
        }

        private Device_FSM_Hook m_fsm;
        public Device_FSM_Hook fsm => m_fsm;

        public float Damage_Increase { get; set; }
        public float Knockback_Increase { get; set; }
        public int Attack_Interval { get; set; }
        public int Current_Interval { get; set; }       //把射击间隔当做 上弦来用
        public float Attack_Interval_Factor { get; set; } 

        public int Recycle_Interval { get; set; }
        public int Current_Recycle_Interval { get; set; }

        public int Max_Ammo { get; set; }
        public int Current_Ammo { get; set; }
        public float Reloading_Process { get; set; }
        public float Reload_Speed { get; set; }

        public ITarget hook_target;

        private float _st;
        public float spring_tightness_01
        {
            get { return _st; }
            set { _st = Mathf.Clamp(value, 0f, 1f); }
        }
        private float spring_tightness_recover
        {
            get { return 1f / Attack_Interval; }
        }

        public override void InitData(device_all rc)
        {


            rotate_speed = rc.rotate_speed.Item1;
            attack_range_max = rc.basic_range.Item2;

            hook_logics.TryGetValue(rc.hook_logic.ToString(), out var record);

            Attack_Interval = record.cd;
            Current_Interval = 0;
            Attack_Interval = (int)(record.cd * (1 + Attack_Interval_Factor));

            Recycle_Interval = record.cd;   //手动回收cd先也按射击cd来
            Current_Recycle_Interval = 0;

            move_speed_shooting = record.speed_shooting;
            move_speed_hooked = record.speed_hooked;
            move_speed_reeling_in = record.speed_reeling_in;

            dmg_init = record.damage_hit;
            dmg_by_period = record.damage_by_period;
            dmg_interval_max = record.damage_interval;

            rope_elasticity = record.elasticity;
            rope_breaking_overlength = record.breaking_overlength;
            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATION, Vector2.right);

            spring_tightness_01 = SPRING_TIGHTNESS_INIT;

            base.InitData(rc);
        }

        public override void UpdateData()
        {
            fire_logics.TryGetValue(desc.fire_logic.ToString(), out var record);
            Attack_Interval = (int)(record.cd * (1 + Attack_Interval_Factor));
            base.UpdateData();
        }
        public override void Start()
        {
            base.Start();
            FSM_change_to(Device_FSM_Hook.Idle);
        }

        //====================================================================================================
        public override void tick()
        {
            muzzel_pos = key_points[KEY_POINT_NAME_FOR_MUZZEL].position;

            if (WorldContext.instance.is_need_reset)
            {
                hook_position.x -= WorldContext.instance.reset_dis;
                muzzel_pos.x -= WorldContext.instance.reset_dis;          //感觉是没用的 有待验证   不对，好像还真有用，此时逻辑的transform还没更新
            }
            if (!is_validate)
            {
                end_traction();
                FSM_change_to(Device_FSM_Hook.Broken);
            }

            switch (m_fsm)
            {
                case Device_FSM_Hook.Idle:
                    hook_position = muzzel_pos;

                    if (spring_tightness_01 >= SPRING_TIGHTNESS_DECAY_ON_SHOOT)
                        FSM_change_to(Device_FSM_Hook.Ready_to_Shoot);
                    else
                        spring_tightness_01 += spring_tightness_recover;
                    break;
                case Device_FSM_Hook.Ready_to_Shoot:
                    hook_position = muzzel_pos;

                    if (spring_tightness_01 < 1)
                        spring_tightness_01 += spring_tightness_recover;

                    if (target_list.Count != 0)
                        if (parabolic_trajectory_prediction(muzzel_pos, BattleUtility.get_target_colllider_pos(target_list[0]), move_speed_shooting, out var expt_dir))
                        {
                            rotate_bone_to_dir(BONE_FOR_ROTATION, expt_dir);
                            if (can_shoot(expt_dir))
                                shoot_hook(expt_dir);
                        }
                    break;
                case Device_FSM_Hook.Shooting:
                    bones_direction[BONE_FOR_ROTATION] = hook_position - muzzel_pos;
                    if (hook_position.y <= Road_Info_Helper.try_get_altitude(hook_position.x) || (hook_position - muzzel_pos).magnitude >= MAX_ROPE_DISTANCE)
                    {
                        //判定为抓取失败
                        hook_target = null;
                        hook_v = Vector2.zero;
                        FSM_change_to(Device_FSM_Hook.Shooting_reeling_in);
                        break;
                    }

                    hook_v.y += Config.current.gravity * Config.PHYSICS_TICK_DELTA_TIME;
                    hook_position += hook_v * Config.PHYSICS_TICK_DELTA_TIME;

                    break;

                case Device_FSM_Hook.Hooked:
                    bones_direction[BONE_FOR_ROTATION] = hook_position - muzzel_pos;

                    if (spring_tightness_01 == 0f || hook_target == null || hook_target.hp <= 0 || !hook_target.is_interactive)
                    {
                        end_traction();
                        FSM_change_to(Device_FSM_Hook.Breaking_reeling_in);
                        break;
                    }

                    if (--dmg_interval_current <= 0)
                    {
                        hook_make_dmg(dmg_by_period);
                        dmg_interval_current = dmg_interval_max;
                    }

                    spring_tightness_01 -= SPRING_TIGHTNESS_DECAY_WHILE_HOOKED;

                    var collider_pos_of_target = BattleUtility.get_target_colllider_pos(hook_target);

                    var relative_pos = collider_pos_of_target - muzzel_pos;
                    if (WorldContext.instance.is_need_reset)
                        relative_pos.x -= WorldContext.instance.reset_dis;
                    rope_current_length = relative_pos.magnitude;

                    if (rope_current_length > rope_limit_length)
                    {
                        var dl = rope_current_length - rope_limit_length;
                        var T = Mathf.Min(dl, rope_limit_length) * rope_elasticity;
                        var direction = (hook_position - muzzel_pos).normalized;
                        Road_Info_Helper.try_get_slope(WorldContext.instance.caravan_pos.x, out var slope);
                        var road_dir = new Vector2(1, slope).normalized;
                        var angle = Vector2.SignedAngle(direction, road_dir);

                        WorldContext.instance.tractive_force_max -= (int)self_traction.x;
                        WorldContext.instance.tractive_force_max += (int)(T * Mathf.Cos(angle));
                        self_traction = new Vector2((int)(T * Mathf.Cos(angle)), 0);

                        hook_target.acc_attacher -= opposing_traction;
                        opposing_traction = -direction.normalized * T;
                        hook_target.acc_attacher += opposing_traction;
                    }

                    hook_position = collider_pos_of_target;
                    if (WorldContext.instance.is_need_reset)
                        hook_position.x -= WorldContext.instance.reset_dis;

                    /*var mag = (hook_position - muzzel_pos).magnitude;
                    if (mag >= rope_limit_length + rope_breaking_overlength || rope_current_hp <= 0)
                    {
                        end_traction();
                        target = null;      //断了
                        FSM_change_to(Device_FSM_Hook.Breaking_reeling_in);
                    }*/
                    break;

                case Device_FSM_Hook.Shooting_reeling_in:
                case Device_FSM_Hook.Breaking_reeling_in:
                    var pos_relative = hook_position - muzzel_pos;
                    var new_length_limit = Mathf.Max(0f, pos_relative.magnitude - move_speed_reeling_in * Config.PHYSICS_TICK_DELTA_TIME);

                    hook_v.y += Config.current.gravity * Config.PHYSICS_TICK_DELTA_TIME;
                    hook_position += hook_v * Config.PHYSICS_TICK_DELTA_TIME;
                    var road_height = Road_Info_Helper.try_get_altitude(hook_position.x);
                    if (hook_position.y <= road_height)
                    {
                        hook_position.y = road_height;
                        hook_v.y = 0f;
                    }
                    var pos_relative_new = hook_position - muzzel_pos;
                    if (pos_relative_new.magnitude < 0.2f)
                    {
                        hook_position = muzzel_pos;
                        FSM_change_to(Device_FSM_Hook.Idle);
                    }
                    else
                    {
                        hook_position = muzzel_pos + pos_relative_new.normalized * Mathf.Min(pos_relative_new.magnitude, new_length_limit);
                        hook_position.x += pos_relative_new.x < 0 ? WorldContext.instance.caravan_velocity.x * Config.PHYSICS_TICK_DELTA_TIME : 0;
                        bones_direction[BONE_FOR_ROTATION] = pos_relative_new;
                    }
                    break;

                case Device_FSM_Hook.Broken:
                    if (is_validate)
                        FSM_change_to(Device_FSM_Hook.Idle);
                    break;

                default:
                    break;
            }

            base.tick();
        }

        private void FSM_change_to(Device_FSM_Hook target_fsm)
        {
            m_fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_FSM_Hook.Idle:
                    ChangeAnim(ANIM_IDLE, true);
                    CloseCollider(COLLIDER_1);
                    break;
                case Device_FSM_Hook.Shooting_reeling_in:
                case Device_FSM_Hook.Shooting:
                    ChangeAnim(ANIM_ATTACK, true);
                    OpenCollider(COLLIDER_1, (ITarget t) =>
                    {
                        if (t is Enemy e)
                        {
                            IEnemy_Can_Be_Separated separatable = e.old_bt as IEnemy_Can_Be_Separated;
                            if (separatable != null)
                                separatable.Forced_Separate(e);
                            else
                                hook();
                        }
                        else
                            hook();

                        void hook()
                        {
                            hook_target = t;
                            var collider_pos_of_target = BattleUtility.get_target_colllider_pos(hook_target);
                            rope_current_hp = rope_max_hp;
                            rope_current_length = (collider_pos_of_target - muzzel_pos).magnitude;
                            rope_limit_length = rope_current_length;

                            hook_make_dmg(dmg_init);
                            dmg_interval_current = dmg_interval_max;

                            FSM_change_to(Device_FSM_Hook.Hooked);
                        }
                    });
                    break;
                case Device_FSM_Hook.Hooked:
                    ChangeAnim(ANIM_HOOKING, true);
                    CloseCollider(COLLIDER_1);
                    break;
                case Device_FSM_Hook.Breaking_reeling_in:
                    hook_v = Vector2.zero;
                    ChangeAnim(ANIM_HOOKING, true);
                    CloseCollider(COLLIDER_1);
                    break;
                case Device_FSM_Hook.Broken:
                    ChangeAnim(ANIM_BROKEN, true);
                    CloseCollider(COLLIDER_1);
                    break;
                default:
                    break;
            }
        }

        //----------------------------------------------------------------------------------------------------

        private void shoot_hook(Vector2 dir)
        {
            //Current_Interval = Attack_Interval;
            spring_tightness_01 -= SPRING_TIGHTNESS_DECAY_ON_SHOOT;
            hook_v = move_speed_shooting * dir.normalized + WorldContext.instance.caravan_velocity;
            FSM_change_to(Device_FSM_Hook.Shooting);
        }

        private void end_traction()
        {
            WorldContext.instance.tractive_force_max -= (int)self_traction.x;
            self_traction = Vector2.zero;

            if (hook_target != null)
            {
                hook_target.acc_attacher -= opposing_traction;
                opposing_traction = Vector2.zero;
            }
        }

        private void hook_make_dmg(Dictionary<string,int> atk)
        {
            var attack_datas = ExecDmg(hook_target,atk);

            foreach(var attack_data in attack_datas)
            {
                attack_data.calc_device_coef(this);
                hook_target.hurt(this, attack_data, out var dmg_data);
                BattleContext.instance.ChangeDmg(this, dmg_data.dmg);
                hook_target.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);
            }

            if (hook_target.hp <= 0)
            {
                kill_enemy_action?.Invoke(hook_target);
            }
        }

        protected override bool target_in_radius(ITarget t)
        {
            var tp = BattleUtility.get_target_colllider_pos(t);

            if (WorldContext.instance.is_need_reset)
                tp.x -= WorldContext.instance.reset_dis;

            var t_distance = (tp - muzzel_pos).magnitude;
            return t_distance >= desc.basic_range.Item1 && t_distance <= desc.basic_range.Item2;
        }

        protected override bool try_get_target()
        {
            var target = BattleUtility.select_target_in_circle_min_angle(muzzel_pos, bones_direction[BONE_FOR_ROTATION], desc.basic_range.Item2, faction, (ITarget t) =>
            {
                return target_can_be_selected(t);
            });

            if (target != null)
                target_list.Add(target);

            return target != null;
        }


        private bool can_shoot(Vector2 target_dir)
        {
            return spring_tightness_01 >= SPRING_TIGHTNESS_THRESHOLD_FOR_SHOOTING && can_shoot_check_angle(target_dir);

            bool can_shoot_check_angle(Vector2 target_dir)
            {
                return Mathf.Abs(Vector2.SignedAngle(bones_direction[BONE_FOR_ROTATION], target_dir)) <= SHOOT_ERROR_DEGREE;
            }
        }


        private bool parabolic_trajectory_prediction(Vector2 muzzel_pos, Vector2 target_pos, float init_speed, out Vector2 target_dir)
        {
            var L = target_pos.x - muzzel_pos.x;
            var H = target_pos.y - muzzel_pos.y;
            var left = L < 0;
            if (left)
            {
                L = -L;
            }
            var discriminant = (H - Config.current.gravity * Mathf.Pow(L / init_speed, 2)) / Mathf.Sqrt(L * L + H * H);
            if (Mathf.Abs(discriminant) > 1)
            {
                target_dir = Vector2.zero;
                return false;
            }
            var M = Mathf.Atan2(-H, L);
            var angle = (Mathf.Asin(discriminant) - M) * 0.5f;
            target_dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            if (left)
            {
                target_dir.x = -target_dir.x;
            }
            return true;
        }

        public void Rotate_Reel_To_Tighten_Spring(float input_angle)
        {
            rotate_angle += input_angle;

            if (input_angle < 0)
                spring_tightness_01 -= input_angle * SPRING_TIGHTNESS_ADD_BY_ROTATION;
        }


        void IAttack.TryToAutoAttack()
        {
            if (!player_oper && fsm == Device_FSM_Hook.Ready_to_Shoot)
            {
                if (target_list.Count == 0)
                    try_get_target();

                if (target_list.Count != 0)
                    if (parabolic_trajectory_prediction(muzzel_pos, BattleUtility.get_target_colllider_pos(target_list[0]), move_speed_shooting, out var expt_dir))
                    {
                        rotate_bone_to_dir(BONE_FOR_ROTATION, expt_dir);
                        if (can_shoot(expt_dir))
                            shoot_hook(expt_dir);
                    }
            }
        }

        void IRecycle.TryToAutoRecycle()
        {
            if (!player_oper && (fsm == Device_FSM_Hook.Idle || fsm == Device_FSM_Hook.Ready_to_Shoot))
                Rotate_Reel_To_Tighten_Spring(-1f);
        }

        public void TryToAutoRotate()
        {
            
        }
    }
}
