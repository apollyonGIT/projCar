using Foundations;

namespace Camp.Techs
{
    public class TechMgr : IMgr
    {
        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        public Tech cell;

        //==================================================================================================

        public TechMgr(string name, int priority, params object[] args)
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
            cell.tick();
        }


        void tick1()
        {
            cell.tick1();
        }


        public void show_info(int id)
        {
            AutoCodes.techs.TryGetValue($"{id}", out var _desc);
            cell._desc = _desc;

            foreach (var view in cell.views)
            {
                view.notify_on_show_info();
            }
        }
    }
}