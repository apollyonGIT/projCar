using UnityEngine;

namespace World.Enemys.BT
{
    public class Enemy_BT_VFX_CPN
    {

        //==================================================================================================

        public static void set_death_vfx(Enemy cell, int index)
        {
            cell.death_vfx_index = index;
        }


        public static void set_death_vfx(Enemy cell)
        {
            //规则：常规死亡特效，固定配置4个，从中选取一个
            var index = Random.Range(0, 4);

            set_death_vfx(cell, index);
        }


        public static void set_explosion_death_vfx(Enemy cell)
        {
            //规则：爆炸死亡特效，固定为第5个
            var index = 4;

            set_death_vfx(cell, index);
        }
    }
}

