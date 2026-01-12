using Commons;
using Foundations;
using World.Environments.Roads;

namespace World.Environments
{
    public class EnvironmentPD : Producer
    {
        public EnvironmentMgrView ev;
        public RoadMgrView rv;

        public override IMgr imgr => mgr;
        EnvironmentMgr mgr;

        public override void init(int priority)
        {
            mgr = new(Config.EnvironmentMgr_Name, priority);
            mgr.add_view(ev);
            mgr.roadMgr.add_view(rv);

            var ctx = WorldContext.instance;
            mgr.Init(ctx.r_scene.id);
        }


        public override void call()
        {

        }
    }
}
