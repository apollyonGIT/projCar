using AutoCodes;
using Commons;
using Foundations.Tickers;
using System.Collections.Generic;
using UnityEngine;
using World.Audio;
using World.Devices.Device_AI;
using World.Helpers;
using static World.WorldEnum;

namespace World.Devices
{
    public class Unique_War_Drum : Device, IAttack,IAttack_Device
    {
        #region CONST
        private const string ANIM_IDLE = "idle";
        private const string ANIM_ATTACK_L = "attack_1";
        private const string ANIM_ATTACK_R = "attack_2";
        private const string ANIM_BROKEN = "idle";
        //private const string BONE_FOR_ROTATION = "control";

        private const float EXPLOSION_EFFECT_MIN = 0.5F;

        private const float ATK_EVENT_TIME_PERCENT = 13f / 20f;

        private const int AUTO_ATTACK_DELAY = 120;
        #endregion

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;
        #endregion

        public class Beat
        {
            public bool is_right;
            public int state; // 0: 未触发 1：当前 2：已触发
        }


        public List<Beat> beats = new List<Beat>();

        public int current_attacking_tick { get; private set; }
        public int attacking_tick { get; private set; }

        private int current_explode_interval;   //距离上次爆炸的间隔帧数
        private const int explode_interval = 60;

        private enum Device_War_Drum_FSM
        {
            idle,
            attack,
            broken,
        }
        private Device_War_Drum_FSM fsm;

        private List<ITarget> targets;
        private Request request;

        public float attack_factor_expt = 1f;
        private float attack_range;

        private bool will_atk_right;

        public bool manual_left_ready => fsm == Device_War_Drum_FSM.idle && !will_atk_right;
        public bool manual_right_ready => fsm == Device_War_Drum_FSM.idle && will_atk_right;

        public float Damage_Increase { get; set; }
        public float Knockback_Increase { get; set; }
        public int Attack_Interval { get; set; }
        public int Current_Interval { get; set; }

        public float Attack_Interval_Factor { get; set; }

        public override List<Attack_Data> ExecDmg(ITarget target, Dictionary<string, int> dmg)
        {
            battle_datas.TryGetValue(desc.id.ToString(), out var battle_data);

            var dis = Vector2.Distance(target.Position, position);
            var dis_coef = (EXPLOSION_EFFECT_MIN - 1) / attack_range * dis + 1;  // Min Damage == 50%
            List<Attack_Data> attack_datas = new();
            foreach (var d in dmg)
            {
                Attack_Data attack_data = new()
                {
                    atk = Mathf.CeilToInt((d.Value + Damage_Increase) * dis_coef),
                    critical_chance = battle_data.critical_chance + BattleContext.instance.global_critical_chance,
                    critical_dmg_rate = battle_data.critical_rate + BattleContext.instance.global_critical_chance,
                    diy_atk_str = d.Key,
                };

                attack_datas.Add(attack_data);
            }

            return attack_datas;
        }
        public override void InitData(device_all rc)
        {
            bones_direction.Clear();

            other_logics.TryGetValue(rc.other_logic.ToString(), out var logic);

            attack_range = rc.basic_range.Item2;
            
            base.InitData(rc);

            FSM_change_to(Device_War_Drum_FSM.idle);

            #region AnimInfo
            var atk_anim_l_hit = new AnimEvent()
            {
                anim_name = ANIM_ATTACK_L,
                percent = ATK_EVENT_TIME_PERCENT,
                anim_event = (Device d) =>
                {
                    BoardCast_Helper.to_all_target(explosion);
                    AudioSystem.instance.PlayOneShot(logic.se_1);
                }
            };

            var atk_anim_l_end = new AnimEvent()
            {
                anim_name = ANIM_ATTACK_L,
                percent = 1,
                anim_event = (Device d) => FSM_change_to(Device_War_Drum_FSM.idle)
            };

            var atk_anim_r_hit = new AnimEvent()
            {
                anim_name = ANIM_ATTACK_R,
                percent = ATK_EVENT_TIME_PERCENT,
                anim_event = (Device d) =>
                {
                    BoardCast_Helper.to_all_target(explosion);
                    AudioSystem.instance.PlayOneShot(logic.se_1);
                }
            };

            var atk_anim_r_end = new AnimEvent()
            {
                anim_name = ANIM_ATTACK_R,
                percent = 1,
                anim_event = (Device d) => FSM_change_to(Device_War_Drum_FSM.idle)
            };

            #region 子函数 explosion
            void explosion(ITarget target)
            {
                WorldSceneRoot.instance.sr.SetActive(true);

                if (request != null)
                    request.interrupt();

                request = Request_Helper.delay_do("close_screen_shock", 30, (req) =>
                {
                    WorldSceneRoot.instance.sr.SetActive(false);
                });

                var dis = Vector2.Distance(target.Position, position);
                if (dis >= attack_range)  //爆炸范围
                    return;

                if (target.Faction != faction)
                {
                    var attack_datas = ExecDmg(target,logic.damage);
                    
                    foreach(var attack_data in attack_datas)
                    {
                        attack_data.calc_device_coef(this);

                        target.hurt(this, attack_data, out var dmg_data);
                        BattleContext.instance.ChangeDmg(this, dmg_data.dmg);

                        target.applied_outlier(this, attack_data.diy_atk_str,dmg_data.dmg);
                    }

                    if (target.hp <= 0)
                    {
                        kill_enemy_action?.Invoke(target);
                    }
                    
                    // Make Damage
                    var dis_coef = (EXPLOSION_EFFECT_MIN - 1) / attack_range * dis + 1;  // Min Damage == 50%

                    // Make KnockBack
                    target.impact(impact_source_type.melee, position, target.Position, logic.knockback_ft * dis_coef * (1 + Knockback_Increase));
                }
            }
            #endregion

            #endregion

            /*anim_events.Add(atk_anim_l_hit);
            anim_events.Add(atk_anim_l_end);
            anim_events.Add(atk_anim_r_hit);
            anim_events.Add(atk_anim_r_end);*/
            
        }


        private void set_beats()
        {
            beats.Clear();
            var rand = Random.Range(3, 6);
            for (int i = 0; i < rand; i++)
            {
                var b = new Beat()
                {
                    is_right = Random.value > 0.5f,
                    state = 0
                };
                if (i == 0)
                    b.state = 1;
                beats.Add(b);
            }
        }

        private void explosion(ITarget target)
        {
            other_logics.TryGetValue(desc.other_logic.ToString(), out var logic);
            WorldSceneRoot.instance.sr.SetActive(true);

            if (request != null)
                request.interrupt();

            request = Request_Helper.delay_do("close_screen_shock", 30, (req) =>
            {
                WorldSceneRoot.instance.sr.SetActive(false);
            });

            var dis = Vector2.Distance(target.Position, position);
            if (dis >= attack_range)  //爆炸范围
                return;

            if (target.Faction != faction)
            {
                var attack_datas = ExecDmg(target, logic.damage);

                foreach (var attack_data in attack_datas)
                {
                    attack_data.calc_device_coef(this);

                    target.hurt(this, attack_data, out var dmg_data);
                    BattleContext.instance.ChangeDmg(this, dmg_data.dmg);

                    target.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);
                }

                if (target.hp <= 0)
                {
                    kill_enemy_action?.Invoke(target);
                }

                // Make Damage
                var dis_coef = (EXPLOSION_EFFECT_MIN - 1) / attack_range * dis + 1;  // Min Damage == 50%

                // Make KnockBack
                target.impact(impact_source_type.melee, position, target.Position, logic.knockback_ft * dis_coef * (1 + Knockback_Increase));
            }
        }

        public override void tick()
        {
            if (!is_validate)       //坏了
                FSM_change_to(Device_War_Drum_FSM.broken);

            switch (fsm)
            {
                case Device_War_Drum_FSM.idle:
                    attack_factor_expt -= (attack_factor_expt - 1) * 0.01f;
                    break;
                case Device_War_Drum_FSM.attack:
                    if (current_attacking_tick < attacking_tick)
                    {
                        current_attacking_tick++;
                        current_explode_interval--;
                        if(current_explode_interval <= 0)
                        {
                            current_explode_interval = explode_interval;

                            other_logics.TryGetValue(desc.other_logic.ToString(), out var logic);
                            BoardCast_Helper.to_all_target(explosion);
                            AudioSystem.instance.PlayOneShot(logic.se_1);
                        }
                    }   
                    else
                        FSM_change_to(Device_War_Drum_FSM.idle);



                    break;
                case Device_War_Drum_FSM.broken:
                    if (is_validate)
                        FSM_change_to(Device_War_Drum_FSM.idle);
                    break;

                default:
                    break;
            }
            base.tick();
        }

        private void FSM_change_to(Device_War_Drum_FSM target_fsm)
        {
            fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_War_Drum_FSM.idle:
                    set_beats();
                    ChangeAnim(ANIM_IDLE, true);
                    break;
                case Device_War_Drum_FSM.attack:
                    current_explode_interval = 0;
                    ChangeAnim(will_atk_right ? ANIM_ATTACK_R : ANIM_ATTACK_L, true);
                    will_atk_right = !will_atk_right;
                    break;
                case Device_War_Drum_FSM.broken:
                    ChangeAnim(ANIM_BROKEN, true);
                    break;
                default:
                    break;
            }
        }

        private void change_factor_of_anim_speed(float f)
        {
            foreach (var view in views)
                view.notify_change_anim_speed(f);
        }


        private void Auto_tick()
        {
            if (player_oper)
                return;

            switch (fsm)
            {
                case Device_War_Drum_FSM.idle:
                    if (Current_Interval <= 0)
                    {
                        if (has_target())
                            try_to_beat_drum();
                        Current_Interval = AUTO_ATTACK_DELAY;
                    }
                    else
                        Current_Interval--;
                    break;

                default:
                    break;
            }
        }

        private void try_to_beat_drum()
        {
            for (int i = 0; i < beats.Count; i++)
            {
                if (beats[i].state == 1)
                {
                    beats[i].state = 2;

                    if (i + 1 < beats.Count)
                    {
                        beats[i + 1].state = 1;
                    }
                    else
                    {
                        attacking_tick = beats.Count * Config.PHYSICS_TICKS_PER_SECOND;
                        current_attacking_tick = 0;
                        beats.Clear();
                        FSM_change_to(Device_War_Drum_FSM.attack);
                    }
                    break;
                }
            }
        }


        private bool has_target()
        {
            targets = BattleUtility.select_all_target_in_circle(position, attack_range, faction);
            return targets.Count > 0;
        }


        void IAttack.TryToAutoAttack()
        {
            Auto_tick();
        }

        private void beat_drum(bool input_right)
        {
            if (fsm == Device_War_Drum_FSM.idle && input_right == will_atk_right)
            {
                FSM_change_to(Device_War_Drum_FSM.attack);
                attack_factor_expt += 0.5f;
            }
            else
            {
                attack_factor_expt = 1f;
            }
            change_factor_of_anim_speed(attack_factor_expt);
        }

        #region UI Info & UI Controlled Actions



        /// <summary>
        /// 检查当前节拍是否正确，如果正确判断是否会进入到攻击状态 (失败惩罚暂时不做)
        /// </summary>
        /// <param name="is_right"></param>
        public void UI_Controlled_Beating(bool is_right)
        {
            for (int i = 0; i < beats.Count; i++)
            {
                if (beats[i].state == 1)
                {
                    if (beats[i].is_right == is_right)
                    {
                        beats[i].state = 2;

                        if(i + 1 < beats.Count)
                        {
                            beats[i + 1].state = 1;
                        }
                        else
                        {
                            attacking_tick = beats.Count * Config.PHYSICS_TICKS_PER_SECOND;
                            current_attacking_tick = 0;
                            beats.Clear();
                            FSM_change_to(Device_War_Drum_FSM.attack);
                        }
                            
                    }
                    else
                    {
                        //敲错了 奖励重敲一遍
                        set_beats();

                    }
                        break;
                }
            }
        }

        #endregion

        #region PlayerControl
        public override void StartControl()
        {
            base.StartControl();
        }

        public override void EndControl()
        {
            base.EndControl();
        }

        public void Attack_1()
        {
            beat_drum(false);
        }

        public void Attack_2()
        {
            beat_drum(true);
        }
        #endregion
    }
}
