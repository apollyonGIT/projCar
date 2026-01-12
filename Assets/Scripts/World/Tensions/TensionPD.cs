using Foundations;

namespace World.Tensions
{
    public class TensionPD : Producer
    {
        public override IMgr imgr => mgr;
        TensionMgr mgr;

        //==================================================================================================

        public override void init(int priority)
        {
            mgr = new("TensionMgr", priority);
        }


        public override void call()
        {
        }
    }
}