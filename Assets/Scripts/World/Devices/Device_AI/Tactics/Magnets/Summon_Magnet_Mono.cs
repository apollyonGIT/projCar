using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Devices
{
    public class Summon_Magnet_Mono : Summon_Mono
    {
        const float C_pull_radius = 10f;

        //==================================================================================================

        public override void do_bll_tick()
        {
            do_pull_force();
        }


        public void do_pull_force()
        {
            BoardCast_Helper.to_all_projectile(pull_force);

            #region 子函数 pull_force
            void pull_force(ITarget target)
            {
                var dis = Vector2.Distance(target.Position, m_pos);
                if (dis >= C_pull_radius) return;  //吸力范围

                target.impact(WorldEnum.impact_source_type.melee, m_pos, target.Position, -20f * Config.PHYSICS_TICK_DELTA_TIME);
            }
            #endregion
        }
    }
}

