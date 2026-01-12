using Foundations;
using System.Collections.Generic;
using World;

namespace World.VFXs.Enemy_Death_VFXs
{
    public class Enemy_Death_VFXMgr : IMgr
    {
        public Enemy_Death_VFXPD pd;

        LinkedList<Enemy_Death_VFX> m_cells = new();
        public LinkedList<Enemy_Death_VFX> fini_cells = new();

        WorldContext ctx;

        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        //==================================================================================================

        public Enemy_Death_VFXMgr(string name, int priority, params object[] args)
        {
            m_mgr_name = name;
            m_mgr_priority = priority;

            (this as IMgr).init(args);
        }


        void IMgr.init(params object[] args)
        {
            Mission.instance.attach_mgr(m_mgr_name, this);

            var ticker = Foundations.Tickers.Ticker.instance;
            ticker.add_tick(m_mgr_priority, m_mgr_name, tick);
            ticker.add_tick1(m_mgr_priority, m_mgr_name, tick1);

            ctx = WorldContext.instance;
        }


        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Foundations.Tickers.Ticker.instance;
            ticker.remove_tick(m_mgr_name);
            ticker.remove_tick1(m_mgr_name);
        }


        void tick()
        {
            foreach (var cell in m_cells)
            {
                if (ctx.is_need_reset)
                {
                    cell.pos.x -= ctx.reset_dis;
                }
            }

            foreach (var cell in fini_cells)
            {
                m_cells.Remove(cell);
                cell.remove_all_views();
            }
            fini_cells.Clear();
        }


        void tick1()
        {
            foreach (var cell in m_cells)
            {
                cell.tick1();
            }
        }


        public void add_cell(Enemy_Death_VFX cell)
        {
            m_cells.AddLast(cell);
        }
    }
}