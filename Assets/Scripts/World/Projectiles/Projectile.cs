using AutoCodes;
using Commons;
using Foundations;
using System;
using System.Collections.Generic;
using UnityEngine;
using World.Devices;
using World.Enemys;
using World.Helpers;
using World.VFXs;
using static World.WorldEnum;

namespace World.Projectiles
{
    public enum MovementStatus
    {
        normal,
        in_object,
        in_ground,
    }


    public class Projectile : ITarget
    {

        private const float MAX_SCALE = 3F;

        ProjectileMgr pmgr;

        public projectile desc;

        public Vector2 position;
        public Vector2 velocity;
        public Vector2 direction;

        private float m_scale = 1;
        public float scale {
            get
            {
                return m_scale;
            }
            set 
            {
                m_scale = Mathf.Min(value, MAX_SCALE);
            }
        }
        public float mass;
        public int life_ticks;
        protected int life_ticks_init;
        public float radius;
        public List<Attack_Data> attack_datas;
        public MovementStatus movement_status = MovementStatus.normal;
        public Faction faction;
        public int view_prefab_index = 0;

        public bool validate = true;

        public ITarget emitter = null; // 发射者
        public ITarget in_target = null;
        protected Vector2 pos_offset_in_object = Vector2.zero;
        protected Vector2 direction_in_object = Vector2.zero;

        public ITarget last_hit = null;

        public float init_speed { protected set; get; }  // For Caculating Actual Damage When Hit Target
        protected float rot_speed;
        protected float rot_propulsion;
        protected Vector2 last_position;

        public Action<ITarget> hit_target_event;
        public Action<Projectile> tick_event;

        Action<ITarget> m_tick_handle;

        Vector2 ITarget.Position => position;

        Faction ITarget.Faction => faction;

        float ITarget.Mass => mass;

        bool ITarget.is_interactive => true;

        int ITarget.hp => validate ? 1 : 0;
        int ITarget.hp_max => validate ? 1 : 0;

        Vector2 ITarget.acc_attacher { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        Vector2 ITarget.direction => direction;

        Vector2 ITarget.velocity => velocity;

        private const float VELOCITY_IN_GROUND_REMAINING_PER_TICK = 0.2f;
        private const float VALIDATE_DISTANCE = 80F;                //当飞射物距离车距离超过一定值 判定为飞射物没有维护的价值

        Dictionary<string, LinkedList<Action<ITarget>>> ITarget.buff_flags { get => m_buff_flags; set => m_buff_flags = value; }
        Dictionary<string, LinkedList<Action<ITarget>>> m_buff_flags = new();

        List<string> ITarget.fini_buffs { get => m_fini_buffs; set => m_fini_buffs = value; }
        List<string> m_fini_buffs = new();

        public event Action<ITarget> fini_callback_event;

        private const int MAX_IN_OBJECT_TICKS = 10;

        private int in_object_ticks = 0;

        float ITarget.def_cut_rate { get => m_def_cut_rate; set => m_def_cut_rate = value; }
        float m_def_cut_rate;

        //==================================================================================================

        public virtual void Init(projectile _desc,
            Vector2 dir, Vector2 position, Vector2 shooter_velocity,
            float rnd_angle_1, float rnd_angle_2, float speed, float init_speed,
            Faction f, int life_ticks, ITarget emitter, List<Attack_Data> atk_datas, float rot_speed, Action<ITarget> hit_event = null)
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out pmgr);

            desc = _desc;
            mass = desc.mass;
            this.life_ticks = life_ticks;
            life_ticks_init = life_ticks;

            this.emitter = emitter;
            radius = desc.radius;
            faction = f;
            attack_datas = atk_datas;
            this.rot_speed = rot_speed;
            rot_propulsion = (UnityEngine.Random.value + UnityEngine.Random.value - 1f) * desc.propulsion_error;  // 取两次value相加是为了改变概率密度，不能改成乘2
            hit_target_event = hit_event;

            var rnd = UnityEngine.Random.Range(rnd_angle_1, rnd_angle_2);
            direction = (Quaternion.AngleAxis(rnd, Vector3.forward) * dir).normalized;
            velocity = direction * speed + shooter_velocity;

            this.init_speed = init_speed;

            this.position = position;
            last_position = position;
        }

        public virtual void tick()
        {
            switch (movement_status)
            {
                case MovementStatus.normal:
                    var acc_x = -velocity.x * desc.k_feedback / desc.mass;
                    var acc_y = Config.current.gravity;

                    if (faction == Faction.player)
                    {
                        acc_y *= (1 + BattleContext.instance.ranged_player_ammo_gravity / 1000f);
                    }

                    Vector2 ammo_acc = new Vector2(acc_x, acc_y);
                    if (desc.propulsion_force > 0)
                    {
                        ammo_acc += direction.normalized * desc.propulsion_force / desc.mass;
                        rot_speed += rot_propulsion * Config.PHYSICS_TICK_DELTA_TIME;
                    }

                    velocity += ammo_acc * Config.PHYSICS_TICK_DELTA_TIME;
                    position += velocity * Config.PHYSICS_TICK_DELTA_TIME;  // Move
                    rotate(); // Rotate

                    // Check If Hit Ground
                    var road_height = Road_Info_Helper.try_get_altitude(position.x);
                    if (position.y <= road_height)
                        if (desc.exploded_by_lifetime[1])
                            projectile_explode();
                        else
                            HitGround(road_height);

                    switch (desc.detection_type)
                    {
                        case "Radius":
                            if (check_and_select_single_target(out var target_single))
                                if (desc.exploded_by_lifetime[2])
                                    projectile_explode();
                                else
                                    hit_enemy_check(target_single);
                            break;
                        case "Ray":
                            if (check_and_select_targets_collection(out var targets_collection))
                                if (desc.exploded_by_lifetime[2])
                                    projectile_explode();
                                else
                                    foreach (var t in targets_collection)
                                    {
                                        if (faction == t.Faction)  // 命中盾牌有可能会改变飞射物阵营
                                            continue;
                                        hit_enemy_check(t);
                                    }
                            break;
                        default:
                            break;
                    }

                    break;
                case MovementStatus.in_object:
                    movement_in_object();
                    if (in_target.hp <= 0)
                        RemoveSelf();
                    break;
                case MovementStatus.in_ground:
                    if (velocity.magnitude < 0.1f)
                        break;
                    velocity *= VELOCITY_IN_GROUND_REMAINING_PER_TICK;
                    position += velocity * Config.PHYSICS_TICK_DELTA_TIME;  // Move
                    rotate(); // Rotate

                    break;
            }

            if (--life_ticks <= 0)
            {
                if (desc.exploded_by_lifetime[0])
                    projectile_explode();
                RemoveSelf();
            }

            var dis = (WorldContext.instance.caravan_pos - position).magnitude;
            if (dis > VALIDATE_DISTANCE)
                RemoveSelf();

            m_tick_handle?.Invoke(this);
            m_tick_handle = null;

            last_position = position;
        }

        public int GetLifeTick()
        {
            return life_ticks_init - life_ticks;
        }

        public virtual void tick1()
        {
            // Nothing for now
        }

        protected void movement_status_change_to(MovementStatus expected_movement_status)
        {
            movement_status = expected_movement_status;
            switch (expected_movement_status)
            {
                case MovementStatus.in_object:
                    // temp: Arrows should not stick in device or carbody
                    if (!(in_target is Enemys.Enemy))
                        RemoveSelf();

                    break;
            }
        }


        /// <summary>
        /// 这个的意义为何 有待研究 2025.8.12
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected List<Attack_Data> modify_attack_data(ITarget target)
        {
            for (int i = 0; i < attack_datas.Count; i++)
            {
                var result = attack_datas[i];
                var target_v = Vector2.zero;
                if (target is Enemy e)
                    target_v = e.velocity;

                if (init_speed == 0)
                { }
                else
                {
                    var v_ration = (velocity - target_v).magnitude / init_speed;
                    var coef = Mathf.Lerp(1, v_ration, desc.speed_dmg_mod);
                    result.atk = (int)(result.atk * Mathf.Pow(coef, 2));
                }
                result.critical_chance += BattleContext.instance.global_critical_chance;
                result.critical_dmg_rate += BattleContext.instance.global_critical_chance;

                attack_datas[i] = result;
            }
            return attack_datas;
        }

        private void projectile_explode()
        {
            //生成逻辑爆炸
            var targets = BattleUtility.select_all_target_in_circle(position, desc.explode_radius, faction);
            foreach (var t in targets)
            {
                if (t != null && t.is_interactive)
                {
                    Attack_Data attack_data = new()
                    {
                        atk = desc.explode_dmg,
                    };
                    t.hurt(this, attack_data, out var dmg_data);
                    if (emitter is Device d)
                    {
                        BattleContext.instance.ChangeDmg(d, dmg_data.dmg);
                        if (t.hp <= 0)
                        {
                            d.kill_enemy_action?.Invoke(t);
                        }
                    }
                    t.impact(impact_source_type.melee, position, BattleUtility.get_target_colllider_pos(t), desc.explode_ft);
                }
            }
            //生成特效 仅外观
            Mission.instance.try_get_mgr("VFX", out VFXMgr vmgr);
            vmgr.AddVFX(desc.explode_vfx, Config.PHYSICS_TICKS_PER_SECOND, position);
            Audio.AudioSystem.instance.PlayOneShot(desc.explode_se);
            RemoveSelf();
        }

        protected virtual void rotate()
        {
            switch (desc.rotate_rule)
            {
                case "By_Velocity":
                    direction = velocity.normalized;
                    break;
                case "Ignored":
                    break;
                case "Free":
                    direction = Quaternion.AngleAxis(rot_speed * Config.PHYSICS_TICK_DELTA_TIME, Vector3.forward) * direction;
                    break;
                default:
                    break;
            }
        }


        //--------------------------------------------------------------------------------
        protected virtual bool check_and_select_single_target(out ITarget target_selected, bool same_last_target = false)
        {
            target_selected = BattleUtility.select_target_in_circle(position, radius, faction,
                (ITarget t) => (t != last_hit) ^ same_last_target && t.is_interactive);
            return target_selected != null;
        }

        protected virtual bool check_and_select_targets_collection(out List<ITarget> targets_selected, bool same_last_target = false)
        {
            var size_factor = 1 + BattleContext.instance.ranged_ammo_bigger / 1000f;
            var axis = (position - last_position).normalized;
            var bv = new Vector2(1 / axis.x, -1 / axis.y).normalized;
            if (axis.x == 0)
            {
                bv = new Vector2(1, 0);
            }
            if (axis.y == 0)
            {
                bv = new Vector2(0, 1);
            }

            var p1 = last_position + bv * radius * size_factor;
            var p2 = position - bv * radius * size_factor;

            targets_selected = BattleUtility.select_all_target_in_rect(p1, p2, faction, (ITarget t) =>
            {
                return t != last_hit;
            });
            return targets_selected.Count > 0;
        }
        //--------------------------------------------------------------------------------


        protected virtual void movement_in_object()
        {
            if (in_object_ticks < 0)
            {
                var current_dir = in_target.direction;
                var angel = Vector2.SignedAngle(direction_in_object, current_dir);
                Vector2 current_offset = Quaternion.AngleAxis(angel, Vector3.forward) * pos_offset_in_object;

                position = in_target.Position + current_offset;
            }
            else
            {
                var acc_y = Config.current.gravity;
                Vector2 ammo_acc = new Vector2(0, acc_y);
                if (desc.propulsion_force > 0)
                {
                    ammo_acc += direction.normalized * desc.propulsion_force / desc.mass;
                    rot_speed += rot_propulsion * Config.PHYSICS_TICK_DELTA_TIME;
                }

                velocity += ammo_acc * Config.PHYSICS_TICK_DELTA_TIME;
                velocity *= 0.95f;  //降低速度
                position += velocity * Config.PHYSICS_TICK_DELTA_TIME;  // Move

                if (--in_object_ticks >= 0)
                    return;

                // last，check if is still collided with the same target
                if (check_and_select_single_target(out var target, true))
                {
                    //记录插入时 物体的 position 与 direction
                    pos_offset_in_object = position - in_target.Position;
                    direction_in_object = in_target.direction;
                }
                else
                {
                    movement_status_change_to(MovementStatus.normal);
                }
            }
        }


        private void hit_enemy_check(ITarget target_hit)
        {
            var ts = target_hit as Devices.Device_AI.IShield;

            // Countered by Shield
            if (ts != null)
                if (ts.Try_Rebound_Projectile(this, velocity))
                    return;

            HitEnemy(target_hit);
        }

        public virtual void HitEnemy(ITarget target_hit)
        {
            if (faction == Faction.player)
                pmgr.hit_enemy_event?.Invoke(this);
            hit_enemy(target_hit);
        }

        public virtual void HitGround(float road_height)
        {
            hit_ground(road_height);
        }

        private void hit_ground(float road_height)
        {
            switch (desc.hit_ground_rule)
            {
                case "Arrow":
                    movement_status_change_to(MovementStatus.in_ground);
                    break;
                case "Bullet":
                    bullet_hit_ground(road_height);
                    break;
                case "Big_Slow":
                    big_slow_hit_ground(road_height);
                    break;
                case "Destroy":
                    validate = false;
                    break;
                default:
                    break;
            }
        }

        private void bullet_hit_ground(float road_height)
        {
            Road_Info_Helper.try_get_slope(position.x, out var slope);
            var ground_slope = new Vector2(1f, slope);
            var ground_normal = new Vector2(-slope, 1f);

            var v_landing_parall = Vector2.Dot(velocity, ground_slope) * ground_slope;
            var v_landing_vertical = Vector2.Dot(velocity, ground_normal) * ground_normal;

            var ek = mass * Mathf.Pow(velocity.magnitude, 2);
            var vtm = v_landing_vertical.magnitude;
            var vpm = v_landing_parall.magnitude;

            if (ek > Config.current.bullet_bounce_threshold_ekmin && vtm < Config.current.bullet_bounce_threshold_vmax)
            {
                v_landing_vertical *= -Config.current.bullet_surface_bounce_coef;
                if (vpm != 0)
                    v_landing_parall *= (vpm - Mathf.Min(vpm, vtm * (1 + Config.current.bullet_surface_bounce_coef) * Config.current.surface_friction)) / vpm;
                rot_speed *= 0.6f;
                velocity = v_landing_vertical + v_landing_parall;
                position = new Vector2(position.x, road_height);
                direction = velocity.normalized;

                pmgr.rebound_event?.Invoke(this);
                if (velocity.magnitude > desc.vfx_v_threshold && Vector2.SqrMagnitude(position - WorldContext.instance.caravan_pos) < 400f)
                {
                    if (desc.vfx_on_hit_ground != null)
                        Vfx_Helper.InstantiateVfx(desc.vfx_on_hit_ground, 240, position);
                    if (desc.se_on_hit_ground != null)
                        Audio.AudioSystem.instance.PlayOneShot(desc.se_on_hit_ground);
                }
            }
            else
            {
                movement_status_change_to(MovementStatus.in_ground);
            }
        }

        private void big_slow_hit_ground(float road_height)
        {
            Road_Info_Helper.try_get_slope(position.x, out var slope);
            var ground_slope = new Vector2(1f, slope);
            var ground_normal = new Vector2(-slope, 1f);

            var v_landing_parall = Vector2.Dot(velocity, ground_slope) * ground_slope;
            var v_landing_vertical = Vector2.Dot(velocity, ground_normal) * ground_normal;

            v_landing_parall *= 0.8f;
            v_landing_vertical *= -0.8f;
            rot_speed *= 0.6f;

            var ek = mass * Mathf.Pow(velocity.magnitude, 2);
            var vtm = v_landing_vertical.magnitude;
            var vpm = v_landing_parall.magnitude;

            velocity = v_landing_vertical + v_landing_parall;
            position = new Vector2(position.x, road_height);
            pmgr.rebound_event?.Invoke(this);
            if (velocity.magnitude > desc.vfx_v_threshold && Vector2.SqrMagnitude(position - WorldContext.instance.caravan_pos) < 400f)
            {
                if (desc.vfx_on_hit_ground != null)
                    Vfx_Helper.InstantiateVfx(desc.vfx_on_hit_ground, 240, position);
                if (desc.se_on_hit_ground != null)
                    Audio.AudioSystem.instance.PlayOneShot(desc.se_on_hit_ground);
            }
        }

        private void arrow_hit_enemy(ITarget target_hit, bool can_stick_in)
        {
            //1.对目标造成伤害与击退
            var ad = modify_attack_data(target_hit);

            /*            if (emitter is Device d)
                            BattleContext.instance.ChangeDmg(d, target_hit.hurt(ad));
                        else
                            target_hit.hurt(ad);*/

            foreach (var attack_data in ad)
            {
                target_hit.hurt(this, attack_data, out var dmg_data);
                target_hit.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);

                if (emitter is Device device)
                    BattleContext.instance.ChangeDmg(device, dmg_data.dmg);
            }

            if (emitter is Device d)
            {
                if (target_hit.hp <= 0)
                {
                    d.kill_enemy_action?.Invoke(target_hit);
                    pmgr.kill_enemy_event?.Invoke(this);
                }
            }

            target_hit.impact(WorldEnum.impact_source_type.projectile, velocity, mass, Config.current.arrow_penetration_loss);

            //2.根据剩余动能，判定飞射物自身的后续运动方式
            last_hit = target_hit;
            var ek_mul_2 = mass * Mathf.Pow(velocity.magnitude, 2);
            var delta_ek = ek_mul_2 - Config.current.arrow_penetration_loss * target_hit.Mass;

            if (delta_ek > 0)
            {
                velocity *= Mathf.Sqrt(delta_ek / mass) / velocity.magnitude;
                direction = velocity.normalized;
            }
            else if (can_stick_in)
            {
                in_object_ticks = MAX_IN_OBJECT_TICKS;
                in_target = target_hit;
                movement_status_change_to(MovementStatus.in_object);

                //规则：附加自身在目标上的重量
                target_hit.attach_data(mass);
            }
            else
            {
                velocity = Vector2.zero;
                direction = velocity.normalized;
            }

            hit_target_event?.Invoke(target_hit);
        }

        private void bullet_hit_enemy(ITarget target_hit)
        {
            var ad = modify_attack_data(target_hit);

            foreach (var attack_data in ad)
            {
                target_hit.hurt(this, attack_data, out var dmg_data);
                target_hit.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);

                if (emitter is Device device)
                    BattleContext.instance.ChangeDmg(device, dmg_data.dmg);
            }

            if (emitter is Device d)
            {
                if (target_hit.hp <= 0)
                {
                    d.kill_enemy_action?.Invoke(target_hit);
                    pmgr.kill_enemy_event?.Invoke(this);
                }
            }

            target_hit.impact(WorldEnum.impact_source_type.projectile, velocity, mass, Config.current.bullet_penetration_loss);

            //2.根据剩余动能，判定飞射物自身的后续运动方式
            last_hit = target_hit;
            var ek_mul_2 = mass * Mathf.Pow(velocity.magnitude, 2);
            var delta_ek = ek_mul_2 - Config.current.bullet_penetration_loss * target_hit.Mass;
            if (delta_ek > 0)
            {
                velocity *= Mathf.Sqrt(delta_ek / mass) / velocity.magnitude;
                direction = velocity.normalized;
            }
            else
            {
                if (mass > target_hit.Mass * 0.5)
                {
                    velocity *= Config.current.bullet_enemy_bounce_coef;
                    direction = UnityEngine.Random.rotation * velocity;
                }
                else
                {
                    validate = false;
                }
            }

            hit_target_event?.Invoke(target_hit);
        }

        private void destroy_hit_enemy(ITarget target_hit)
        {
            var ad = modify_attack_data(target_hit);

            foreach (var attack_data in ad)
            {
                target_hit.hurt(this, attack_data, out var dmg_data);
                target_hit.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);

                if (emitter is Device device)
                    BattleContext.instance.ChangeDmg(device, dmg_data.dmg);
            }

            if (emitter is Device d)
            {
                if (target_hit.hp <= 0)
                {
                    d.kill_enemy_action?.Invoke(target_hit);
                    pmgr.kill_enemy_event?.Invoke(this);
                }
            }

            target_hit.impact(WorldEnum.impact_source_type.projectile, velocity, mass, Config.current.bullet_penetration_loss);
            hit_target_event?.Invoke(target_hit);

            validate = false;
        }

        private void hit_enemy(ITarget target_hit)
        {
            switch (desc.hit_monster_rule)
            {
                case "Arrow":
                    arrow_hit_enemy(target_hit, true);
                    break;
                case "Huge_Arrow":
                    arrow_hit_enemy(target_hit, false);
                    break;
                case "Bullet":
                    bullet_hit_enemy(target_hit);
                    break;
                case "Destroy":
                    destroy_hit_enemy(target_hit);
                    break;
                default:
                    break;
            }
        }



        public virtual void RemoveSelf()
        {
            validate = false;
        }
        public virtual void ResetPos()
        {
            position -= new Vector2(WorldContext.instance.reset_dis, 0);
        }

        public virtual void ResetProjectile(Vector2 vel, Vector2 dir, Faction f, MovementStatus movement_status)
        {
            life_ticks = life_ticks_init;
            velocity = vel;
            direction = dir;
            faction = f;
            this.movement_status = movement_status;
        }

        #region ITargetFunc
        void ITarget.impact(params object[] prms)
        {
            var impact_source_type = (WorldEnum.impact_source_type)prms[0];

            //1.计算击退方向dir
            Vector2 dir = new();

            switch (impact_source_type)
            {

                case WorldEnum.impact_source_type.melee:
                    var pos_atk = (Vector2)prms[1];
                    var pos_def = (Vector2)prms[2];
                    var dir_a2d = pos_def - pos_atk;
                    dir = dir_a2d.normalized;
                    break;
            }

            //2.获取本次攻击的冲量ft
            //temp:冲量
            float ft;
            switch (impact_source_type)
            {

                case WorldEnum.impact_source_type.melee:
                    ft = (float)prms[3];
                    ft += ft * 1 * 0.5f;
                    break;

                default:
                    ft = 0;
                    break;
            }

            //5.使子弹被击退
            velocity += ft / mass * dir;



        }


        void ITarget.heal(Dmg_Data dmg_data)
        {

        }


        void ITarget.attach_data(params object[] prms)
        {

        }
        void ITarget.detach_data(params object[] prms)
        {

        }


        void ITarget.do_when_hurt_target(ref Attack_Data attack_data)
        {
            emitter.do_when_hurt_target(ref attack_data);
        }


        void ITarget.hurt(ITarget harm_source, Attack_Data attack_data, out Dmg_Data dmg_data)
        {
            harm_source?.do_when_hurt_target(ref attack_data);
            dmg_data = default;
        }
        float ITarget.distance(ITarget target)
        {
            return Vector2.Distance(position, target.Position);
        }
        void ITarget.tick_handle(System.Action<ITarget> outter_request, params object[] prms)
        {
            m_tick_handle += outter_request;
        }

        void ITarget.applied_outlier(ITarget source, string outlier_type, int value)
        {

        }
        #endregion
    }
}
