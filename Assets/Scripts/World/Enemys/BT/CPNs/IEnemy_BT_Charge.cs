using Foundations.Tickers;
using System;
using UnityEngine;

namespace World.Enemys.BT
{
    public interface IEnemy_BT_Charge<T> where T : Enum
    {
        public float charge_dis_min { get; }
        public float charge_dis_max { get; }
        public float charge_tangent_limit { get; }

        public int charge_cd_min { get; }
        public int charge_cd_max { get; }
        public int charge_lasting_tick { get; }

        public int charge_cd { get; set; }
        public bool charge_dir { get; set; }
        public int charge_dir_flag => charge_dir ? 1 : -1;

        public T state { get; set; }
        public T charge_next_state { get; }
        public T charge_state { get; }

        //==================================================================================================

        void try_charge(Enemy cell)
        {
            charge_cd--;
            charge_cd = Mathf.Max(charge_cd, 0);

            if (charge_cd == 0)
            {
                //冲刺检测：自身与目标的距离，在设定区间内时，可释放冲刺
                var dis_x = Mathf.Abs(cell.pos.x - cell.target.Position.x);
                bool is_in_charge_dis = dis_x > charge_dis_min && dis_x < charge_dis_max;

                //冲刺检测：自身高度，不超过冲刺高度时，可释放冲刺
                var charge_y_limit = cell.target.Position.y + dis_x * charge_tangent_limit;
                bool is_in_charge_pos_y = cell.pos.y < charge_y_limit;

                if (is_in_charge_dis && is_in_charge_pos_y)
                {
                    state = charge_state;
                }
            }
        }


        void start_charge(Enemy cell)
        {
            charge_cd = UnityEngine.Random.Range(charge_cd_min, charge_cd_max);
            charge_dir = cell.target.Position.x >= cell.pos.x;

            Request_Helper.delay_do($"back_to_state_{cell.GUID}", charge_lasting_tick, (_) =>
            {
                if (state.ToString() == "Charge")
                    state = charge_next_state;
            });
        }


        public void do_charge(Enemy cell)
        {
            var current_charge_dir = cell.target.Position.x >= cell.pos.x;
            if (current_charge_dir == charge_dir)
            {
                cell.position_expt = cell.target.Position + new Vector2(0, 1f);
            }
            else
            {
                cell.position_expt = new(cell.target.Position.x + charge_dir_flag * 5f, 5f);

                if (cell.velocity.x == 0)
                    state = charge_next_state;
            }
        }


        public void end_charge(Enemy cell)
        {
            Request_Helper.del_requests($"back_to_state_{cell.GUID}");
        }

    }
}

