using Foundations;
using World.BackPack;
using World.Devices;
using World.Devices.DeviceUpgrades;

namespace World.Helpers
{
    public class Upgrade_Cost_Helper
    {
        public static int GetLootAmount(int loot_id)
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
            return bmgr.GetLootAmount(loot_id);
        }

        public static bool CheckUpgradeCost(DeviceUpgrade upgrade)
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
            foreach(var cost in upgrade.desc.cost)
            {
                var amount = bmgr.GetLootAmount((int)cost.Key);
                if (amount < cost.Value)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 消耗材料并且升级，此函数默认能满足消耗
        /// </summary>
        /// <param name="upgrade"></param>
        public static void UpgradeCost(DeviceUpgrade upgrade,Device device)       
        {
            Mission.instance.try_get_mgr("BackPack", out BackPackMgr bmgr);
            foreach (var cost in upgrade.desc.cost)
            {
                bmgr.RemoveLoot((int)cost.Key, cost.Value);
            }

            upgrade.ApplyUpgrade(device);
        }
    }
}
