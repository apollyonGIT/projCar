using AutoCodes;
using World.Enemys.BT;
using World.Enemys;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace World.Devices.Device_AI
{
    public class Nailed_Enemy
    {
        public Enemy enemy;
        public int hurt_interval;
        public Vector2 offset_pos;

        public bool need_remove = false;
        public Nailed_Enemy(Enemy e, Vector2 offset_pos)
        {
            enemy = e;
            this.offset_pos = offset_pos;
        }

        public void tick(Device d)
        {

        }
    }

    public class Unique_Cactus : Device,IAttack, IAttack_Device
    {
        // 通过逻辑数值来拉长碰撞器 ×

        // 6.18 改为每个仙人掌都有自己独立的碰撞器，根据进度来控制碰撞器的开关

        #region CONST

        private const string ANIM_IDLE = "idle";
        private const string ANIM_BROKEN = "idle";
        private const string COLLIDER_1 = "collider_1";
        protected const string KEY_POINT_1 = "collider_1";

        private readonly List<string> CACTUS_COLLIDERS = new List<string>()
        {
            "CACTUS_8", "CACTUS_7", "CACTUS_6", "CACTUS_5", "CACTUS_4", "CACTUS_3", "CACTUS_2","CACTUS_1"
        };
        #endregion

        #region IAttack

        public float Damage_Increase { get; set; }
        public float Knockback_Increase { get; set; }
        int IAttack.Attack_Interval { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        int IAttack.Current_Interval { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        float IAttack.Attack_Interval_Factor { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        #endregion 


        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;
        #endregion

        protected int basic_damage = 10;
        protected float damage_speed_mod;
        protected float basic_knockback;
        protected float knockback_speed_mod;
        protected float knock_y;

        private enum Device_FSM_Cactus
        {
            idle,
            broken,
        }

        private List<Nailed_Enemy> nailed_enemys = new();

        private Device_FSM_Cactus fsm;

        public float cactus_states = 0;
        public float cactus_states_max = 100;

        public override void InitData(device_all rc)
        {
            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATE, Vector2.right);

            other_logics.TryGetValue(rc.other_logic.ToString(), out var logic);
            base.InitData(rc);

            nailed_enemys.Clear();
        }

        private void UpdateCollider()
        {
            var cnt = (cactus_states/12.5f);

            for(int i = 0; i < CACTUS_COLLIDERS.Count; i++)
            {
                if(i <= cnt)
                {
                    OpenCollider(CACTUS_COLLIDERS[i], collider_enter_event, null, collider_exit_event);
                }
                else
                {
                    CloseCollider(CACTUS_COLLIDERS[i]);
                }
            }
        }


        public override void tick()
        {
            if (!is_validate)
            {
                FSM_change_to(Device_FSM_Cactus.broken);
            }

            switch (fsm)
            {
                case Device_FSM_Cactus.idle:

                    cactus_states += 5f * Commons.Config.PHYSICS_TICK_DELTA_TIME;
                    cactus_states = Mathf.Min(cactus_states, cactus_states_max);

                    UpdateCollider();


                    if (nailed_enemys.Count > 0)
                    {
                        foreach (var ne in nailed_enemys)
                        {
                            if (ne.enemy.hp >0)
                            {
                                ne.tick(this);
                                // 强制怪物的位置
                            }
                            else
                            {
                                ne.need_remove = true;   
                            }
                        }
                    }
                    break;
                case Device_FSM_Cactus.broken:
                    if (is_validate)
                        FSM_change_to(Device_FSM_Cactus.idle);
                    break;
            }

            for(int i = nailed_enemys.Count - 1; i >= 0; i--)
            {
                var ne = nailed_enemys[i];
                if (ne.need_remove)
                {
                    nailed_enemys.RemoveAt(i);
                    // 解除眩晕
                }
            }

            base.tick();
        }

        private void FSM_change_to(Device_FSM_Cactus target_fsm)
        {
            if (fsm != target_fsm)
            {
                fsm = target_fsm;
                switch (fsm)
                {
                    case Device_FSM_Cactus.idle:
                        ChangeAnim(ANIM_IDLE,true);
                        break;
                    case Device_FSM_Cactus.broken:
                        ChangeAnim(ANIM_BROKEN,true);
                        break;
                }
            }
        }

        protected virtual void collider_enter_event(ITarget t)
        {
            var v_relative_parallel = Vector2.Dot((this as ITarget).velocity - t.velocity, bones_direction[BONE_FOR_ROTATE].normalized);
 //           if (v_relative_parallel <= 0)
 //               return;

            int final_dmg = (int)((basic_damage + Damage_Increase) * Mathf.Abs(v_relative_parallel) * damage_speed_mod);
            float final_ft = (basic_knockback + Knockback_Increase) * Mathf.Abs(v_relative_parallel) * knockback_speed_mod;

            battle_datas.TryGetValue(desc.id.ToString(), out var battle_data);

            Attack_Data attack_data = new()
            {
                atk = final_dmg,
                critical_chance = battle_data.critical_chance + BattleContext.instance.global_critical_chance,
                critical_dmg_rate = battle_data.critical_rate + BattleContext.instance.global_critical_chance,
                armor_piercing = battle_data.armor_piercing + BattleContext.instance.global_armor_piercing,
            };

            attack_data.calc_device_coef(this);

            t.hurt(this, attack_data, out var dmg_data);
            BattleContext.instance.ChangeDmg(this, dmg_data.dmg);

            if (t.hp <= 0)
            {
                kill_enemy_action?.Invoke(t);
            }

            cactus_states -= 10;
            cactus_states = Mathf.Max(0,cactus_states);

            var sign = Mathf.Sign(BattleUtility.get_target_colllider_pos(t).x - key_points[KEY_POINT_1].position.x);
            t.impact(WorldEnum.impact_source_type.melee, Vector2.zero, new Vector2(sign, knock_y), final_ft);
            if (t is Enemy e)
            {
                if (!nailed_enemys.Any(ne => ne.enemy == e))
                {
                    var nailed_enemy = new Nailed_Enemy(e, BattleUtility.get_target_colllider_pos(e) - position);
                    nailed_enemys.Add(nailed_enemy);

                    // e.stunned(true);  眩晕怪物
                }

                if (e.old_bt is IEnemy_Can_Jump j)
                    j.Get_Rammed(final_ft);
            }
        }

        protected virtual void collider_exit_event(ITarget t)
        {
            var enemy = t as Enemy;
            if(nailed_enemys.Any(e => e.enemy == enemy))
            {
                var nailed_enemy = nailed_enemys.FirstOrDefault(e => e.enemy == enemy);
                if (nailed_enemy != null)
                {
                    nailed_enemy.need_remove = true;
                    //e.stunned(false);  不晕了
                }
            }
        }
        public override void Disable()
        {
            cactus_states = 0;

            foreach(var c in CACTUS_COLLIDERS)
            {
                CloseCollider(c);
            }

            foreach(var nailed_enemy in nailed_enemys)
            {
                //解除眩晕
            }

            nailed_enemys.Clear();
            base.Disable();
        }

        public void Accelerate()
        {
            if (cactus_states < cactus_states_max)
            {
                cactus_states += 2f;
                cactus_states = Mathf.Min(cactus_states, cactus_states_max);
            }
        }

        void IAttack.TryToAutoAttack()
        {
            
        }
    }
}
