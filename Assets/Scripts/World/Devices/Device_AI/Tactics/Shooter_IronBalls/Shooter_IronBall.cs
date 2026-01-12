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
    public class Shooter_IronBall : NewBasicShooter
    {
        const int C_ironBall_survive_tick = 2 * Config.PHYSICS_TICKS_PER_SECOND;

        IronBall_View buff_head_ironBall_model => get_ironBall_img_model();
        IronBall_View m_buff_head_ironBall_model;

        //==================================================================================================

        public override void projectile_hit_callback(ITarget target)
        {
            if (target.buff_flags.ContainsKey("ironBall")) return;

            var ironBall_view = show_ironBall_view(target);

            LinkedList<Action<ITarget>> actions = new();
            actions.AddLast(do_ironBall);

            target.buff_flags.Add("ironBall", actions);
            target.fini_callback_event += target_fini_callback_event;

            Request_Helper.delay_do("cancel_ironBall", C_ironBall_survive_tick,
                    (_) => {
                        target.fini_buffs.Add("ironBall");
                        target.fini_callback_event -= target_fini_callback_event;

                        if (ironBall_view == null) return;
                        UnityEngine.Object.DestroyImmediate(ironBall_view.gameObject);
                    }
            );

            #region 子函数 do_ironBall
            void do_ironBall(ITarget target)
            {
                target.impact(WorldEnum.impact_source_type.melee, target.Position + Vector2.down, target.Position, -20f * Config.PHYSICS_TICK_DELTA_TIME);
                
                ironBall_view.tick(target);
            }
            #endregion

            #region 子函数 target_fini_callback_event
            void target_fini_callback_event(ITarget target)
            {
                if (ironBall_view == null) return;
                UnityEngine.Object.DestroyImmediate(ironBall_view.gameObject);
            }
            #endregion
        }


        public IronBall_View get_ironBall_img_model()
        {
            if (m_buff_head_ironBall_model != null)
                return m_buff_head_ironBall_model;

            Addrs.Addressable_Utility.try_load_asset("buff_head_ironBall", out IronBall_View model);
            m_buff_head_ironBall_model = model;
            return model;
        }


        IronBall_View show_ironBall_view(ITarget target)
        {
            if (target is not Enemy enemy) return null;
            
            var enemy_view = (EnemyView)enemy.views.Where(t => t is EnemyView).First();
            var ironBall_view = UnityEngine.Object.Instantiate(buff_head_ironBall_model, enemy_view.transform.parent);

            return ironBall_view;
        }
    }
}

