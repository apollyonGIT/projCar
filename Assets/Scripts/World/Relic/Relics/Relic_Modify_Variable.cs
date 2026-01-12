using System.Collections.Generic;
using World.Devices;
using World.Helpers;

namespace World.Relic.Relics
{
    public class Relic_Modify_Variable :Relic
    {
        public override void Get()
        {
            modify_variable(desc.parm_string[0], desc.parm_int[0]);
        }

        public override void Drop()
        {
            modify_variable(desc.parm_string[0], -desc.parm_int[0]);
        }

        private void modify_variable(string variable_name,int delta)
        {
            List<Device> all_devices = new();

            switch (variable_name)
            {
                case "global_atk_pts":
                    BattleContext.instance.global_atk_pts += delta;
                    break;
                case "global_atk_add":
                    BattleContext.instance.global_atk_add += delta;
                    break;
                case "global_critical_chance":
                    BattleContext.instance.global_critical_chance += delta;
                    break;
                case "global_critical_dmg_rate":
                    BattleContext.instance.global_critical_dmg_rate += delta;
                    break;
                case "global_knockback":
                    BattleContext.instance.global_knockback += delta;
                    break;
                case "global_armor_piercing":
                    BattleContext.instance.global_armor_piercing += delta;
                    break;
                case "global_armor_add":
                    BattleContext.instance.global_armor_add += delta;
                    break;
                case "global_hp_pts":
                    BattleContext.instance.global_hp_pts += delta;
                    break;
                case "global_hp_add":
                    BattleContext.instance.global_hp_add += delta;
                    break;
                case "melee_atk_pts":
                    BattleContext.instance.melee_atk_pts += delta;
                    break;
                case "melee_atk_add":
                    BattleContext.instance.melee_atk_add += delta;
                    break;
                case "melee_critical_chance":
                    BattleContext.instance.melee_critical_chance += delta;
                    break;
                case "melee_critical_dmg_rate":
                    BattleContext.instance.melee_critical_dmg_rate += delta;
                    break;
                case "melee_knockback":
                    BattleContext.instance.melee_knockback += delta;
                    break;
                case "melee_armor_piercing":
                    BattleContext.instance.melee_armor_piercing += delta;
                    break;
                case "melee_sharpness_durability":
                    BattleContext.instance.melee_sharpness_durability += delta;
                    break;
                case "melee_sharpness_add":
                    BattleContext.instance.melee_sharpness_add += delta;
                    all_devices = Device_Slot_Helper.GetAllDevice();
                    foreach (var device in all_devices)
                    {
                        device.UpdateData();
                    }
                    break;
                case "melee_sharpness_recover":
                    BattleContext.instance.melee_sharpness_recover += delta;
                    break;
                case "ranged_atk_pts":
                    BattleContext.instance.ranged_atk_pts += delta;
                    break;
                case "ranged_atk_add":
                    BattleContext.instance.ranged_atk_add += delta;
                    break;
                case "ranged_critical_chance":
                    BattleContext.instance.ranged_critical_chance += delta;
                    break;
                case "ranged_critical_dmg_rate":
                    BattleContext.instance.ranged_critical_dmg_rate += delta;
                    break;
                case "ranged_knockback":
                    BattleContext.instance.ranged_knockback += delta;
                    break;
                case "ranged_armor_piercing":
                    BattleContext.instance.ranged_armor_piercing += delta;
                    break;
                case "ranged_reloading_speedup":
                    BattleContext.instance.ranged_reloading_speedup += delta;
                    break;
                case "ranged_capacity":
                    BattleContext.instance.ranged_capacity += delta;
                    all_devices = Device_Slot_Helper.GetAllDevice();
                    foreach(var device in all_devices)
                    {
                        device.UpdateData();
                    }
                    break;
                case "ranged_salvo":
                    BattleContext.instance.ranged_salvo += delta;
                    break;
                case "ranged_critical_reload":
                    BattleContext.instance.ranged_critical_reload += delta;
                    break;
                case "ranged_ammo_bigger":
                    BattleContext.instance.ranged_ammo_bigger += delta;
                    break;
                case "ranged_player_ammo_gravity":
                    BattleContext.instance.ranged_player_ammo_gravity += delta;
                    break;  
                case "wheel_charge_capacity":
                    BattleContext.instance.wheel_charge_capacity += delta;
                    break;
                case "wheel_charge_effect":
                    BattleContext.instance.wheel_charge_effect += delta;
                    break;
                case "wheel_charge_recover":
                    BattleContext.instance.wheel_charge_recover += delta;
                    break;
                case "wheel_tractive_force":
                    BattleContext.instance.wheel_tractive_force += delta;
                    break;
                case "ram_stability":
                    BattleContext.instance.ram_stability += delta;
                    break;
                case "ram_maneuverability":
                    BattleContext.instance.ram_maneuverability += delta;
                    break;
                case "ram_dmg_speed_bonus":
                    BattleContext.instance.ram_dmg_speed_bonus += delta;
                    break;
                case "energy_capacity_add":
                    BattleContext.instance.energy_capacity_add += delta;
                    all_devices = Device_Slot_Helper.GetAllDevice();
                    foreach (var device in all_devices)
                    {
                        device.UpdateData();
                    }
                    break;
                case "energy_recover_add":
                    BattleContext.instance.energy_recover_add += delta;
                    all_devices = Device_Slot_Helper.GetAllDevice();
                    foreach (var device in all_devices)
                    {
                        device.UpdateData();
                    }
                    break;
                default:
                    UnityEngine.Debug.LogError("Relic_Modify_Variable: Unknown variable name " + variable_name);
                    break;
            }
        }
    }
}
