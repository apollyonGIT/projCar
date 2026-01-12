using Commons;
using Foundations;
using Foundations.MVVM;
using Foundations.Tickers;
using System.Collections.Generic;
using UnityEngine;
using World.Audio;
using World.BackPack;
using World.Helpers;

namespace World.Loots
{
    public interface ILootMgrView : IModelView<LootMgr>
    {
        void instantiate_loot(Loot loot);
        void destroy_loot(Loot loot);
        void update_loots();
    }

    public class LootMgr : Model<LootMgr, ILootMgrView>, IMgr
    {
        public List<Loot> lootList = new List<Loot>();

        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        const float LOOT_SPEED = 3F;
        const float SPEED_LOST = 0.98F;

        public void InstantiateLoot(Loot loot)
        {
            lootList.Add(loot);

            foreach (var view in views)
            {
                view.instantiate_loot(loot);
            }
        }

        public void DestroyLoot(Loot loot)
        {
            lootList.Remove(loot);

            foreach (var view in views)
            {
                view.destroy_loot(loot);
            }
        }

        public void tick()
        {
            foreach (var loot in lootList)
            {
                if (WorldContext.instance.is_need_reset)
                    loot.pos.x -= WorldContext.instance.reset_dis;
                Vector2 acc = (WorldContext.instance.caravan_pos - loot.pos).normalized * Config.current.loot_attraction_power;
                acc.y += Config.current.gravity;
                loot.velocity += acc * Config.PHYSICS_TICK_DELTA_TIME;
                loot.velocity += (WorldContext.instance.caravan_pos - loot.pos).normalized * Config.PHYSICS_TICK_DELTA_TIME * LOOT_SPEED;
                loot.velocity *= SPEED_LOST;
                loot.pos += (loot.velocity + new Vector2(WorldContext.instance.caravan_velocity.x, 0)) * Config.PHYSICS_TICK_DELTA_TIME;

                var road_height = Road_Info_Helper.try_get_altitude(loot.pos.x);
                if (road_height >= loot.pos.y)
                {
                    Road_Info_Helper.try_get_slope(loot.pos.x, out var slope);
                    var ground_slope = new Vector2(1f, slope);
                    var ground_normal = new Vector2(-slope, 1f);

                    var v_landing_parall = Vector2.Dot(loot.velocity, ground_slope) * ground_slope;
                    var v_landing_vertical = Vector2.Dot(loot.velocity, ground_normal) * ground_normal;

                    var vtm = v_landing_vertical.magnitude;
                    var vpm = v_landing_parall.magnitude;

                    v_landing_vertical *= -Config.current.coin_surface_bounce_coef;
                    if (vpm != 0)
                    {
                        v_landing_parall *= (vpm - Mathf.Min(vpm, vtm * (1 + Config.current.coin_surface_bounce_coef) * Config.current.surface_friction)) / vpm;
                    }

                    loot.velocity = v_landing_vertical + v_landing_parall;
                    loot.pos = new Vector2(loot.pos.x, road_height);
                }

                if (!BattleUtility.check_pos_in_screen(new Vector3(loot.pos.x, loot.pos.y, 10f)))
                {
                    var dir = ((Vector2)WorldSceneRoot.instance.mainCamera.transform.position - loot.pos).normalized;
                    loot.velocity += dir * WorldContext.instance.caravan_velocity * Config.current.caravan_vel2_screen_bounce;
                }
            }

            for (int i = lootList.Count - 1; i >= 0; i--)
            {
                if (Vector2.Distance(lootList[i].pos, WorldContext.instance.caravan_pos) <= Config.current.loot_pickup_distance)
                {
                    AudioSystem.instance.PlayClip(Config.current.SE_pick_up_item,false);

                    Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
                    bmgr.AddLoot(lootList[i]);

                    DestroyLoot(lootList[i]);
                }
            }

            foreach (var view in views)
            {
                view.update_loots();
            }
        }
        public void tick1()
        {

        }

        void IMgr.init(params object[] args)
        {
            Mission.instance.attach_mgr(m_mgr_name, this);

            var ticker = Ticker.instance;
            ticker.add_tick(m_mgr_priority, m_mgr_name, tick);
            ticker.add_tick1(m_mgr_priority, m_mgr_name, tick1);
        }

        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Ticker.instance;
            ticker.remove_tick(m_mgr_name);
            ticker.remove_tick1(m_mgr_name);
        }

        public LootMgr(string name, int proirity, params object[] args)
        {
            m_mgr_name = name;
            m_mgr_priority = proirity;

            (this as IMgr).init(args);
        }
    }
}
