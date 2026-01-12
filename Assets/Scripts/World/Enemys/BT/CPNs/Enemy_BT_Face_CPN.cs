using UnityEngine;

namespace World.Enemys.BT
{
    public class Enemy_BT_Face_CPN
    {

        //==================================================================================================

        public static void set_face_dir_by_velocity_only_x(Enemy cell)
        {
            //规则：只设定左右朝向，右正左负，与速度一致
            cell.dir.x = cell.velocity.x;
        }


        public static void set_face_dir_by_velocity(Enemy cell)
        {
            cell.dir = cell.velocity;
        }


        public static void set_face_dir_to_player_only_x(Enemy cell)
        {
            //规则：只设定左右朝向，怪物朝向玩家
            cell.dir.x = (WorldContext.instance.caravan_pos - cell.pos).x;
        }
    }
}

