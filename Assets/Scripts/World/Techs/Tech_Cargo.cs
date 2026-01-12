using Foundations;
using World.Helpers;

namespace World.Techs
{
    public class Tech_Cargo
    {
        public static void @do(int num)
        {
            Mission.instance.try_get_mgr("BackPack", out BackPack.BackPackMgr mgr);
            mgr.AddSlot(num);
        }
    }
}