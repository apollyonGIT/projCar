using Foundations;

namespace World.Loots
{
    public class LootPD : Producer
    {
        public override IMgr imgr => mgr;
        LootMgr mgr;

        public LootMgrView cv;
        public override void call()
        {
            
        }

        public override void init(int priority)
        {
            mgr = new("Loot", priority);
            mgr.add_view(cv);
        }
    }
}
