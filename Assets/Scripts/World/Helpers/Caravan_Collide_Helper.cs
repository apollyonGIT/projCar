using Commons;
using UnityEngine;
using World.Caravans;
using World.Devices.Device_AI;
using World.Enemys;

namespace World.Helpers
{
    public class Caravan_Collide_Helper
    {
        public static void crush_to_enemy(ITarget t_caravan ,ITarget t)
        {
            if (t is not Enemy cell) return;
            if (cell.is_ban_select) return;

            //1.车体接触
            ref var v_car = ref WorldContext.instance.caravan_velocity;
            var v_target = t.velocity;
            var v_relative = v_car - v_target;

            //规则：此时为擦身而过
            if (!cell._desc.is_block_car || v_relative.x <= 0) return;

            //2.车轮碾压
            var car_weight_total = WorldContext.instance.total_weight;

            var wheel = Device_Slot_Helper.GetDevice("device_slot_wheel");
            var wheel_desc = (Device_Slot_Helper.GetDevice("device_slot_wheel") as BasicWheel).wheel_desc;

            var radius = wheel_desc.crush_radius;
            var crush_level = wheel_desc.crush_level;
            var crush_v_mod = wheel_desc.crush_v_mod;
            var crush_atk = wheel_desc.crush_atk;
            var crush_jump_coef = wheel_desc.crush_jump_coef;
            var crush_ft = wheel_desc.crush_ft;

            var target_weight_total = t.Mass;

            var condition_1 = Vector2.Distance(t.Position, wheel.position) < radius;

            bool condition_2;
            var condition_2_1 = (t as Enemy).is_unconsious;
            var condition_2_2 = car_weight_total * 0.001f * crush_level + v_relative.x * crush_v_mod >= target_weight_total;
            condition_2 = condition_2_1 || condition_2_2;

            //规则：满足车轮碾压
            if (condition_1 && condition_2)
            {
                //小车跳起
                var jump_vy = Mathf.Min(3.5f, v_car.x * crush_jump_coef);
                CaravanMover.do_jump_input_vy(jump_vy);

                //对被碾压的怪物造成伤害与击退
                var EK = car_weight_total * 0.001f * v_relative.SqrMagnitude() * 0.01f;

                if (crush_atk != null)
                {
                    foreach (var (crush_atk_type, crush_atk_value) in crush_atk)
                    {
                        Attack_Data atk_data = new()
                        {
                            diy_atk_str = crush_atk_type,
                            atk = Mathf.CeilToInt(EK * crush_atk_value)
                        };
                        t.hurt(t_caravan, atk_data, out _);
                    }
                }

                t.impact(WorldEnum.impact_source_type.melee, wheel.position, t.Position, EK * crush_ft);

                return;
            }

            //3.轻微碰撞
            ref var lever_value = ref WorldContext.instance.driving_lever;

            condition_1 = lever_value > 0.5f;
            condition_2 = v_car.x > Config.current.car_body_collision_vx;

            //规则：满足轻微碰撞
            if (condition_1 && condition_2)
            {
                //小车速度损失
                var cw = car_weight_total;
                var tw = target_weight_total;

                v_car *= Mathf.Exp(-tw * Config.current.car_collision_loss_vel / cw);

                //小车油门挡位降低
                lever_value *= Mathf.Exp(-tw * Config.current.car_collision_loss_acc / cw);
            }
        }
    }
}

