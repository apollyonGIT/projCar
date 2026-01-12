using Commons;
using System;
using System.Collections.Generic;
using UnityEngine;
using World.Caravans;
using World.Devices;
using World.Projectiles;
using static World.WorldEnum;

namespace World
{
    public static class BattleUtility
    {
        public static Collider2D[] results = new Collider2D[256];


        /// <summary>
        /// 按距离中心的距离排序
        /// </summary>
        /// <param name="turn"></param>
        /// <param name="center"></param>
        private static void sort_results(int turn, Vector2 center)
        {

            Array.Sort(results, (c1, c2) =>
            {
                if (c1 == null && c2 == null)
                {
                    return 0;
                }
                if (c1 == null)
                {
                    return 1;
                }
                if (c2 == null)
                {
                    return -1;
                }
                Vector2 pos_i = c1.transform.position;
                Vector2 pos_j = c2.transform.position;
                var dis_i = (pos_i - center).magnitude;
                var dis_j = (pos_j - center).magnitude;
                // return (int)(dis_i - dis_j);          不用这个 0.7 = 0

                var result = 0;
                if (dis_i < dis_j)
                {
                    result = -1;
                }
                else if (dis_i > dis_j)
                {
                    result = 1;
                }
                return result;
            });
        }

        /// <summary>
        /// 按距离中心当前朝向夹角排序
        /// </summary>
        /// <param name="turn"></param>
        /// <param name="center"></param>
        /// <param name="direction"></param>
        private static void sort_results_with_angle(int turn, Vector2 center, Vector2 direction)
        {
            Array.Sort(results, (c1, c2) =>
            {
                if (c1 == null && c2 == null)
                {
                    return 0;
                }
                if (c1 == null)
                {
                    return 1;
                }
                if (c2 == null)
                {
                    return -1;
                }
                Vector2 pos_i = c1.transform.position;
                Vector2 pos_j = c2.transform.position;
                var dir_i = (pos_i - center);
                var dir_j = (pos_j - center);

                var a_i = Vector2.Angle(dir_i, direction);
                var a_j = Vector2.Angle(dir_j, direction);
                var result = 0;
                if (a_i < a_j)
                {
                    result = -1;
                }
                else if (a_i > a_j)
                {
                    result = 1;
                }
                return result;
            });
        }

        public static ITarget select_projectile_in_circle(Vector2 center, float radius, Faction faction, Func<ITarget, bool> func = null)
        {
            var turn = Physics2D.OverlapCircleNonAlloc(center, radius, results);
            sort_results(turn, center);
            for (int i = 0; i < turn; i++)
            {
                if (results[i].TryGetComponent<Projectile>(out var projectile))
                {
                    if (projectile.faction == faction) continue;
                    if (func != null && func.Invoke(projectile) == false)
                    {
                        continue;
                    }
                    if (projectile.validate == false)
                        continue;
                    return projectile;
                }
            }
            return null;
        }


        /// <summary>
        /// 返回在指定中心范围内最近的不同阵营且满足筛选条件的ITarget
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="faction"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static ITarget select_target_in_circle(Vector2 center, float radius, Faction faction, Func<ITarget, bool> func = null)
        {
            var turn = Physics2D.OverlapCircleNonAlloc(center, radius, results);

            if (turn == 0)
                return null;

            sort_results(turn, center);

            if (faction == Faction.player)
            {
                for (int i = 0; i < turn; i++)
                {
                    if (results[i].TryGetComponent<Enemys.EnemyHitbox>(out var enemyHitBox))
                    {
                        var owner = enemyHitBox.view.owner;
                        if (func != null && func.Invoke(owner) == false)
                        {
                            continue;
                        }

                        if (owner.hp <= 0)
                            continue;

                        if (owner.is_ban_select)
                            continue;

                        return owner;
                    }
                }
            }
            else if (faction == Faction.opposite)
            {
                for (int i = 0; i < turn; i++)
                {
                    if (results[i].TryGetComponent<Devices.DeviceHitBox>(out var deviceHitBox))
                    {
                        var dv = deviceHitBox.view;
                        if (func != null && func.Invoke(dv.owner) == false)
                        {
                            continue;
                        }
                        if (dv.owner.current_hp <= 0)
                            continue;
                        return dv.owner;
                    }

                    if (results[i].TryGetComponent<CaravanHitBox>(out var caravanHitBox))
                    {
                        var cv = caravanHitBox.view;
                        if (func != null && func.Invoke(cv.owner) == false)
                        {
                            continue;
                        }
                        return cv.owner;
                    }
                }
            }
            return null;
        }
        public static ITarget select_target_in_circle_min_angle(Vector2 center, Vector2 direction, float radius, Faction faction, Func<ITarget, bool> func = null)
        {
            var turn = Physics2D.OverlapCircleNonAlloc(center, radius, results);

            if (turn == 0)
                return null;

            sort_results_with_angle(turn, center, direction);


            if (faction == Faction.player)
            {
                for (int i = 0; i < turn; i++)
                {
                    if (results[i].TryGetComponent<Enemys.EnemyHitbox>(out var enemyHitBox))
                    {
                        var owner = enemyHitBox.view.owner;
                        if (func != null && func.Invoke(owner) == false)
                        {
                            continue;
                        }

                        if (owner.hp <= 0)
                            continue;
                        if (owner.is_ban_select)
                            continue;

                        return owner;
                    }
                }
            }
            else if (faction == Faction.opposite)
            {
                for (int i = 0; i < turn; i++)
                {
                    if (results[i].TryGetComponent<Devices.DeviceHitBox>(out var deviceHitBox))
                    {
                        var dv = deviceHitBox.view;
                        if (func != null && func.Invoke(dv.owner) == false)
                        {
                            continue;
                        }
                        if (dv.owner.current_hp <= 0)
                            continue;

                        //规则：不索同阵营
                        var _owner = (ITarget)dv.owner;
                        if (_owner.Faction == Faction.opposite) continue;

                        return dv.owner;
                    }

                    if (results[i].TryGetComponent<CaravanHitBox>(out var caravanHitBox))
                    {
                        var cv = caravanHitBox.view;
                        if (func != null && func.Invoke(cv.owner) == false)
                        {
                            continue;
                        }

                        //规则：不索同阵营
                        var _owner = (ITarget)cv.owner;
                        if (_owner.Faction == Faction.opposite) continue;

                        return cv.owner;
                    }
                }
            }
            return null;
        }

        public static List<ITarget> select_all_target_in_circle(Vector2 center, float radius, Faction faction, Func<ITarget, bool> func = null)
        {
            List<ITarget> targets = new();

            var turn = Physics2D.OverlapCircleNonAlloc(center, radius, results);

            if (turn == 0)
                return targets;

            sort_results(turn, center);

            if (faction == Faction.player)
            {
                for (int i = 0; i < turn; i++)
                {
                    if (results[i].TryGetComponent<Enemys.EnemyHitbox>(out var enemyHitBox))
                    {
                        var owner = enemyHitBox.view.owner;
                        if (func != null && func.Invoke(owner) == false)
                        {
                            continue;
                        }

                        if (owner.hp <= 0)
                            continue;
                        if (owner.is_ban_select)
                            continue;

                        targets.Add(owner);
                    }
                }
            }
            else if (faction == Faction.opposite)
            {
                for (int i = 0; i < turn; i++)
                {
                    if (results[i].TryGetComponent<Devices.DeviceHitBox>(out var deviceHitBox))
                    {
                        var dv = deviceHitBox.view;
                        if (func != null && func.Invoke(dv.owner) == false)
                        {
                            continue;
                        }
                        if (dv.owner.current_hp <= 0)
                            continue;

                        if (dv.owner.faction == Faction.player)
                            targets.Add(dv.owner);
                    }

                    if (results[i].TryGetComponent<CaravanHitBox>(out var caravanHitBox))
                    {
                        var cv = caravanHitBox.view;
                        if (func != null && func.Invoke(cv.owner) == false)
                        {
                            continue;
                        }

                        if (cv.owner.faction == Faction.player)
                            targets.Add(cv.owner);
                    }
                }
            }
            return targets;
        }

        
        public static List<ITarget> select_all_target_in_rect(Vector2 p1, Vector2 p2, Faction faction, Func<ITarget, bool> func = null)
        {
            var pp1 = new Vector3(p1.x, p1.y, 10);
            var pp2 = new Vector3(p2.x, p2.y, 10);
            Debug.DrawLine(pp1, pp2, Color.cyan, 3f);

            List<ITarget> targets = new();

            var turn = Physics2D.OverlapAreaNonAlloc(p1, p2, results);
            if (turn != 0)
            {
                if (faction == Faction.player)
                {
                    for (int i = 0; i < turn; i++)
                    {
                        if (results[i].TryGetComponent<Enemys.EnemyHitbox>(out var enemyHitBox))
                        {
                            var owner = enemyHitBox.view.owner;
                            if (func != null && func.Invoke(owner) == false)
                            {
                                continue;
                            }
                            if (owner.hp <= 0)
                                continue;
                            if (owner.is_ban_select)
                                continue;
                            targets.Add(owner);
                        }
                    }
                }
                else if (faction == Faction.opposite)
                {
                    for (int i = 0; i < turn; i++)
                    {
                        if (results[i].TryGetComponent<Devices.DeviceHitBox>(out var deviceHitBox))
                        {
                            var dv = deviceHitBox.view;
                            if (func != null && func.Invoke(dv.owner) == false)
                            {
                                continue;
                            }
                            if (dv.owner.current_hp <= 0)
                                continue;
                            targets.Add(dv.owner);
                        }

                        if (results[i].TryGetComponent<CaravanHitBox>(out var caravanHitBox))
                        {
                            var cv = caravanHitBox.view;
                            if (func != null && func.Invoke(cv.owner) == false)
                            {
                                continue;
                            }
                            targets.Add(cv.owner);
                        }
                    }
                }
            }

            return targets;
        }


        /// <summary>
        /// 判断限定角速度下的二维向量旋转
        /// </summary>
        /// <param name="current_v"></param>
        /// <param name="expected_v"></param>
        /// <param name="rotate_speed"></param>
        /// <returns></returns>
        public static Vector2 rotate_v2(Vector2 current_v, Vector2 expected_v, float rotate_speed)
        {
            var delta_deg = Vector2.SignedAngle(current_v, expected_v);
            if (Mathf.Abs(delta_deg) > rotate_speed)
            {
                return Quaternion.AngleAxis(rotate_speed * (delta_deg > 0 ? 1 : -1), Vector3.forward) * current_v;
            }
            return expected_v;
        }

        public static Projectile GetProjectile(string ammo_type)
        {
            var p = new Projectile();
            switch (ammo_type)
            {
                case "Special":
                    p = new SpecialProjectile();
                    break;
                default:

                    break;
            }

            return p;
        }

        public static float GetDeviceInitAngleBySlot(Device device,string slot_name)
        {
            switch (slot_name)
            {
                case "slot_back":
                    return device.desc.init_angle_back;
                case "slot_back_top":
                    return device.desc.init_angle_back_top;
                case "slot_front":
                    return device.desc.init_angle_front;
                case "slot_front_top":
                    return device.desc.init_angle_front_top;
                case "slot_top":
                    return device.desc.init_angle_top;
            }

            return 0;
        }

        public static bool query_field<T>(ITarget target, string field_name, out T value)
        {
            value = default;
            if (target == null || field_name == null) return false;

            var fi = target.GetType().GetField(field_name);
            if (fi == null) return false;

            var t = fi.GetValue(target);
            if (t is not T) return false;

            value = (T)t;
            return true;
        }

        public static Vector2 get_target_colllider_pos(ITarget target)
        {
            if (target == null)
                return Vector2.zero;
            if (query_field(target, "collider_pos", out Vector2 target_collider_offset))
            {
                return target.Position + target_collider_offset;
            }
            return target.Position;
        }

        public static Vector2 get_v2_to_target_collider_pos(ITarget target, Vector2 start_position)
        {
            return get_target_colllider_pos(target) - start_position;
        }

        public static bool check_pos_in_screen(Vector3 pos)
        {
            var cam_pos = WorldSceneRoot.instance.mainCamera.transform.position;
            var obj_dis = (Config.current.desiredResolution.x / (float)Config.current.pixelPerUnit) * (1 + Mathf.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / pos.z);
            var obj_wid = (Config.current.desiredResolution.y / (float)Config.current.pixelPerUnit) * (1 + Mathf.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / pos.z);

            if (pos.x > cam_pos.x - obj_dis / 2 && pos.x < cam_pos.x + obj_dis / 2 && pos.y < cam_pos.y + obj_wid / 2)
                return true;
            return false;
        }

        public static int slot_2_order_in_layer(string slot_name)
        {
            switch (slot_name)
            {
                case "device_slot_wheel":
                    return 100;
                case "slot_top":
                    return 200;
                case "slot_back":
                    return 500;
                case "slot_back_top":
                    return 300;
                case "slot_front":
                    return 600;
                case "slot_front_top":
                    return 400;
            }
            return 0;
        }
    }


    public interface ITarget
    {
        Vector2 Position { get; }
        Vector2 velocity { get; }
        Vector2 direction { get; }
        Faction Faction { get; }

        float Mass { get; }

        int hp { get; }
        int hp_max { get; }

        Vector2 acc_attacher { get; set; }

        bool is_interactive { get; }

        float distance(ITarget target);

        void hurt(ITarget harm_source, Attack_Data attack_data, out Dmg_Data dmg_data);

        void impact(params object[] prms);

        void heal(Dmg_Data dmg_data);

        void applied_outlier(ITarget source,string outlier_type, int value);

        void do_when_hurt_target(ref Attack_Data attack_data);

        void attach_data(params object[] prms);

        void detach_data(params object[] prms);

        void tick_handle(Action<ITarget> outter_request, params object[] prms);

        Dictionary<string, LinkedList<Action<ITarget>>> buff_flags { get; set; }
        List<string> fini_buffs { get; set; }

        event Action<ITarget> fini_callback_event;

        float def_cut_rate { get; set; }
    }


    public interface IAttack_Device
    {
        int ind_atk_pts { get;} //伤害百分比
        int ind_atk_add { get;} //伤害附加值
        int ind_armor_piercing { get;} //破甲附加值
        int ind_critical_chance { get;} //暴击几率附加值
        int ind_critical_dmg_rate { get;} //暴击倍率附加值
    }


    public struct Attack_Data
    {
        public int atk;
        public int armor_piercing;
        public int critical_chance;
        public int critical_dmg_rate;
        
        public float _adm; 
        public float adm => _adm == 0f ? 1f : _adm / 100f; //规则：adm意为 attacker_dmg_mod，如无特殊说明，这个属性的值为100%

        public int ts; //命中

        public string diy_atk_str;

        public bool is_miss; //绝对落空

        //==================================================================================================

        public void calc_device_coef(IAttack_Device idevice)
        {
            var itarget = (ITarget)idevice;
            if (itarget.Faction == Faction.opposite) return;

            var bctx = BattleContext.instance;

            //规则：攻击力 = 基础攻击力 * ( 1 + 全局百分比修正 + 独特百分比修正 ) + 全局固定值修正 + 独特固定值修正
            atk = Mathf.FloorToInt(atk * (1 + bctx.global_atk_pts / 1000f + idevice.ind_atk_pts / 1000f) + bctx.global_atk_add + idevice.ind_atk_add);
            atk = Mathf.Max(atk, 0);

            //规则：破甲 = 基础破甲 + 全局修正 + 独特修正
            armor_piercing = armor_piercing + bctx.global_armor_piercing + idevice.ind_armor_piercing;

            //规则：暴击几率 = 基础暴击几率 + 全局修正 + 独特修正
            critical_chance = critical_chance + bctx.global_critical_chance + idevice.ind_critical_chance;

            //规则：暴击倍率 = 基础暴击倍率 + 全局修正 + 独特修正
            critical_dmg_rate = critical_dmg_rate + bctx.global_critical_dmg_rate + idevice.ind_critical_dmg_rate;
        }
    }


    public struct Defense_Data
    {
        public int def;

        public int dodge; //闪避

        public float fire_value;

        public float fixed_def_cut_rate;

        public float k_hurt;
    }


    public struct Dmg_Data
    {
        public int dmg;
        public bool is_critical;
        public bool is_miss;
        public bool is_immune;
        
        public bool is_heal;
        public float heal_power;

        public string diy_dmg_str;
    }
}
