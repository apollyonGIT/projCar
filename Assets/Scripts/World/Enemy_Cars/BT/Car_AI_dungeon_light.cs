using UnityEngine;
using World.Caravans;
using World.Enemys;

namespace World.Enemy_Cars.BT
{
    public class Car_AI_dungeon_light : IEnemy_BT
    {
        EN_caravan_move_type m_state = EN_caravan_move_type.Run;

        string IEnemy_BT.state => $"{m_state}";

        Enemy_Car car_cell;

        float m_target_pos_x;
        float m_expt_pos_x => WorldContext.instance.caravan_pos.x + m_target_pos_x;

        float m_driving_lever_change_velocity = 0f;

        //==================================================================================================

        void IEnemy_BT.init(Enemy _cell, params object[] prms)
        {
            car_cell = _cell as Enemy_Car;

            calc_follow_pos_x();
        }


        void IEnemy_BT.tick(Enemy _)
        {
            var world_ctx = WorldContext.instance;
            if (world_ctx.is_need_reset) return;

            var ctx = car_cell.ctx;
            ref var driving_lever = ref ctx.driving_lever;
            var self_x = car_cell.pos.x;

            var expt_driving_lever = 1 * Mathf.Sign(m_expt_pos_x - self_x);
            driving_lever = Mathf.SmoothDamp(driving_lever, expt_driving_lever, ref m_driving_lever_change_velocity, 0.25f);

            if (Mathf.Abs(m_expt_pos_x - self_x) < 0.5f)
            {
                calc_follow_pos_x();
            }
        }


        void IEnemy_BT.notify_on_enter_die(Enemy _)
        {
            car_cell.fini();
        }


        void IEnemy_BT.notify_on_dying(Enemy cell)
        {
            Debug.Log("notify_on_dying");
        }


        void IEnemy_BT.notify_on_dead(Enemy cell)
        {
            Debug.Log("notify_on_dead");
        }


        void calc_follow_pos_x()
        {
            float radius_min = 2f;
            float radius_max = 5f;

            var radius = Random.Range(radius_min, radius_max);

            var deg = Random.value > 0.5f ? Random.Range(30, 70) : Random.Range(110, 150);
            var rad = deg * Mathf.Deg2Rad;

            //增幅
            var radius_nor = Mathf.Clamp01((radius - radius_min) / (radius_max - radius_min));
            var dis_modifier = Mathf.Lerp(0.6f, 1.2f, radius_nor);

            //扰动
            radius *= dis_modifier * Random.Range(0.9f, 1.1f);

            m_target_pos_x = Mathf.Cos(rad) * radius;
        }
    }
}

