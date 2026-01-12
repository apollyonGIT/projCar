using System.Collections.Generic;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUpgrades
{
    public class VariableUpgrade : IDeviceUpgrade
    {
        private List<int> int_parms = new();
        private List<string> string_parms = new();

        public VariableUpgrade(List<int> int_parms,List<string> string_parms)
        {
            this.int_parms = int_parms;
            this.string_parms = string_parms;
        }

        public bool Applied { get; set; }
        public void ApplyUpgrade(Device device)
        {
            for(int i = 0; i < string_parms.Count; i++)
            {
                update_variable(string_parms[i], int_parms[i], device);
            }

            Applied = true;
        }
        public void RemoveUpgrade(Device device)
        {
            for (int i = 0; i < string_parms.Count; i++)
            {
                update_variable(string_parms[i], -int_parms[i], device);
            }

            Applied = false;
        }

        private void update_variable(string variable_name,int variable_delta,Device device)
        {
            switch (variable_name)
            {
                case "weight":
                    device.desc.weight += variable_delta;
                    WorldContext.instance.total_weight += variable_delta;
                    break;
                case "def":
                    
                    break;
                case "hp":
                    device.battle_data.hp += variable_delta;
                    break;
                case "armor_piercing":
                    
                    break;
                case "atk_interval":
                    if(device is IAttack attack)
                    {
                        attack.Attack_Interval_Factor += variable_delta/1000f;
                        device.UpdateData();
                    }
                    break;
                case "tractive_force":
                    if(device is BasicWheel wheel)
                    {
                        wheel.wheel_desc.tractive_force_max += variable_delta;
                        device.UpdateData();
                    }
                    break;
                case "ammo_capacity":
                    if(device is ILoad load)
                    {
                        load.Max_Ammo += variable_delta;
                        device.UpdateData();
                    }
                    break;
                case "atk":
                    if (device is IAttack)
                    {
                        (device as IAttack).Damage_Increase += variable_delta;
                    }
                    break;
                case "reload_speed":
                    if (device is ILoad)
                    {
                        (device as ILoad).Reload_Speed_Factor += variable_delta / 1000f;
                        device.UpdateData();
                    }
                    break;
                case "proj_spread":
                    if(device is IShoot)
                    {
                        (device as IShoot).Proj_Spread += variable_delta/1000f;
                    }
                    break;
                case "proj_speed":
                    if (device is IShoot)
                    {
                        (device as IShoot).Proj_Speed += variable_delta / 1000f;
                    }
                    break;
                case "proj_num":
                    if (device is IShoot)
                    {
                        (device as IShoot).Proj_Num += variable_delta;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
