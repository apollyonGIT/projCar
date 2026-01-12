using Commons;
using Foundations;
using System.Reflection;
using UnityEngine;

namespace World.Enemys
{
    public enum EN_enemy_move_type
    {
        None,
        Fly,
        Fly1, //新飞行
        Hover,
        Slide,
        Follow, //幽浮
        Walk //步行
    }


    public class EnemyMover
    {
        public EN_enemy_move_type move_type;
        public EN_enemy_move_type old_move_type;

        static Assembly m_assembly = Assembly.Load("World");
        public EnemyMgr mgr;
        public Enemy cell;

        //==================================================================================================

        public EnemyMover(Enemy cell)
        {
            Mission.instance.try_get_mgr("EnemyMgr", out mgr);
            move_type = EN_enemy_move_type.None;

            this.cell = cell;
        }


        public void move()
        {
            if (old_move_type != move_type)
            {
                m_assembly.GetType($"World.Enemys.Mover.EnemyMover_{move_type}").GetMethod("enter")?.Invoke(null, new object[] { cell });
                old_move_type = move_type;
            }

            m_assembly.GetType($"World.Enemys.Mover.EnemyMover_{move_type}").GetMethod("do")?.Invoke(null, new object[] { cell });
        }


        public void jump(float height_expt, float vx_jump)
        {
            if (move_type == EN_enemy_move_type.Slide)
                move_type = EN_enemy_move_type.Hover;

            cell.velocity = new(vx_jump, Mathf.Sqrt(2 * Config.current.gravity * height_expt));
        }


        public void impact(params object[] prms)
        {
            var impact_source_type = (WorldEnum.impact_source_type)prms[0];

            //1.计算击退方向dir
            Vector2 dir = new();

            switch (impact_source_type)
            {
                case WorldEnum.impact_source_type.projectile:
                    var proj_v = (Vector2)prms[1];
                    dir = proj_v.normalized;
                    break;

                case WorldEnum.impact_source_type.melee:
                    var pos_atk = (Vector2)prms[1];
                    var pos_def = (Vector2)prms[2];
                    var dir_a2d = pos_def - pos_atk;
                    dir = dir_a2d.normalized;
                    break;
            }

            //2.获取本次攻击的冲量ft
            //temp:冲量
            float ft;
            switch (impact_source_type)
            {
                case WorldEnum.impact_source_type.projectile:
                    var proj_v = (Vector2)prms[1];
                    var ammo_mass = (float)prms[2];
                    var penetration_loss = (float)prms[3];
                    var delta_v_mag = (proj_v - cell.velocity).magnitude;
                    ft = Mathf.Min(ammo_mass * delta_v_mag * penetration_loss * 0.25f, proj_v.magnitude * cell.mass_total);   //不科学的冲量，这里为了表现，牺牲一下科学
                    break;

                case WorldEnum.impact_source_type.melee:
                    ft = (float)prms[3];
                    ft += ft * cell.mass_self * 0.5f;
                    break;

                default:
                    ft = 0;
                    break;
            }

            //3.记录受击怪物的下列数据
            var mass_total = cell.mass_total;
            var delta_v = ft / mass_total * dir;

            ref var monster_v = ref cell.velocity;

            //4.击飞判断
            ref var move_type = ref cell.mover.move_type;
            if (move_type == EN_enemy_move_type.Slide || move_type == EN_enemy_move_type.Walk)
            {
                var delta_v_g = Config.current.gravity * Vector2.down * Config.PHYSICS_TICK_DELTA_TIME;
                var knock_air_threshold_factor = Mathf.Max(Config.current.knock_air_threshold_factor, 0);

                if (delta_v.y > 0 && Mathf.Abs(delta_v.y) - Mathf.Abs(delta_v_g.y) * knock_air_threshold_factor > 0)
                {
                    move_type = EN_enemy_move_type.Hover;
                    Enemy_Buff_Helper.buff_flied(cell);
                }
            }

            //5.使怪物被击退
            monster_v += delta_v;
        }
    }
}

