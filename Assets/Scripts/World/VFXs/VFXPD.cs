using Foundations;

namespace World.VFXs
{
    public class VFXPD : Producer
    {
        public override IMgr imgr => mgr;
        VFXMgr mgr;
        public VFXMgrView view;

        public override void call()
        {
            
        }

        public override void init(int priority)
        {
            mgr = new VFXMgr("VFX", priority);
            mgr.add_view(view);
        }
    }
}
