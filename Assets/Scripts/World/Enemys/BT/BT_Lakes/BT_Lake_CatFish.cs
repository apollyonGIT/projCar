using Commons;
using Foundations.Tickers;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Lake_CatFish : Enemy_BT, IEnemy_BT_FlyAround, IEnemy_BT_FlyFollow
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Hover,
            Close,
            Prepare,
            Attack,
            Recover
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        Vector2 e_flyFollow_dis;
        float m_flyFollow_dis;

        float e_before_attack_sec;
        float e_after_attack_sec;

        float e_attack_lasting_sec;
        int e_attack_count;

        int m_atk;
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


        #region FlyFollow
        Vector2 IEnemy_BT_FlyFollow.flyFollow_pos => m_flyFollow_pos;
        Vector2 m_flyFollow_pos;

        IEnemy_BT_FlyFollow i_flyFollow => this;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_flyAround_deg = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_DEG");
            e_flyAround_radius = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYAROUND_RADIUS");
            

            e_flyAround_count_max = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MAX"));
            e_flyAround_count_min = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "FLYAROUND_COUNT_MIN"));

            e_flyFollow_dis = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "FLYFOLLOW_DIS");

            e_before_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEFORE_ATTACK_SEC"));
            e_after_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "AFTER_ATTACK_SEC"));

            e_attack_lasting_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATTACK_LASTING_SEC"));
            e_attack_count = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATTACK_COUNT"));
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Hover;

            cell.mover.move_type = EN_enemy_move_type.Fly1;

            //m_atk = Mathf.FloorToInt(cell._desc.basic_atk / (float)e_attack_count);
            m_atk = Mathf.FloorToInt(10f / (float)e_attack_count);

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        #region Hover
        public void start_Hover(Enemy cell)
        {
            SeekTarget_Helper.select_caravan(out cell.target);

            m_flyAround_count_limit = Random.Range(e_flyAround_count_min, e_flyAround_count_max + 1);

            i_flyAround.calc_flyAround_pos();
        }


        public void do_Hover(Enemy cell)
        {
            i_flyAround.flyAround_on_tick(cell, cell.target.Position);

            if (m_flyAround_count >= m_flyAround_count_limit)
            {
                m_flyAround_count = 0;
                m_state = EN_FSM.Close;
            }
        }
        #endregion


        #region Close
        public void start_Close(Enemy cell)
        {
            //规则：优先索最近玩家设备，如果全部损坏，转火车体
            SeekTarget_Helper.nearest_player_device(cell, out cell.target);
            if (cell.target == null)
            {
                SeekTarget_Helper.select_caravan(out cell.target);
            }

            m_flyFollow_dis = Random.Range(e_flyFollow_dis.x, e_flyFollow_dis.y);
        }


        public void do_Close(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

            if (Vector2.Distance(cell.pos, cell.target.Position) <= m_flyFollow_dis)
            {
                m_state = EN_FSM.Prepare;
            }
        }
        #endregion


        #region Prepare
        public void start_Prepare(Enemy cell)
        {
            m_flyFollow_pos = cell.pos - cell.target.Position;

            Request_Helper.delay_do($"end_prepare_{cell.GUID}",
                Mathf.FloorToInt(e_before_attack_sec * Config.PHYSICS_TICKS_PER_SECOND),
                (_) => { m_state = EN_FSM.Attack; });
        }


        public void do_Prepare(Enemy cell)
        {
            i_flyFollow.notify_on_tick(cell, cell.target.Position);
        }
        #endregion


        #region Attack
        public void start_Attack(Enemy cell)
        {
            var finish_attack_count = 0;

            for (int i = 0; i < e_attack_count; i++)
            {
                Request_Helper.delay_do($"attack_req_201804_{cell.GUID}", i * Mathf.FloorToInt(e_attack_lasting_sec / e_attack_count * Config.PHYSICS_TICKS_PER_SECOND),
                (_) => {
                    var attack_data = Enemy_Hurt_Helper.create_attack_data(cell, "sharp", 10);
                    attack_data.atk = m_atk;

                    cell.target.hurt(cell, attack_data, out var ob);

                    if (++finish_attack_count == e_attack_count)
                    {
                        m_state = EN_FSM.Recover;
                    }
                });
            }
        }


        public void do_Attack(Enemy cell)
        {
            i_flyFollow.notify_on_tick(cell, cell.target.Position);
        }
        #endregion


        #region Recover
        public void start_Recover(Enemy cell)
        {
            Request_Helper.delay_do($"end_recover_{cell.GUID}",
                Mathf.FloorToInt(e_after_attack_sec * Config.PHYSICS_TICKS_PER_SECOND),
                (_) => { m_state = EN_FSM.Hover; });
        }


        public void do_Recover(Enemy cell)
        {
            i_flyFollow.notify_on_tick(cell, cell.target.Position);
        }
        #endregion
    }
}

