using UnityEngine;

namespace World.Enemys.BT
{
    public class Enemy_BT_Speed_CPN
    {

        //==================================================================================================

        public static void set_chasing_speed(Enemy cell, float v_x, float v_x_max_limit)
        {
            var target_v_x = cell.target.velocity.x;

            //规则：目标速度小于设定速度，使用设定速度
            bool is_target_v_slower = target_v_x <= v_x;

            //规则：自身位置在追逐点前方，使用设定速度
            var chasing_expt_pos_x = cell.target.Position.x + 10f;
            bool is_target_pos_front = cell.pos.x >= chasing_expt_pos_x;

            if (is_target_v_slower || is_target_pos_front)
            {
                cell.speed_expt = Mathf.Clamp(v_x, 0, v_x_max_limit);
                return;
            }

            //规则：根据滞后距离，使用线性插值计算速度 y-y1 = (y2-y1)/(x2-x1) * (x-x1)
            var y1 = v_x;
            var y2 = target_v_x;
            var x = cell.pos.x;
            var x1 = chasing_expt_pos_x;
            var x2 = cell.target.Position.x;

            var y = (y2 - y1) / (x2 - x1) * (x - x1) + y1;
            cell.speed_expt = Mathf.Clamp(y, 0, v_x_max_limit);
        }
    }
}

