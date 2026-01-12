using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public interface IEnemy_BT_Jump
    {
        float jump_velocity_x_max{ get; }
        Vector2 jump_height_area { get; }

        //==================================================================================================

        public void @do(Enemy cell)
        {
            var self_pos = cell.pos;
            var car_speed = WorldContext.instance.caravan_velocity.x;
            var target_position_start = cell.target.Position;

            var h_up = Random.Range(jump_height_area.x, jump_height_area.y);
            var t_up = Mathf.Pow(2 * h_up / -Config.current.gravity, 0.5f);

            var y_distance = target_position_start.y - Road_Info_Helper.try_get_altitude(target_position_start.x);
            var h_down = Mathf.Max(0, h_up + self_pos.y - target_position_start.y);

            var jump_iterate_time = Config.current.jump_iterate_time;
            Vector2 predicted_pos = new();
            var t_total = 0f;

            for (int i = 0; i <= jump_iterate_time; i++)
            {
                t_total = Mathf.Pow(2 * h_down / -Config.current.gravity, 0.5f) + t_up;
                
                predicted_pos.x = target_position_start.x + car_speed * t_total;
                predicted_pos.y = Road_Info_Helper.try_get_altitude(predicted_pos.x) + y_distance;

                h_down = Mathf.Max(0, h_up + self_pos.y - predicted_pos.y);
            }

            var d = predicted_pos.x - self_pos.x;

            var speed_x_abs = Mathf.Abs(d / t_total);
            var sign = d > 0 ? 1 : -1;

            Vector2 jump_velocity = new()
            {
                x = Mathf.Min(jump_velocity_x_max, speed_x_abs) * sign,
                y = Mathf.Abs(Config.current.gravity) * t_up
            };

            cell.mover.move_type = EN_enemy_move_type.Hover;

            cell.velocity += jump_velocity;
            cell.pos += cell.velocity * Config.PHYSICS_TICK_DELTA_TIME;
        }


        public bool is_land(Enemy cell)
        {
            ref var pos = ref cell.pos;
            var ground_y = Road_Info_Helper.try_get_altitude(pos.x);

            if (pos.y > ground_y) return false;
            //if (cell.mover.move_type != EN_enemy_move_type.Hover) return false;

            cell.pos.y = ground_y;
            cell.mover.move_type = EN_enemy_move_type.Slide;
            return true;
        }
    }
}

