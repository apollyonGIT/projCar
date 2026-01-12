using Foundations;

namespace World.Lenses
{
    public class LensPD : Producer
    {
        public override IMgr imgr => mgr;
        LensMgr mgr;

        //==================================================================================================

        public override void init(int priority)
        {
            mgr = new("LensMgr", priority);
            mgr.Init();
        }


        public override void call()
        {
        }
    }
}