using Commons;
using Foundations.Tickers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Devices.Device_AI;
using World.Enemys;

namespace World.Devices
{
    public class Shooter_Balloon : NewBasicShooter
    {
        const int C_balloon_survive_tick = 2 * Config.PHYSICS_TICKS_PER_SECOND;

        Balloon_View buff_head_balloon_model => get_balloon_img_model();
        Balloon_View m_buff_head_balloon_model;

        //==================================================================================================

        public override void projectile_hit_callback(ITarget target)
        {
            if (target.buff_flags.ContainsKey("balloon")) return;

            var ballon_view = show_balloon_view(target);

            LinkedList<Action<ITarget>> actions = new();
            actions.AddLast(do_balloon);

            target.buff_flags.Add("balloon", actions);
            target.fini_callback_event += target_fini_callback_event;

            Request_Helper.delay_do("cancel_balloon", C_balloon_survive_tick,
                    (_) => {
                        target.fini_buffs.Add("balloon");
                        target.fini_callback_event -= target_fini_callback_event;

                        if (ballon_view == null) return;
                        UnityEngine.Object.DestroyImmediate(ballon_view.gameObject);
                    }
            );

            #region 子函数 do_balloon
            void do_balloon(ITarget target)
            {
                target.impact(WorldEnum.impact_source_type.melee, target.Position + Vector2.up, target.Position, -10f * Config.PHYSICS_TICK_DELTA_TIME);
                
                ballon_view.tick(target);
            }
            #endregion

            #region 子函数 target_fini_callback_event
            void target_fini_callback_event(ITarget target)
            {
                if (ballon_view == null) return;
                UnityEngine.Object.DestroyImmediate(ballon_view.gameObject);
            }
            #endregion
        }


        public Balloon_View get_balloon_img_model()
        {
            if (m_buff_head_balloon_model != null)
                return m_buff_head_balloon_model;

            Addrs.Addressable_Utility.try_load_asset("buff_head_balloon", out Balloon_View model);
            m_buff_head_balloon_model = model;
            return model;
        }


        Balloon_View show_balloon_view(ITarget target)
        {
            if (target is not Enemy enemy) return null;
            
            var enemy_view = (EnemyView)enemy.views.Where(t => t is EnemyView).First();
            var ballon_view = UnityEngine.Object.Instantiate(buff_head_balloon_model, enemy_view.transform.parent);

            return ballon_view;
        }
    }
}

