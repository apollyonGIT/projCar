using Commons;
using System.Collections.Generic;
using UnityEngine;
using World.Caravans;
using World.Helpers;


namespace World.Enemys.BT
{
    public interface IEnemy_Can_Shoot
    {
        int Shoot_CD { get; set; }
        int Shoot_CD_Max { get; set; }
        bool Shoot_Finished { get; set; }
        float Projectile_Speed { get; set; }
        (float, float) Projectile_Speed_Range { get; set; }
        IEnemy_Can_Shoot I_Shoot { get; set; }

        void Init_Shooting_Data(Enemy e)
        {
            AutoCodes.monsters.TryGetValue($"{e._desc.id}", out var monster_r);
            if (AutoCodes.fire_logics.TryGetValue($"{monster_r.fire_logic}", out var fire_logic_r))
            {
                Shoot_CD_Max = fire_logic_r.cd;
                Shoot_CD = Random.Range(0, Shoot_CD_Max);
                Projectile_Speed_Range = (fire_logic_r.speed.Item1, fire_logic_r.speed.Item2);
                Projectile_Speed = Get_Projectile_Speed(e, 0f);
            }
        }

        void Monster_Shoot(Enemy actor, string muzzle_bone_name, Vector2 expected_pos, float target_v)
        {
            actor.try_get_bone_pos(muzzle_bone_name, out Vector2 muzzle_pos);
            Vector2 shoot_dir;
            var dx = Mathf.Abs(expected_pos.x - muzzle_pos.x);
            Projectile_Speed = Get_Projectile_Speed(actor, dx * 0.05f + target_v * 0.1f);
            if (!Monster_Common_Action.Parabolic_Trajectory_Prediction_New(muzzle_pos, expected_pos, Projectile_Speed, out Vector2 target_dir))
                target_dir.y = Mathf.Max(target_dir.y, Mathf.Abs(target_dir.x));
            shoot_dir = target_dir;
            Enemy_Shoot_Helper.@do(actor, muzzle_pos, shoot_dir, Projectile_Speed);
            Shoot_Finished = true;
            Shoot_CD = Shoot_CD_Max;
        }

        float Get_Projectile_Speed(Enemy e, float t) => Mathf.Lerp(Projectile_Speed_Range.Item1, Projectile_Speed_Range.Item2, t);
    }

    // ----------------------------------------------------------------------------------------------------------------------

    public interface IEnemy_Can_Jump
    {
        public enum Enemy_Jump_Mode
        {
            Jump_To,
            Jump_Away,
            Jump_Around,
        }
        bool Jump_Finished { get; set; }
        int Jump_CD_Ticks { get; set; }
        Enemy_Jump_Mode Jump_Mode { get; set; }
        IEnemy_Can_Jump I_Jump { get; set; }
        float Jump_Speed_Min { get; set; }
        float Jump_Speed_Max { get; set; }
        float Jump_Y_Speed_Min { get; set; }
        float X_Distance_Mod_Coef { get; set; }
        float Y_Distance_Mod_Coef { get; set; }

        /// <summary>
        /// 按照当前的 Jump_Mode 进行一次跳跃
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="target_pos"></param>
        /// <param name="cd"></param>
        /// <returns>Jump Direction.X</returns>
        float? Jump_By_Mode(Enemy actor, Vector2 target_pos, int cd)
        {
            Jump_CD_Ticks = cd;
            Jump_Finished = true;

            /*
            if (actor.mover.move_type != EN_enemy_move_type.Slide)
            {
                Be_Panic_In_Air();
                return null;
            }*/

            Vector2 v2;
            actor.mover.move_type = EN_enemy_move_type.Hover;

            switch (Jump_Mode)
            {
                case Enemy_Jump_Mode.Jump_To:
                    v2 = get_jump_v2(target_pos - actor.pos, Jump_Speed_Max);
                    break;
                case Enemy_Jump_Mode.Jump_Away:
                    v2 = get_jump_v2(actor.pos - target_pos, Jump_Speed_Max);
                    break;
                case Enemy_Jump_Mode.Jump_Around:
                default:
                    v2 = get_jump_v2(Random.Range(0, 2) == 1 ? Vector2.right : Vector2.left, Jump_Speed_Min);
                    break;
            }
            actor.velocity.x += v2.x;
            actor.velocity.y = v2.y;

            return v2.x;

            Vector2 get_jump_v2(Vector2 jump_towards, float speed_max)
            {
                var x_dis = jump_towards.x;
                Vector2 jump_velocity = jump_towards.normalized * Jump_Speed_Min;
                jump_velocity += WorldContext.instance.caravan_velocity;
                var x_distance_parm = Mathf.Min(15f, x_dis);
                jump_velocity.x += x_distance_parm * X_Distance_Mod_Coef;
                if (jump_velocity.magnitude > speed_max)
                    jump_velocity = jump_velocity.normalized * speed_max;
                jump_velocity.y = Mathf.Max(jump_velocity.y, Jump_Y_Speed_Min * (1 + Mathf.Max(x_distance_parm, 0F) * Y_Distance_Mod_Coef));

                return jump_velocity;
            }
        }

        /// <summary>
        /// 被撞角撞飞时调用此函数。
        /// 若 ft_input > 阈值，则切换至惊恐状态。
        /// </summary>
        /// <param name="ft_input"></param>
        void Get_Rammed(float ft_input)
        {
            if (ft_input > 15)
                Be_Panic_In_Air();
        }

        /// <summary>
        /// 实现时：控制状态机，切换至Default状态。
        /// </summary>
        public void Be_Panic_In_Air();
    }

    // ----------------------------------------------------------------------------------------------------------------------

    public interface IEnemy_Can_Be_Separated
    {
        bool Separate_All { get; set; }
        Dictionary<string, int> Sub_Monsters { get; set; }

        sealed void Forced_Separate(Enemy cell)
        {
            cell.hp = 0;
            Separate_All = true;
        }

        sealed void Die_Of_Being_Seperated(Enemy cell)
        {
            // 代替 Monster_Common_Action.Basic_Die(cell);
            if (Separate_All)
            {
                foreach (var sub in Sub_Monsters)
                    cell.mgr.pd.add_enemy_directly_req(0, (uint)sub.Value, get_init_pos(sub.Key), "Default");
                Monster_Common_Action.Set_Death_VFX_By_Index(cell, 4);
            }
            else
            {
                // 0 = generate mount ; 1 = generate rider
                var r = Random.Range(0, 2);
                switch (r)
                {
                    case 0:
                        cell.mgr.pd.add_enemy_directly_req(0, (uint)Sub_Monsters["Mount"], get_init_pos("Mount"), "Default");
                        break;
                    case 1:
                        cell.mgr.pd.add_enemy_directly_req(0, (uint)Sub_Monsters["Rider"], get_init_pos("Rider"), "Default");
                        break;
                    default:
                        break;
                }
                Monster_Common_Action.Set_Death_VFX_By_General_Rule(cell);
            }

            cell.fini();

            Vector2 get_init_pos(string key)
            {
                switch (key)
                {
                    case "Rider":
                        return cell.pos - cell.mgr.ctx.caravan_pos;
                    case "Mount":
                    default:
                        return (Vector2)cell.collider_pos - cell.mgr.ctx.caravan_pos;
                }
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------------------------

    public interface IEnemy_Can_Flyaround
    {
        (float, float) Flyaround_Deg { get; }
        (float, float) Flyaround_Radius { get; }
        float Flyaround_Radius_Ratio { get; }  // Vertical_Radius divided by Horizontal_Radius
        Vector2 Flyaround_Relative_Following_Pos { get; set; }

        void Init_Or_Reset_Relative_Following_Pos()
        {
            // also executed when initialized 
            var radius = Random.Range(Flyaround_Radius.Item1, Flyaround_Radius.Item2);
            var deg = Random.Range(Flyaround_Deg.Item1, Flyaround_Deg.Item2);
            var rad = deg * Mathf.Deg2Rad;
            var angle_fix = Mathf.Lerp(Flyaround_Radius_Ratio, 1f, Mathf.Abs(radius - 90f) / 90f);
            Flyaround_Relative_Following_Pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius * angle_fix;
        }

        void Flyaround_Per_Tick(Enemy actor, Vector2 target_pos)
        {
            actor.position_expt = Flyaround_Relative_Following_Pos + target_pos;

            float d2 = Mathf.Pow((actor.pos.x - actor.position_expt.x), 2) + Mathf.Pow((actor.pos.y - actor.position_expt.y), 2);
            if (d2 < 1f)
                Init_Or_Reset_Relative_Following_Pos();
        }
    }

    // ======================================================================================================================

    public abstract class Enemy_Monster_Basic_BT : Enemy_Basic_BT
    {
        #region CONST
        private const float CHASING_POINT_X_OFFSET_MAX = 10f;   // Car.Pos + offset_x = Self.Expt_Pos
        private const float CHASING_POINT_X_OFFSET_MIN = -1f;   // Car.Pos + offset_x = Self.Expt_Pos

        private const float CHASING_LAG_DISTACEN_MAX = 10f;
        private const float CHASING_LAG_DISTACEN_MIN = 1f;
        #endregion

        private float chasing_lag_distance = Random.Range(CHASING_LAG_DISTACEN_MIN, CHASING_LAG_DISTACEN_MAX);

        protected ushort Ticks_In_Current_State;


        protected bool Check_State_Time(ushort ticks)
        {
            return Ticks_In_Current_State >= ticks;
        }

        protected void Set_Chasing_Speed(Enemy cell, float v_base, float v_max)   // 2025.03.24 关卡追逐优化
        {
            var target_velocity_x = Forced_Get_Target_Vel().x;
            var v_delta = target_velocity_x - v_base;
            if (v_delta <= 0)
            {
                set_v(v_base);
                return;
            }

            var chasing_expt_pos = Forced_Get_Target_Pos().x + CHASING_POINT_X_OFFSET_MAX;  // 追逐目标的期望位置
            if (cell.pos.x >= chasing_expt_pos)
            {
                set_v(v_base);
                return;
            }

            // v_delta 此时必定是大于0的
            float chasing_expt_offset = CHASING_POINT_X_OFFSET_MAX + (CHASING_POINT_X_OFFSET_MIN - CHASING_POINT_X_OFFSET_MAX) / (v_max - v_base) * v_delta;
            float v = v_base + v_delta / -chasing_lag_distance * (cell.pos.x - chasing_expt_pos);  // y = y1 + (y2-y1)/(x2-x1) * (x-x1)
            set_v(v);

            // SubFunc
            void set_v(float _v) => cell.speed_expt = Monster_Common_Action.Get_Speed_Expt_By_Clamp(_v, v_max);
        }
    }


    public abstract class Enemy_Basic_BT
    {
        protected ushort Ticks_Target_Has_Been_Locked_On;
        protected ITarget Target_Locked_On;

        protected void Lock_Target()
        {
            // can only be used when target is null
            SeekTarget_Helper.random_player_part(out var target);
            Target_Locked_On = target;
            Ticks_Target_Has_Been_Locked_On = 0;
        }

        /// <summary>
        /// 获取当前锁定目标的坐标
        /// </summary>
        /// <returns>
        /// <para>目标为空时：null</para>
        /// <para>目标为Caravan时：Caravan_Pos + Vector2.up</para>
        /// <para>目标为Enemy时：Enemy的Collider位置</para>
        /// <para>其他情况下：ITarget的Position值</para>
        /// </returns>
        protected Vector2? Get_Target_Pos()
        {
            if (Target_Locked_On == null)
                return null;

            if (Target_Locked_On is Caravan)              // 车体的坐标逻辑位置紧贴地面，需要添加 Y_offset 以修正瞄准位置。 
                return Target_Locked_On.Position + Vector2.up;

            if (Target_Locked_On is Enemy)
                return BattleUtility.get_target_colllider_pos(Target_Locked_On);

            return Target_Locked_On.Position;
        }

        protected Vector2? Get_Target_Vel()
        {
            return Target_Locked_On == null ? null : Target_Locked_On.velocity;
        }

        protected Vector2 Forced_Get_Target_Pos() => Get_Target_Pos() ?? WorldContext.instance.caravan_pos + Vector2.up;
        protected Vector2 Forced_Get_Target_Vel() => Get_Target_Vel() ?? WorldContext.instance.caravan_velocity;

        /// <summary>
        /// 当实现了 IEnemy_Can_Be_Separated 接口时，需要重写此函数
        /// </summary>
        /// <param name="self"></param>
        virtual protected void basic_die(Enemy self)
        {
            Monster_Common_Action.Set_Death_VFX_By_General_Rule(self);

            self.fini();
        }

        public void get_killed(Enemy self)
        {
            basic_die(self);
        }
    }

    // ======================================================================================================================


    internal static class Monster_Common_Action
    {
        internal static bool Parabolic_Trajectory_Prediction(Vector2 start_pos, Vector2 target_pos, float init_speed, out Vector2 target_dir, bool high_trajectory = false)
        {
            var L = target_pos.x - start_pos.x;
            var H = target_pos.y - start_pos.y;
            var to_left = L < 0;
            var xor = to_left ^ high_trajectory;
            if (xor)
                L = -L;
            var discriminant = (H - Config.current.gravity * Mathf.Pow(L / init_speed, 2)) / Mathf.Sqrt(L * L + H * H);
            if (Mathf.Abs(discriminant) > 1)
            {
                target_dir = target_pos - start_pos;
                return false;
            }
            var M = Mathf.Atan2(-H, L);
            var angle = (Mathf.Asin(discriminant) - M) * 0.5f;
            target_dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            if (xor)
                target_dir.x = -target_dir.x;
            return true;
        }


        internal static bool Parabolic_Trajectory_Prediction_New(Vector2 start_pos, Vector2 target_pos, float init_speed, out Vector2 target_dir, bool high_trajectory = false)
        {
            var L = target_pos.x - start_pos.x;
            var H = target_pos.y - start_pos.y;
            var g = -Config.current.gravity;

            var target_left = L < 0;
            if (target_left)
                L = -L;

            var discriminant_t = init_speed * init_speed - 2 * g * H < 0;
            if (discriminant_t)
                return false_return(out target_dir);

            #region Parms Explain
            // define: sq = sqrt( L^2 + H^2 )
            // ( g * L^2 / v^2 + H ) / sq = ( L / sq * sin2θ - H / sq * cos2θ )
            // define: cosφ = L / sq ; sinφ = H / sq
            // ( g * L^2 / v^2 + H ) / sq = sin( 2θ - φ )
            #endregion

            var discriminant_sin = (g * Mathf.Pow(L / init_speed, 2) + H) / Mathf.Sqrt(L * L + H * H);
            if (Mathf.Abs(discriminant_sin) > 1)
                return false_return(out target_dir);

            // all angles are in radians
            var phi = Mathf.Atan2(H, L);   // tanφ = H / L

            var double_theta_1 = Mathf.Asin(discriminant_sin);
            var double_theta_2 = Mathf.PI - double_theta_1;

            float repeat_right_semisphere(float input) => (input + Mathf.PI * 0.5f) % Mathf.PI - Mathf.PI * 0.5f;

            var angle_rad_1 = repeat_right_semisphere((double_theta_1 + phi) * 0.5f);
            var angle_rad_2 = repeat_right_semisphere((double_theta_2 + phi) * 0.5f);

            var shooting_angle = high_trajectory ? Mathf.Max(angle_rad_1, angle_rad_2) : Mathf.Min(angle_rad_1, angle_rad_2);

            #region Check
            //float vx = init_speed * Mathf.Cos(shooting_angle);
            //float vy = init_speed * Mathf.Sin(shooting_angle);
            //float tx_check = L / vx;
            //float delta_check = Mathf.Sqrt(vy * vy - 2 * g * H);
            //float ty_check_1 = (vy + delta_check) / g;
            //float ty_check_2 = (vy - delta_check) / g;
            #endregion

            target_dir = new Vector2(Mathf.Cos(shooting_angle), Mathf.Sin(shooting_angle));

            if (target_left)
                target_dir.x = -target_dir.x;

            return true;

            bool false_return(out Vector2 target_dir)
            {
                target_dir = target_pos - start_pos;
                var tdx = Mathf.Abs(target_dir.x);
                if (target_dir.y < tdx)
                    target_dir.y = tdx;
                return false;
            }
        }

        internal static float Get_Damage_Mod_By_Distance(float distance, float radius, float min_dmg_pcts)
        {
            // Mostly For Explosion
            return (min_dmg_pcts - 1) * Mathf.Pow(distance / radius, 2) + 1;
            // Radius >= Distance
        }

        internal static void Sync_Pos_By_World_Pos_Reset(ref Vector2 pos_to_be_sync)
        {
            // 当某一位置在一段时间内相对于场景不变时，需要调用此函数
            if (WorldContext.instance.is_need_reset)
                pos_to_be_sync.x += WorldContext.instance.reset_dis;
        }

        #region 设置死亡特效
        internal static void Set_Death_VFX_By_Index(Enemy cell, int index) => cell.death_vfx_index = index;
        internal static void Set_Death_VFX_By_General_Rule(Enemy cell) => Set_Death_VFX_By_Index(cell, Random.Range(0, 4));
        #endregion


        #region Set Speed
        internal static float Get_Speed_Expt_By_Clamp(float v_expt, float v_limit_max) => Mathf.Clamp(v_expt, 0, v_limit_max);

        internal static void Set_Speed_Expt_By_Remaining_HP(Enemy cell, float v_expt, float v_limit_max)
        {
            // 使怪物移速随血量降低而降低
            cell.speed_expt = Get_Speed_Expt_By_Clamp(v_expt, v_limit_max);
            var t_hp = 0.8f * (cell.hp_max - cell.hp) / cell.hp_max;
            var t_v = (v_limit_max - t_hp * cell.speed_expt) / v_limit_max;
            cell.speed_expt *= t_v;
        }
        #endregion
    }
}
