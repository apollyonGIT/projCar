using Commons;
using Foundations;
using Foundations.Tickers;

namespace World.Progresss
{
    public class ProgressMgr : IMgr
    {
        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        //==================================================================================================

        public Progress progress;
        public ProgressMgr(string name, int priority, params object[] args)
        {
            m_mgr_name = name;
            m_mgr_priority = priority;

            (this as IMgr).init(args);
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
            AddProgress(WorldContext.instance.caravan_move_delta_dis);

            Share_DS.instance.try_get_value<float>(Game_Mgr.key_total_distance, out var total_dis);
            Share_DS.instance.add(Game_Mgr.key_total_distance,total_dis + WorldContext.instance.caravan_move_delta_dis);

            progress.tick();
        }
        void tick1()
        {
        }

        public void AddProgress(float dis)
        {
            progress.Move(dis);
        }

        public void Init()
        {
            var p = new Progress();
            progress = p;
        }
    }
}