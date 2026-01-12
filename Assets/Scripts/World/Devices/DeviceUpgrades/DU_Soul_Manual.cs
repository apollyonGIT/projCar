namespace World.Devices.DeviceUpgrades
{
    public class DU_Soul_Manual : IDeviceUpgrade
    {
        private int effect;
        public DU_Soul_Manual(int delta)
        {
            effect = delta;
        }

        public bool Applied { get; set; }
        public void ApplyUpgrade(Device device)
        {
            device.efficiency_func.Add(soul_manual_func);
            Applied = true;
        }

        private int soul_manual_func(Device device, int t)
        {
            /*if (device.module_list.Count > 0 && device.module_list[0].has_worker)
                return t + effect;*/
            return t;
        }
        public void RemoveUpgrade(Device device)
        {
            device.efficiency_func.Remove(soul_manual_func);
            Applied = false;
        }
    }
}
