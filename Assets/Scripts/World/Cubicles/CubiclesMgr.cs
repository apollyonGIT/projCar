using Foundations;
using Foundations.Tickers;
using System.Collections.Generic;

namespace World.Cubicles
{

    public class CubiclesMgr : IMgr
    {
        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        public List<BasicCubicle> cubicles = new List<BasicCubicle>();
        public CubiclesMgr(string name,int priority,params object[] args)
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
            ticker.add_tick(m_mgr_priority,m_mgr_name,tick);
            ticker.add_tick1(m_mgr_priority,m_mgr_name, tick1);
        }

        public void tick()
        {
            foreach(var cub in cubicles)
            {
                cub.tick();
            }
        }

        public void tick1()
        {

        }

        public void AddCubicle(BasicCubicle cub)
        {
            cubicles.Add(cub);
        }

        public void HighLightCubicles(Characters.Character  c)
        {
            foreach(var cub in cubicles)
            {
                if (cub is DeviceCubicle device_cubicle)
                {
                    if (device_cubicle != null)
                    {
                        if (c.CanWork(device_cubicle.device))
                        {
                            cub.HighlightCubicle(true);
                        }
                    }
                }
                else
                {
                    cub.HighlightCubicle(false);
                }
            }
        }

        public void CancelHightLightCubicles()
        {
            foreach (var cub in cubicles)
            {
                cub.HighlightCubicle(false);
            }
        }
    }
}