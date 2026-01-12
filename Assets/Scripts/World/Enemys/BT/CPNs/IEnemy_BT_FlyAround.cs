using Commons;
using UnityEngine;

namespace World.Enemys.BT
{
    public interface IEnemy_BT_FlyAround
    {
        public Vector2 flyAround_deg { get; }
        public Vector2 flyAround_radius { get; }
        public Vector2 flyAround_pos { get; set; }

        public int flyAround_count { get; set; }

        //==================================================================================================

        void calc_flyAround_pos()
        {
            var radius = Random.Range(flyAround_radius.x, flyAround_radius.y);
            var deg = Random.Range(flyAround_deg.x, flyAround_deg.y);
            var rad = deg * Mathf.Deg2Rad;

            var angle_fix = Mathf.Lerp(Config.current.flyAround_radius_ratio, 1f, Mathf.Abs(deg - 90f) / 90f);
            flyAround_pos = angle_fix * radius * new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }


        void flyAround_on_tick(Enemy cell, Vector2 target_pos)
        {
            cell.position_expt = flyAround_pos + target_pos;

            var dis = Vector2.Distance(cell.pos, cell.position_expt);
            if (dis < 1f)
            {
                calc_flyAround_pos();

                //规则：达到目标，视为一次盘旋
                flyAround_count++;
            }
                
        }
    }
}

