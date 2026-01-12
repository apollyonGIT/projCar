


namespace World.Devices.DeviceUpgrades
{
    public class DU_Empty : IDeviceUpgrade
    {
        public DU_Empty()
        {
            // 没有任何卵用，仅仅是为了临时填表，避免报错
            Applied = false;
        }

        public bool Applied { get; set; }
        void IDeviceUpgrade.ApplyUpgrade(Device device)
        {
            Applied = true;
        }

        void IDeviceUpgrade.RemoveUpgrade(Device device)
        {
            Applied = false;
        }
    }
}
