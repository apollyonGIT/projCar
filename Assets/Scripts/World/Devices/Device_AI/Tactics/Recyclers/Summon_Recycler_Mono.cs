using UnityEngine;
using World.Helpers;

namespace World.Devices
{
    public class Summon_Recycler_Mono : Summon_Mono
    {
        const float C_recycle_radius = 1f;
        const float C_add_money_ratio = 0.5f;

        //==================================================================================================

        public override void do_bll_tick()
        {
            do_recycle();
        }


        public void do_recycle()
        {
            BoardCast_Helper.to_all_projectile(recycle);

            #region 子函数 recycle
            void recycle(ITarget target)
            {
                var dis = Vector2.Distance(target.Position, m_pos);
                if (dis >= C_recycle_radius) return;  //回收范围

                var _owner = owner as Recycler;

                if (target is Projectiles.Projectile p)
                {
                    p.RemoveSelf();

                    var random = Random.Range(0f, 1f);
                    if (random <= C_add_money_ratio)
                        _owner.add_money(transform.position);
                }
            }
            #endregion
        }
    }
}

