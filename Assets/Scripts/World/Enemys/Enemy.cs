using Addrs;
using Commons;
using Foundations;
using Foundations.MVVM;
using Foundations.Tickers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using World.Enemys.BT;
using World.Helpers;
using World.VFXs.Damage_PopUps;
using World.VFXs.Enemy_Death_VFXs;
using static World.WorldEnum;

namespace World.Enemys
{
    public interface IEnemy_BT
    {
        void init(Enemy cell, params object[] prms);

        void tick(Enemy cell);

        void notify_on_enter_die(Enemy cell);

        void notify_on_dying(Enemy cell);

        void notify_on_dead(Enemy cell);

        string state { get; }
    }


    public interface IEnemyView : IModelView<Enemy>
    {
        void notify_on_tick1();

        void notify_on_hurt();

        void notify_on_pre_aim(bool ret);

        void notify_on_aim(bool ret);

        void notify_on_outrange_aim(bool ret);

        void notify_on_change_color(Color color);
    }


    public class Enemy : Model<Enemy, IEnemyView>, ITarget
    {
        public string GUID;

        public Vector2 pos;
        public Vector2 dir;

        public Vector2 view_pos => calc_view_pos();
        public Quaternion view_rotation => calc_view_rotation();
        public float view_scaleX => calc_view_scaleX();
        public float view_scaleY => calc_view_scaleY();
        public bool is_ban_scaleY_slip;

        public float view_pic_scaleX => calc_view_scaleY();

        public Vector2 velocity;
        public Vector2 acc_attacher;

        public EnemyMgr mgr;
        public EnemyMover mover;
        public IEnemy_BT old_bt;
        public Enemy_BT bt;

        public AutoCodes.monster _desc;
        public object _sub_desc;

        public string view_resource_name;

        public float base_speed;

        public float speed_expt;
        public float old_speed_expt;
        public bool is_speed_expt_change;

        public Vector2 position_expt;

        public float mass_self;
        public float mass_attachment;
        public float mass_total => (mass_self + mass_attachment) * battle_ctx.enemy_mass_factor;

        public int hp;
        public int hp_max;

        public bool is_alive = true;

        public bool is_fling_off = false;

        public Vector3 collider_pos;

        public ITarget target;

        string m_death_vfx => _desc.death_vfx[death_vfx_index];
        public int death_vfx_index;

        public Dictionary<string, Spine.Bone> bones = new();

        public Dictionary<string, object> anim_info => calc_anim_info();

        Action<ITarget> m_tick_handle;

        public AutoCodes.mg_scene _group_desc;
        public Dictionary<string, AutoCodes.spine_monster> _spine_descs = new();

        Vector2 ITarget.Position => collider_pos;

        Faction ITarget.Faction => Faction.opposite;

        float ITarget.Mass => mass_total;

        bool ITarget.is_interactive => is_alive;

        int ITarget.hp => hp;
        int ITarget.hp_max => hp_max;

        Vector2 ITarget.acc_attacher { get => acc_attacher; set => acc_attacher = value; }

        Vector2 ITarget.direction => dir;

        Vector2 ITarget.velocity => velocity;

        public bool is_init;

        public BattleContext battle_ctx;

        public Action<object[]> outter_tick;
        public Action<object[]> outter_fini;

        public bool is_drop_relic;
        public uint drop_relic_id;

        Dictionary<string, LinkedList<Action<ITarget>>> ITarget.buff_flags { get => m_buff_flags; set => m_buff_flags = value; }
        Dictionary<string, LinkedList<Action<ITarget>>> m_buff_flags = new();

        List<string> ITarget.fini_buffs { get => m_fini_buffs; set => m_fini_buffs = value; }
        List<string> m_fini_buffs = new();

        public event Action<ITarget> fini_callback_event;

        public bool is_use_bt = true;
        public bool is_unconsious => is_use_bt; //是否失能

        public float hit_rate = 1; //命中率，0-1

        public Vector2 velocity_spectre; //漂浮移速

        public ITarget last_harm_source;

        public LinkedList<string> buff_list = new();

        float ITarget.def_cut_rate { get => m_def_cut_rate; set => m_def_cut_rate = value; }
        float m_def_cut_rate;

        public bool is_elite_group;
        public bool is_ban_drop; //禁止掉落物品

        public bool is_ban_select; //禁止选中

        public bool is_first_hurt; //是否首次受伤

        public bool is_pursuing; //是否为追兵

        //==================================================================================================

        public Enemy(uint id)
        {
            GUID = Guid.NewGuid().ToString();

            battle_ctx = BattleContext.instance;

            Mission.instance.try_get_mgr("EnemyMgr", out mgr);
            AutoCodes.monsters.TryGetValue($"{id}", out _desc);

            mover = new(this);

            var monster_behaviour_tree = _desc.monster_behaviour_tree;
            if (!string.IsNullOrEmpty(monster_behaviour_tree))
            {
                var obj = Activator.CreateInstance(Assembly.Load("World").GetType($"World.Enemys.BT.{monster_behaviour_tree}"));

                if (obj is IEnemy_BT _old_bt)
                    old_bt = _old_bt;
                if (obj is Enemy_BT _bt)
                    bt = _bt;
            }

            mass_self = _desc.mass;
            
            //规则：怪物血量随场景变化，向上取整
            hp_max = Mathf.CeilToInt(_desc.hp * WorldContext.instance.r_scene.enemy_hp_mod / 1000f);
            hp = hp_max;

            view_resource_name = _desc.monster_view;

            base_speed = _desc.base_speed;

            _spine_descs = AutoCodes.spine_monsters.records.Where(t => t.Key.Contains($"{_desc.id}")).ToDictionary(t=> t.Key, v=> v.Value);

            #region 加载子表
            var sub_table_name = _desc.sub_table_name;
            if (!string.IsNullOrEmpty(sub_table_name))
            {
                var sub_table_key = calc_sub_table_key();
                var sub_table_type = Assembly.Load("AutoCodes").GetType($"AutoCodes.{sub_table_name}s");
                var sub_table_agrs = new object[] { sub_table_key, _sub_desc };

                sub_table_type.GetMethod("TryGetValue").Invoke(null, sub_table_agrs);
                _sub_desc = sub_table_agrs[1];
            }
            #endregion
        }


        protected virtual string calc_sub_table_key()
        {
            return _desc.sub_table_key;
        }


        public virtual void do_after_init(params object[] args)
        {
            //规则：精英怪加载特殊血牌
            var go = (GameObject)args[0];
            init_hp_ui(go);

            //临时规则：结算，每生成一名敌人，需将其血量加进该数值中
            var ds = Share_DS.instance;
            ds.try_get_value(Game_Mgr.key_total_enemy_hp, out float total_enemy_hp);
            total_enemy_hp += hp_max;
            Share_DS.instance.add(Game_Mgr.key_total_enemy_hp, total_enemy_hp);
        }


        public virtual void tick()
        {
            if (!is_init) return;

            //甩脱检测
            var dis = mgr.ctx.caravan_pos.x - pos.x;
            if (!mgr.ctx.is_need_reset && dis >= Config.current.fling_off_dis)
            {
                is_fling_off = true;
                hp = -1;
            }

            //空血检测
            if (hp <= 0)
            {
                if (is_alive)
                {
                    is_alive = false;
                    old_bt?.notify_on_enter_die(this);
                    bt?.notify_on_enter_die(this);

                    //规则：非甩脱，怪物死亡增加击杀压力
                    if (!is_fling_off && _group_desc != null)
                    {
                        mgr.ctx.pressure += _group_desc.kill_pressure;
                    }

                    //规则：怪物死亡增加击杀分数
                    mgr.ctx.kill_score++;
                }
                else 
                {
                    old_bt?.notify_on_dying(this);
                    bt?.notify_on_dying(this);
                }

                return;
            }

            if (is_use_bt)
            {
                old_bt?.tick(this);
                bt?.tick(this);
            }
            else
            {
                Mover.EnemyMover_Fly.unconsious(this);
            }

            //临时：计算外部添加buff
            foreach (var (buff_name, buff_ac_list) in m_buff_flags)
            {
                foreach (var buff_ac in buff_ac_list)
                {
                    buff_ac?.Invoke(this);
                }
            }

            //临时：删除buff
            if (m_fini_buffs.Any())
            {
                foreach (var fini_buff_name in m_fini_buffs)
                {
                    if (!m_buff_flags.TryGetValue(fini_buff_name, out var target_buff_list)) continue;

                    target_buff_list.RemoveFirst();

                    if (!target_buff_list.Any()) 
                        m_buff_flags.Remove(fini_buff_name);
                }

                m_fini_buffs.Clear();
            }

            m_tick_handle?.Invoke(this);
            m_tick_handle = null;

            outter_tick?.Invoke(new object[] { this });
        }


        public virtual void tick1()
        {
            foreach (var view in views)
            {
                view.notify_on_tick1();
            }
        }


        public virtual void fini()
        {
            mgr.fini_cells.AddLast(this);

            outter_fini?.Invoke(new object[] { this });

            //删除所有内置任务
            Request_Helper.del_requests(GUID);

            //创建死亡特效
            if (m_death_vfx != "null" && Mission.instance.try_get_mgr("Enemy_Death_VFXMgr", out Enemy_Death_VFXMgr enemy_Death_VFXMgr))
                enemy_Death_VFXMgr.pd.create_cell(m_death_vfx, pos, 240);

            Helpers.Enemy_Fini_Helper.@do(this);

            fini_callback_event?.Invoke(this);
        }


        protected virtual Vector3 calc_view_pos()
        {
            return pos;
        }


        protected virtual Quaternion calc_view_rotation()
        {
            return EX_Utility.look_rotation_from_left(dir);
        }


        protected virtual float calc_view_scaleX()
        {
            return 1;
        }


        protected virtual float calc_view_scaleY()
        {
            if (is_ban_scaleY_slip) return 1;

            return dir.x >= 0 ? 1 : -1;
        }


        protected virtual Dictionary<string, object> calc_anim_info()
        {
            if (is_use_bt)
            {
                var state = old_bt != null ? old_bt.state : bt.state;

                if (!_spine_descs.TryGetValue($"{_desc.id},{state}", out var r_spine))
                    return null;

                return new()
                {
                    { "anim_name", r_spine.anim_name },
                    { "loop", r_spine.loop }
                };
            }
            else
            {
                if (!_spine_descs.TryGetValue($"{_desc.id},Default", out var r_spine))
                    return null;

                var anim_name = r_spine.unconsious_anim_name ?? r_spine.anim_name;
                return new()
                {
                    { "anim_name", anim_name },
                    { "loop", true }
                };
            }
        }


        void ITarget.impact(params object[] prms)
        {
            mover.impact(prms);
        }


        void ITarget.heal(Dmg_Data dmg_data)
        {
            hp += Mathf.RoundToInt(dmg_data.heal_power);

            if (dmg_data.heal_power <= 0) return;
            //    Debug.Log($"【{_desc.id}】{dmg_data.heal_power}");

            Damage_PopUp.instance.create_cell(dmg_data, pos);
        }


        public virtual void do_when_hurt_target(ref Attack_Data attack_data)
        { 
        }


        void ITarget.do_when_hurt_target(ref Attack_Data attack_data)
        {
            do_when_hurt_target(ref attack_data);
        }


        void ITarget.hurt(ITarget harm_source, Attack_Data attack_data, out Dmg_Data dmg_data)
        {
            is_first_hurt = true;

            harm_source?.do_when_hurt_target(ref attack_data);

            dmg_data = default;
            if (hp <= 0) return;

            dmg_data = calc_dmg(attack_data);
            var dmg = dmg_data.dmg;

            hp -= dmg;

            Damage_PopUp.instance.create_cell(dmg_data, pos);

            last_harm_source = harm_source;
            Request_Helper.broadcast_requests(GUID, "is_hurt", true);

            foreach (var view in views)
            {
                view.notify_on_hurt();
            }

            //临时规则：结算，记录玩家伤害量
            var ds = Share_DS.instance;
            if (!is_fling_off)
            {
                ds.try_get_value(Game_Mgr.key_make_damage_count, out int make_damage_count);
                make_damage_count += dmg;
                ds.add(Game_Mgr.key_make_damage_count, make_damage_count);
            }
        }


        protected virtual Dmg_Data calc_dmg(Attack_Data attack_data)
        {
            return Hurt_Calc_Helper.dmg_to_enemy(attack_data, this);
        }


        void ITarget.attach_data(params object[] prms)
        {
            var addition_mass = (float)prms[0];
            mass_attachment += addition_mass;
        }


        void ITarget.detach_data(params object[] prms)
        {
            var addition_mass = (float)prms[0];
            mass_attachment -= addition_mass;
        }


        float ITarget.distance(ITarget target)
        {
            return Vector2.Distance(pos, target.Position);
        }


        void ITarget.tick_handle(Action<ITarget> outter_request, params object[] prms)
        {
            m_tick_handle += outter_request;
        }


        public bool try_get_bone_pos(string bone_name, out Vector2 _pos)
        {
            _pos = default;

            if (!bones.TryGetValue(bone_name, out var bone))
            {
                Debug.LogError("怪物射击类，怪物骨骼未找到");
                return false;
            }

            bones.TryGetValue("root", out var root_bone);
            var offset = new Vector2(bone.WorldX, bone.WorldY) - new Vector2(root_bone.WorldX, root_bone.WorldY);

            if (dir.x > 0)
                _pos = pos + offset;
            else
                _pos = pos - offset;

            return true;
        }

        public void PreSelect(bool ret)
        {
            foreach(var view in views)
            {
                view.notify_on_pre_aim(ret);
            }
        }

        public void Select(bool ret)
        {
            foreach (var view in views)
            {
                view.notify_on_aim(ret);
            }
        }

        public void OutRangeSelect(bool ret)
        {
            foreach (var view in views)
            {
                view.notify_on_outrange_aim(ret);
            }
        }


        public void change_color(Color color)
        {
            foreach (var view in views)
            {
                view.notify_on_change_color(color);
            }
        }


        public virtual void init_hp_ui(GameObject go)
        {
            //精英血牌
            if (_desc.is_elite)
            {
                var hp_clip_count = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(this, "HP_CLIP_COUNT"));
                var sub_hp = Mathf.RoundToInt(hp_max / (float)hp_clip_count);

                Addressable_Utility.try_load_asset("enemy_elite_hp", out Enemy_UIs.Enemy_Elite_HP_View elite_hp_model);
                var elite_hp = UnityEngine.Object.Instantiate(elite_hp_model, go.transform.parent);
                elite_hp.init(this, hp_clip_count, sub_hp);
            }
        }


        void ITarget.applied_outlier(ITarget source, string outlier_type, int value)
        {
            
        }
    }
}





