using Commons;
using Foundations.Tickers;
using System.Collections.Generic;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Lake_GiantJelly : Enemy_BT, IEnemy_BT_FlyAround, IEnemy_BT_FlyFollow
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Chase,
            Hover
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region Outter
        public bool can_attack;

        public LinkedList<Enemy> sub_cells = new();
        #endregion

        #region PRM
        int e_sub_cell_count_max;

        float e_create_interval_sec;
        int m_create_interval_tick;

        float e_create_radius;
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

            e_sub_cell_count_max = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "SUB_CELL_COUNT_MAX"));
            e_create_interval_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "CREATE_INTERVAL_SEC"));
            e_create_radius = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "CREATE_RADIUS"));
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Chase;

            cell.mover.move_type = EN_enemy_move_type.Fly1;

            SeekTarget_Helper.select_caravan(out cell.target);

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            if (m_create_interval_tick-- <= 0)
            {
                m_create_interval_tick = Mathf.RoundToInt(e_create_interval_sec * Config.PHYSICS_TICKS_PER_SECOND);

                if (sub_cells.Count < e_sub_cell_count_max)
                {
                    Request_Helper.delay_do($"add_enemy_req_201806_{cell.GUID}", 0,
                    (_) => {
                        //进行召唤
                        var mgr = cell.mgr;
                        var pd = mgr.pd;

                        var deg = Random.Range(0, 360);
                        var rad = deg * Mathf.Deg2Rad;

                        var sub_cell_pos = 0.1f * new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) + cell.pos;
                        sub_cell_pos.x -= mgr.ctx.caravan_pos.x;

                        var sub_cell = pd.create_single_cell_directly(201807u, sub_cell_pos, "Default");
                        sub_cell.velocity = cell.velocity;
                        sub_cell.target = cell.target;
                        sub_cell.dir = cell.dir;

                        var sub_cell_bt = sub_cell.bt as BT_Lake_SmallJelly;
                        sub_cell_bt.owner = cell;
                        sub_cell_bt.owner_bt = this;

                        sub_cells.AddLast(sub_cell);
                    });
                }
            }

            base.tick(cell);
        }


        public override void notify_on_set_face_dir(Enemy cell)
        {
            Enemy_BT_Face_CPN.set_face_dir_to_player_only_x(cell);
        }


        #region Chase
        public void start_Chase(Enemy cell)
        {
            m_flyAround_count_limit = Random.Range(e_flyAround_count_min, e_flyAround_count_max + 1);

            i_flyAround.calc_flyAround_pos();
        }


        public void do_Chase(Enemy cell)
        {
            i_flyAround.flyAround_on_tick(cell, cell.target.Position);

            if (m_flyAround_count >= m_flyAround_count_limit)
            {
                m_flyAround_count = 0;
                m_state = EN_FSM.Hover;
            }
        }


        public void end_Chase(Enemy cell)
        {
            seek_target();

            #region 子函数 seek_target
            void seek_target()
            {
                //规则：索最近玩家设备
                SeekTarget_Helper.nearest_player_device(cell, out cell.target);
                if (cell.target != null) return;

                //规则：如果全部损坏，转火车体
                SeekTarget_Helper.select_caravan(out cell.target);
            }
            #endregion
        }
        #endregion


        #region Hover
        public void start_Hover(Enemy cell)
        {
            m_flyFollow_pos = cell.pos - cell.target.Position;
            can_attack = true;
        }


        public void do_Hover(Enemy cell)
        {
            i_flyFollow.notify_on_tick(cell, cell.target.Position);

            //转火
            if (cell.target.hp <= 0)
            {
                seek_target();

                #region 子函数 seek_target
                void seek_target()
                {
                    //规则：索最近玩家设备
                    SeekTarget_Helper.nearest_player_device(cell, out cell.target);
                    if (cell.target != null) return;

                    //规则：如果全部损坏，转火车体
                    SeekTarget_Helper.select_caravan(out cell.target);
                }
                #endregion
            }
        }
        #endregion


        public override void notify_on_dead(Enemy cell)
        {
            foreach (var sub_cell in sub_cells)
            {
                sub_cell.hp = -1;
            }

            base.notify_on_dead(cell);
        }

    }
}

