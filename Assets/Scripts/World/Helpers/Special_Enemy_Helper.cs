using Foundations;
using World.Enemys;

namespace World.Helpers
{
    public class Special_Enemy_Helper
    {
        public static void add_enemy_car_by_encounter(uint drop_relic_id)
        {
            if (!Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr mgr)) return;

            var cell = mgr.pd.create_single_cell_directly(211101u, new(1, 0), "None");
            cell.is_drop_relic = true;
            cell.drop_relic_id = drop_relic_id;
        }
    }
}

