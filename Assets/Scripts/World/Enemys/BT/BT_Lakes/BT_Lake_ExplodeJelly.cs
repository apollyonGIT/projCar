using Commons;
using Foundations.Tickers;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Lake_ExplodeJelly : Enemy_BT, IEnemy_BT_FlyAround
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Wander,
            Attack
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        float e_idle_speed;
        float e_charge_speed;

        float e_explode_dis;
        float e_before_attack_sec;

        bool m_is_dead;
        #endregion

        #region FlyAround
        Vector2 IEnemy_BT_FlyAround.flyAround_deg => e_flyAround_deg;

        Vector2 IEnemy_BT_FlyAround.flyAround_radius => e_flyAround_radius;

        

        Vector2 IEnemy_BT_FlyAround.flyAround_pos { get => m_flyAround_pos; set => m_flyAround_pos = value; }
        Vector2 m_flyAround_pos;

        int IEnemy_BT_FlyAround.flyAround_count { get => m_flyAround_count; set => m_flyAround_count = value; }
        int m_flyAround_count;

        IEnemy_BT_FlyAround i_flyAround => this;

        Vector2 e_flyAround_deg;
        Vector2 e_flyAround_radius;
        
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_flyAround_deg = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_DEG");
            e_flyAround_radius = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_RADIUS");
            

            e_idle_speed = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "IDLE_SPEED"));
            e_charge_speed = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "CHARGE_SPEED"));

            e_explode_dis = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "EXPLODE_DIS"));
            e_before_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEFORE_ATTACK_SEC"));
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Wander;

            cell.mover.move_type = EN_enemy_move_type.Fly1;

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        #region Wander
        public void start_Wander(Enemy cell)
        {
            cell.base_speed = e_idle_speed;

            SeekTarget_Helper.select_caravan(out cell.target);

            i_flyAround.calc_flyAround_pos();

            Request req = new($"valid_break_wander_{cell.GUID}",
                (req) =>
                {
                    req.prms_dic.TryGetValue("is_hurt", out var _is_hurt);
                    return (bool)_is_hurt;
                },
                (req) =>
                {
                    req.prms_dic.Add("is_hurt", false);
                },
                (_) =>
                {
                    m_state = EN_FSM.Attack;
                },
                null);
        }


        public void do_Wander(Enemy cell)
        {
            i_flyAround.flyAround_on_tick(cell, cell.target.Position);
        }
        #endregion


        #region Attack
        public void start_Attack(Enemy cell)
        {
            cell.base_speed = e_charge_speed;
        }


        public void do_Attack(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

            if (m_is_dead) return;

            try_explosion(cell.target);

            #region 子函数 explosion
            bool try_explosion(ITarget target)
            {
                //规则：目标处于爆炸范围内，对范围内的所有玩家part进行伤害
                var dis = Vector2.Distance(target.Position, cell.pos);
                if (dis <= e_explode_dis)
                {
                    m_is_dead = true;

                    Request_Helper.delay_do($"explosion_{cell.GUID}", Mathf.FloorToInt(e_before_attack_sec * Config.PHYSICS_TICKS_PER_SECOND),
                        (_) => {
                            Enemy_Hurt_Helper.do_aoe_hurt_by_dis(cell, e_explode_dis);
                            notify_on_dead(cell);
                        });
                    
                    return true;
                }

                return false;
            }
            #endregion
        }
        #endregion


        public override void notify_on_dead(Enemy cell)
        {
            Enemy_BT_VFX_CPN.set_explosion_death_vfx(cell);

            cell.fini();
        }
    }
}


