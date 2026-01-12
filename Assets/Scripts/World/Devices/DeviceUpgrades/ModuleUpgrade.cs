namespace World.Devices.DeviceUpgrades
{
    public class ModuleUpgrade : IDeviceUpgrade
    {
        private int index;

        public ModuleUpgrade(int index)
        {
            this.index = index;
            Applied = false;
        }

        public bool Applied { get; set; }

        void IDeviceUpgrade.ApplyUpgrade(Device device)
        {
            /*if(device.module_list.Count > index)
            {
                foreach(var db in device.module_list[index].db_list)
                {
                    db.is_auto = true;
                }
            }*/
            Applied = true;
        }

        void IDeviceUpgrade.RemoveUpgrade(Device device)
        {
            /*if (device.module_list.Count > index)
            {
                foreach (var db in device.module_list[index].db_list)
                {
                    db.is_auto = false;
                }
            }*/
            Applied = false;
        }
    }
}
