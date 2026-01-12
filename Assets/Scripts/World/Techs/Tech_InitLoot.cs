using World.Helpers;

namespace World.Techs
{
    public class Tech_InitLoot
    {
        public static void @do(int loot_id, int num)
        {
            for (int i = 0; i < num; i++)
            {
                Drop_Loot_Helper.drop_loot((uint)loot_id);
            }
        }
    }
}