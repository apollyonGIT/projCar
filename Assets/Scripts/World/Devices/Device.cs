using AutoCodes;
using Foundations.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Devices.DeviceUpgrades;
using World.Helpers;
using static World.WorldEnum;
using World.Devices.DeviceEmergencies;
using World.Enemys;
using World.Cubicles;
using Commons;

namespace World.Devices
{
    public interface IDeviceView : IModelView<Device>
    {
        void init();
        void init_pos();
        void notify_change_anim(string anim_name, bool loop);
        void notify_change_anim(string anim_name, bool loop, float percent);
        void notify_trigger_event(params object[] args);
        void notify_on_tick();
        void notify_open_collider(string _name, Action<ITarget> t1 = null, Action<ITarget> t2 = null, Action<ITarget> t3 = null);
        void notify_close_collider(string _name);
        void notify_change_anim_speed(float f);
        void notify_hurt(int dmg);
        void notify_fix(int delta);
        void notify_disable();
        void notify_enable();
        void notify_player_oper(bool oper);
        void notify_add_emergency(DeviceEmergency de);
        void notify_remove_emergency(DeviceEmergency de);
        void notify_attack_radius(bool show);
        void notify_update_data();
        void notify_play_audio(string audio_name);
    }


    public class Device : Model<Device, IDeviceView>, ITarget
    {
        public enum DeviceState
        {
            normal,
            immue,
            fired,
            stupor,
            acid,
            bind,
        }

        public DeviceState state;
        public DeviceEmergency device_emergency;

        public enum DeviceType
        {
            melee,
            shooter,
            other,
        };

        public DeviceType device_type = DeviceType.other;

        public Dictionary<string, Vector2> bones_direction = new();
        public List<AnimEvent> anim_events = new();
        public Dictionary<string, Transform> key_points = new();

        public List<BasicCubicle> cubicle_list = new();
        public device_all desc => m_desc;
        public battle_data battle_data;

        public ITarget owner_target;

        Vector2 ITarget.Position => position;

        Faction ITarget.Faction => faction;

        float ITarget.Mass => 10;

        bool ITarget.is_interactive => true;

        int ITarget.hp => current_hp;
        int ITarget.hp_max => max_hp;

        Vector2 ITarget.acc_attacher { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected const string BONE_FOR_ROTATE = "roll_control";
        Vector2 ITarget.direction
        {
            get
            {
                if (bones_direction.TryGetValue(BONE_FOR_ROTATE, out var v2))
                {
                    return v2;
                }
                return Vector2.right;
            }
        }
        Vector2 ITarget.velocity
        {
            get
            {
                if (faction == Faction.player)
                    return WorldContext.instance.caravan_velocity;
                else
                    return velocity;
            }
        }

        private device_all m_desc;
        private battle_data m_battle_data;
        public Faction faction = Faction.player;
        public Vector2 velocity;
        protected float rotate_speed;
        public int current_hp;
        public int max_hp;

        public Vector2 position;
        public Vector2 outter_pos_offset;
        public Vector2 view_position => position + outter_pos_offset;

        public int equip_time = 0;

        public int self_efficiency = 0;         //自身工作效率
       
        public List<Func<Device,int,int>> efficiency_func = new List<Func<Device,int, int>>();       //工作效率的函数列表



        public List<ITarget> target_list = new List<ITarget>();
        public Dictionary<ITarget,int> outrange_targets = new();

        public bool is_validate = true;
        public bool player_oper { get; protected set; }

        public List<DeviceUpgrade> upgrades = new();

        Action<ITarget> m_tick_handle;

        public Action<ITarget> kill_enemy_action;

        Dictionary<string, LinkedList<Action<ITarget>>> ITarget.buff_flags { get => m_buff_flags; set => m_buff_flags = value; }
        Dictionary<string, LinkedList<Action<ITarget>>> m_buff_flags = new();

        List<string> ITarget.fini_buffs { get => m_fini_buffs; set => m_fini_buffs = value; }
        List<string> m_fini_buffs = new();

        public event Action<ITarget> fini_callback_event;

        float ITarget.def_cut_rate { get => m_def_cut_rate; set => m_def_cut_rate = value; }
        float m_def_cut_rate;

        private int immu_countdown = 0;

        //==================================================================================================

        protected Vector2 add_caravan_dir(Vector2 bone_dir)
        {
            var cv = WorldContext.instance.caravan_dir.normalized;
            var bv = bone_dir;

            return new Vector2((bv.x * cv.x - bv.y * cv.y),(bv.x * cv.y + bv.y * cv.x));
        }

        protected virtual void rotate_bone_to_target(string bone_name)
        {
            if (target_list.Count == 0)
                return;

            var target = target_list[0];
            Vector2 target_v2;
            target_v2 = BattleUtility.get_v2_to_target_collider_pos(target, position);
            bones_direction[bone_name] = BattleUtility.rotate_v2(bones_direction[bone_name], target_v2, rotate_speed);
        }

        protected virtual void rotate_bone_to_dir(string bone_name, Vector2 dir)
        {
            bones_direction[bone_name] = BattleUtility.rotate_v2(bones_direction[bone_name], dir, rotate_speed);
        }

        /// <summary>
        /// 判断target是否仍可选
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected virtual bool target_can_be_selected(ITarget t)
        {
            return target_in_radius(t) && target_is_alive(t);
        }

        protected virtual bool target_in_radius(ITarget t)
        {
            var tp = BattleUtility.get_target_colllider_pos(t);

            if (WorldContext.instance.is_need_reset)
            {
                tp -= new Vector2(WorldContext.instance.reset_dis, 0);
            }

            var t_distance = (tp - position).magnitude;
            return t_distance >= desc.basic_range.Item1 && t_distance <= desc.basic_range.Item2;
        }

        protected virtual bool target_is_alive(ITarget t)
        {
            return t.hp > 0;
        }

        protected virtual bool try_get_target()
        {
            return false;           //不知道进行哪个范围的搜索，先进行一个false的返回
        }

        public virtual void UpdateData()
        {
            foreach(var view in views)
            {
                view.notify_update_data();
            }
        }

        public virtual void tick()
        {
            equip_time++;

            m_tick_handle?.Invoke(this);
            m_tick_handle = null;


            if(state == DeviceState.immue)
            {
                immu_countdown--;
                if (immu_countdown <= 0)
                    state = DeviceState.normal;
            }

            device_emergency?.tick();

            if (device_emergency!=null && device_emergency.removed)
            {
                foreach (var view in views)
                {
                    view.notify_remove_emergency(device_emergency);
                }

                device_emergency = null;
                state = DeviceState.immue;
                immu_countdown = Config.current.device_debuff_immue_duration;
            }

            CheckTargets();

            foreach (var view in views)
            {
                view.notify_on_tick();
            }
        }

        private void add_upgrades()
        {
            var records = device_upgrades.records.Where(t => t.Value.id == m_desc.device_upgrade);

            foreach (var record in records)
            {
                var upgrade = new DeviceUpgrade(record.Value);
                upgrades.Add(upgrade);
            }
        }

        /// <summary>
        /// 重置后 base要放到数值赋值的最后执行（状态机改动的前面），base前不要使用desc 使用device_all的rc
        /// </summary>
        /// <param name="rc"></param>
        public virtual void InitData(device_all rc)
        {
            battle_datas.TryGetValue(rc.id.ToString(), out battle_data);
            m_desc = rc;

            if (battle_data == null)
                Debug.Log(rc.id);

            current_hp = battle_data.hp;
            max_hp = current_hp;
            rotate_speed = desc.rotate_speed.Item1;

            if(rc.cubicles != null)
            {
                foreach (var cub in rc.cubicles)
                {
                    var c = Cubicle_Helper.AddDeviceCubicle(this,cub);
                    cubicle_list.Add(c);
                }
            }
            
            add_upgrades();

            foreach (var view in views)
            {
                view.init();
            }
        }

        public virtual void Start()
        {

        }

        public virtual void InitPos()
        {
            foreach(var view in views)
            {
                view.init_pos();
            }

            var slot = Device_Slot_Helper.GetSlot(this);

            var angle = BattleUtility.GetDeviceInitAngleBySlot(this,slot);

            Vector2 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.right;
            bones_direction[BONE_FOR_ROTATE] = dir;
        }

        public void ChangeAnim_Percent(string anim_name, bool loop,float percent)
        {
            foreach (var ae in anim_events)
            {
                ae.triggered = false;
            }
            foreach (var view in views)
            {
                view.notify_change_anim(anim_name, loop,percent);
            }
        }

        public virtual int Hurt(int dmg)
        {
            if (!is_validate)       //坏了就别受伤了
                return current_hp;

            current_hp = Mathf.Clamp(current_hp - dmg, 0, battle_data.hp);

            if (dmg > 0)
            {
                foreach (var view in views)
                {
                    view.notify_hurt(dmg);
                }
            }

            if (current_hp == 0)
                Disable();

            return current_hp;
        }


        void ITarget.heal(Dmg_Data dmg_data)
        {
        }


        public virtual void Disable ()
        {
            is_validate = false;
            foreach (var view in views)
            {
                view.notify_disable();
            }
        }

        public void ChangeAnim(string anim_name, bool loop)
        {
            foreach (var ae in anim_events)
            {
                ae.triggered = false;
            }
            foreach (var view in views)
            {
                view.notify_change_anim(anim_name, loop);
            }
        }

        void ITarget.impact(params object[] prms)
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
            owner_target?.do_when_hurt_target(ref attack_data);
        }


        void ITarget.hurt(ITarget harm_source, Attack_Data attack_data, out Dmg_Data dmg_data)
        {
            harm_source?.do_when_hurt_target(ref attack_data);

            dmg_data = Hurt_Calc_Helper.dmg_to_device(attack_data, this);

            Hurt(dmg_data.dmg);

            //Debug.Log($"设备受到伤害: {dmg_data.dmg} hp剩余: {current_hp} ");
        }

        public void OpenCollider(string collider_name, Action<ITarget> enter_e = null, Action<ITarget> stay_e = null, Action<ITarget> exit_e = null)
        {
            foreach (var view in views)
            {
                view.notify_open_collider(collider_name, enter_e, stay_e, exit_e);
            }
        }

        public void CloseCollider(string collider_name)
        {
            foreach (var view in views)
            {
                view.notify_close_collider(collider_name);
            }
        }

        float ITarget.distance(ITarget target)
        {
            return Vector2.Distance(position, target.Position);
        }

        void ITarget.tick_handle(Action<ITarget> outter_request, params object[] prms)
        {
            m_tick_handle += outter_request;
        }

        public virtual void StartControl()
        {
            player_oper = true;

            foreach(var view in views)
            {
                view.notify_player_oper(player_oper);
            }
        }

        /// <summary>
        /// 好像没用了
        /// </summary>
        public virtual void OperateClick()
        {

        }

        public virtual void OperateDrag(Vector2 dir)
        {

        }


        protected void DevicePlayAudio(string audio_name)
        {
            foreach(var view in views)
            {
                view.notify_play_audio(audio_name);
            }   
        }

        public virtual void EndControl()
        {
            player_oper = false;

            foreach (var view in views)
            {
                view.notify_player_oper(player_oper);
            }
        }

        public void Fix(int delta)
        {
            current_hp += delta;

            foreach(var view in views)
            {
                view.notify_fix(delta);
            }


            if (current_hp >= battle_data.hp)
            {
                current_hp = battle_data.hp;
            }
            if (is_validate == false)
            {
                var h = UnityEngine.Random.Range(0, battle_data.hp);
                if (h >= 0 && h < current_hp)
                {
                    is_validate = true;

                    foreach (var view in views)
                    {
                        view.notify_enable();
                    }
                }
            }

        }

        public void ShowRadius(bool show)
        {
            foreach (var view in views)
            {
                view.notify_attack_radius(show);
            }
        }

        /// <summary>
        /// 计算并返回对于目标t的所有Attack_Data
        /// </summary>
        /// <param name="t"></param>
        /// <param name="dmg"></param>
        /// <returns></returns>
        public virtual List<Attack_Data> ExecDmg(ITarget t,Dictionary<string,int> dmg)
        {

            List<Attack_Data> attack_datas = new();
            foreach (var d in dmg)
            {
                Attack_Data attack_data = new()
                {
                    atk = Mathf.CeilToInt(d.Value),
                    critical_chance = battle_data.critical_chance + BattleContext.instance.global_critical_chance,
                    critical_dmg_rate = battle_data.critical_rate + BattleContext.instance.global_critical_chance,
                    armor_piercing = battle_data.armor_piercing + BattleContext.instance.global_armor_piercing,
                    diy_atk_str = d.Key,
                };

                attack_datas.Add(attack_data);
            }

            return attack_datas;
        }


        #region 设备Target相关

        public void CheckTargets()
        {
            for (int i = target_list.Count - 1; i >= 0; i--)         //每帧确认目标队列中的目标是否仍然有效
            {
                
                if (!target_is_alive(target_list[i]))
                {
                    if (target_list[i] is Enemy enemy)
                    {
                        enemy.Select(false);
                    }

                    target_list.RemoveAt(i);

                    continue;
                }
                if (!target_in_radius(target_list[i]))
                {
                    if (target_list[i] is Enemy enemy)
                    {
                        enemy.Select(false);
                        enemy.OutRangeSelect(true);
                    }
                    outrange_targets.Add(target_list[i],desc.unlock_target_ticks);
                    target_list.RemoveAt(i);
                }
            }


            for(int i = outrange_targets.Count - 1; i >= 0; i--)    //确认超出范围的目标是否有效，有效就回归到目标队列，无效就删了
            {
                if (target_in_radius(outrange_targets.ElementAt(i).Key))        //重回范围
                {
                    if (outrange_targets.ElementAt(i).Key is Enemy enemy)
                    {
                        enemy.Select(true);
                        enemy.OutRangeSelect(false);
                    }

                    target_list.Add(outrange_targets.ElementAt(i).Key);
                    outrange_targets.Remove(outrange_targets.ElementAt(i).Key);

                    continue;
                }

                outrange_targets[outrange_targets.ElementAt(i).Key] -= 1;
                if (outrange_targets.ElementAt(i).Value <= 0)
                {
                    if (outrange_targets.ElementAt(i).Key is Enemy enemy)
                    {
                        enemy.OutRangeSelect(false);
                    }

                    outrange_targets.Remove(outrange_targets.ElementAt(i).Key);
                }
            }
        }

        void ITarget.applied_outlier(ITarget source, string outlier_type, int value)
        {
            if (state == DeviceState.immue)
                return;

            switch (outlier_type)
            {
                case "sharp":
                    break;
                case "blunt":
                    if(state == DeviceState.normal)
                    {
                        var blunt_k = Config.current.k_stupor_rate_factor;
                        var rate = (1 - blunt_k) * (value - blunt_k) / (value - blunt_k + 1) + blunt_k;

                        var rnd = UnityEngine.Random.Range(0f, 1f);
                        if(rate > rnd)
                        {
                            var stupor = new DeviceStupor(this);
                            device_emergency = stupor;
                            
                            foreach(var view in views)
                            {
                                view.notify_add_emergency(stupor);
                            }
                            
                            state = DeviceState.stupor;
                        }
                    }
                    break;
                case "fire":
                    if(state == DeviceState.normal || state == DeviceState.fired)
                    {
                        if(device_emergency is DeviceFired df)
                        {
                            df.ChangeFire(value);
                        }
                        else
                        {
                            var fire_k = Config.current.k_damage_ignite_factor;
                            var fire_k2 = Config.current.max_ignite;
                            float fire_value = fire_k2 * ((1 - fire_k) * (value - fire_k) / (value - fire_k + 1) + fire_k);
                            var fire = new DeviceFired(this, fire_value);
                            device_emergency = fire;

                            foreach (var view in views)
                            {
                                view.notify_add_emergency(fire);
                            }
                        }
                        state = DeviceState.fired;
                    }
                    break;
                case "acid":
                    if(state == DeviceState.normal || state == DeviceState.acid)
                    {
                        var acid_value = (int)(value * Config.current.k_damage_acid_count_rate) + 1;
                        if(device_emergency is DeviceAcid da)
                        {
                            da.AddAcid(acid_value);
                        }
                        else
                        {
                            var acid = new DeviceAcid(this, acid_value);
                            device_emergency = acid;
                            foreach (var view in views)
                            {
                                view.notify_add_emergency(acid);
                            }
                            state = DeviceState.acid;
                        }
                    }
                    break;
                case "wrap":
                    if (state == DeviceState.normal || state == DeviceState.bind)
                    {
                        var wrap_value = (int)(value * Config.current.k_damage_to_vine_rate) + 1;
                        if (device_emergency is DeviceBind db)
                        {
                            db.AddVine(wrap_value);
                        }
                        else
                        {
                            var wrap = new DeviceBind(this, wrap_value);
                            device_emergency = wrap;
                            foreach (var view in views)
                            {
                                view.notify_add_emergency(wrap);
                            }
                            state = DeviceState.bind;
                        }
                    }
                    break;
                case "flash":
                    break;  
                default:
                    break;
            }
        }
        #endregion

        /// <summary>
        /// 对于一个设备，她可能有N个工作状态，比如开火，收缩，旋转等,为true表示 玩家可以操作
        /// </summary>
        /// <param name="work_id"></param>
        /// <returns></returns>
        public virtual List<bool> GetWorkState()
        {
            return null;
        }
    }


    public class AnimEvent
    {
        public string anim_name;
        public float percent;
        public bool triggered;
        public Action<Device> anim_event;
    }
}





