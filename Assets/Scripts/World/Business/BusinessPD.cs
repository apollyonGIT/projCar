using Foundations;

namespace World.Business
{
    public class BusinessPD : Producer
    {
        public override IMgr imgr => mgr;
        BusinessMgr mgr;

        public override void call()
        {
            
        }

        public override void init(int priority)
        {
            mgr = new("BusinessMgr", priority);
        }
    }
}
