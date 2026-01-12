namespace World.Devices.DeviceUpgrades
{
    public class DU_Soul_Auto : IDeviceUpgrade
    {
        private int effect;
        public DU_Soul_Auto(int delta) {
            effect = delta;
        }

        public bool Applied { get; set; }

        void IDeviceUpgrade.ApplyUpgrade(Device device)
        {
            /*foreach(var device_module in device.module_list)
            {
                foreach (var db in device_module.db_list)
                {
                    db.is_auto = true;
                }
            }*/

            device.self_efficiency += effect;
            device.efficiency_func.Add(soul_auto_func);

            Applied = true;
        }

        private int soul_auto_func(Device device,int t)
        {
           /* if (device.module_list.Count > 0 && device.module_list[0].has_worker)
                return t - effect;*/
            return t;
        }

        void IDeviceUpgrade.RemoveUpgrade(Device device)
        {
            /*foreach (var device_module in device.module_list)
            {
                foreach (var db in device_module.db_list)
                {
                    db.is_auto = false;
                }
            }
*/
            device.self_efficiency -= effect;
            device.efficiency_func.Remove(soul_auto_func);

            Applied = false;
        }
    }
}
