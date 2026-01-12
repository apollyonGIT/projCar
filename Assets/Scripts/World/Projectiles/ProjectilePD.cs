using Commons;
using Foundations;

namespace World.Projectiles
{
    public class ProjectilePD : Producer
    {
        public override IMgr imgr => mgr;
        ProjectileMgr mgr;

        public ProjectileMgrView pv;

        //==================================================================================================

        public override void init(int priority)
        {
            mgr = new(Config.ProjectileMgr_Name, priority);
            
            mgr.add_view(pv);
            
            mgr.Init();
        }


        public override void call()
        {
        }
    }
}