using Commons;
using Foundations.Tickers;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Lake_SmallJelly : Enemy_BT, IEnemy_BT_FlyAround
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Surrond,
            Attack
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region Outter
        public BT_Lake_GiantJelly owner_bt;
        public Enemy owner;
        #endregion

        #region PRM
        float e_idle_speed;
        float e_charge_speed;

        bool m_is_face_dir;
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
        

        int e_flyAround_count_max;
        int e_flyAround_count_min;
        int m_flyAround_count_limit;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_flyAround_deg = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_DEG");
            e_flyAround_radius = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_RADIUS");
            

            e_flyAround_count_max = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MAX"));
            e_flyAround_count_min = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MIN"));

            e_idle_speed = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "IDLE_SPEED"));
            e_charge_speed = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "CHARGE_SPEED"));
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Surrond;

            cell.mover.move_type = EN_enemy_move_type.Fly1;

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        #region Surrond
        public void start_Surrond(Enemy cell)
        {
            cell.base_speed = e_idle_speed;

            m_flyAround_count_limit = Random.Range(e_flyAround_count_min, e_flyAround_count_max + 1);

            i_flyAround.calc_flyAround_pos();
        }


        public void do_Surrond(Enemy cell)
        {
            i_flyAround.flyAround_on_tick(cell, owner.pos);

            if (m_flyAround_count >= m_flyAround_count_limit)
            {
                m_flyAround_count = 0;

                //规则：盘旋结束后，如果母体指示攻击，则小水母攻击
                if (owner_bt.can_attack)
                {
                    m_state = EN_FSM.Attack;
                    m_is_face_dir = true;
                }
            } 
        }
        #endregion


        #region Attack
        public void start_Attack(Enemy cell)
        {
            cell.target = owner.target;
            cell.position_expt = cell.target.Position;

            cell.base_speed = e_charge_speed;
            cell.velocity = owner.velocity;
        }


        public void do_Attack(Enemy cell)
        {
            cell.target = owner.target;
            cell.position_expt = cell.target.Position;

            if (Vector2.Distance(cell.pos, cell.position_expt) <= 0.2f)
            {
                Enemy_Hurt_Helper.do_hurt(cell);
                cell.velocity *= -1;

                m_state = EN_FSM.Surrond;

                var req_name = $"req_recover_dir_{cell.GUID}";
                Request_Helper.del_requests(req_name);
                Request_Helper.delay_do(req_name, Config.PHYSICS_TICKS_PER_SECOND,
                    (_) => { m_is_face_dir = false; });
            }
        }
        #endregion


        public override void notify_on_set_face_dir(Enemy cell)
        {
            if (m_is_face_dir)
            {
                cell.is_ban_scaleY_slip = true;
                var ori_dir = cell.velocity.normalized;
                cell.dir = new(ori_dir.y, -ori_dir.x);
            }
            else
            {
                base.notify_on_set_face_dir(cell);
            }
        }


        public override void notify_on_dead(Enemy cell)
        {
            if (owner.is_alive)
            {
                owner_bt.sub_cells.Remove(cell);
            }

            base.notify_on_dead(cell);
        }
    }
}


