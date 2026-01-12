using Commons;
using Foundations;
using Foundations.Tickers;
using UnityEngine;

namespace World.Lenses
{
    public class LensMgr : IMgr
    {
        WorldContext ctx = WorldContext.instance;

        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        //==================================================================================================

        public LensMgr(string name, int priority, params object[] args)
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
        }


        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Ticker.instance;
            ticker.remove_tick(m_mgr_name);
        }

        void tick()
        {
            if (Config.current.free_camera)
                return;

            //var cx = ctx.caravan_pos.x;
            var cx = ctx.caravan_pos.x <= 0 ? 0 : ctx.caravan_pos.x;
            var cy = Mathf.Clamp(ctx.caravan_pos.y * 0.08f, -0.1f, 1f) + Config.current.cam_pos_offset_y;
            var cz = Config.current.cam_pos_offset_z;

            Vector3 camera_pos = new(cx, cy, cz);
            WorldSceneRoot.instance.mainCamera.transform.localPosition = camera_pos;
        }

        public void Init()
        {
            var cx = ctx.caravan_pos.x;
            var cy = Mathf.Clamp(ctx.caravan_pos.y * 0.08f, -0.1f, 1f) + Config.current.cam_pos_offset_y;
            var cz = Config.current.cam_pos_offset_z;

            Vector3 camera_pos = new Vector3(cx, cy, cz);

            WorldSceneRoot.instance.mainCamera.transform.localPosition = camera_pos;
        }
    }
}