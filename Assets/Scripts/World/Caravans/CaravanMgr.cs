using Foundations;
using Foundations.Tickers;
using UnityEngine;

namespace World.Caravans
{
    public class CaravanMgr : IMgr
    {
        public Caravan cell;

        public WorldContext ctx = WorldContext.instance;

        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        //==================================================================================================

        public CaravanMgr(string name, int priority, params object[] args)
        {
            m_mgr_name = name;
            m_mgr_priority = priority;

            (this as IMgr).init(args);

            //加载运动系统
            CaravanMover.init();
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


        void tick()
        {
            ref var caravan_pos = ref ctx.caravan_pos;

            if (ctx.is_need_reset)
            {
                caravan_pos.x -= ctx.reset_dis;
            }

            var temp_pos = caravan_pos;

            CaravanMover.move();
            CaravanMover.calc_and_set_caravan_leap_rad();

            ctx.caravan_move_delta_dis = caravan_pos.x - temp_pos.x;

            cell.tick();
        }


        void tick1()
        {
            cell.tick1();
        }
    }
}