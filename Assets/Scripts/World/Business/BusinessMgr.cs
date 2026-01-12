using Foundations;
using Foundations.Tickers;
using System.Collections.Generic;

namespace World.Business
{
    public class BusinessMgr : IMgr
    {
        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        public List<Business> business_list = new();    

        public BusinessMgr(string name, int priority, params object[] args)
        {
            m_mgr_name = name;
            m_mgr_priority = priority;

            (this as IMgr).init(args);
        }

        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Ticker.instance;
            ticker.remove_tick(m_mgr_name);
            ticker.remove_tick1(m_mgr_name);
        }

        void IMgr.init(params object[] args)
        {
            Mission.instance.attach_mgr(m_mgr_name, this);

            var ticker = Ticker.instance;
            ticker.add_tick(m_mgr_priority, m_mgr_name, tick);
            ticker.add_tick1(m_mgr_priority, m_mgr_name, tick1);
        }

        void tick()
        {
            
        }

        void tick1()
        {
            
        }

        public void AddBusiness(Business business)
        {
            business_list.Add(business);
        }

        public void RemoveBusiness(Business business)
        {
            business_list.Remove(business);
            business.remove_all_views();
        }
    }
}
