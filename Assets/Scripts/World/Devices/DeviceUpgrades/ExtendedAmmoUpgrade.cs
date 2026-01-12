using World.Devices.Device_AI;

namespace World.Devices.DeviceUpgrades
{
    /// <summary>
    /// 扩容升级
    /// </summary>
    public class ExtendedAmmoUpgrade : IDeviceUpgrade
    {
        private int ammo_increase;

        public ExtendedAmmoUpgrade(int ammo_increase)
        {
            this.ammo_increase = ammo_increase;

            Applied = false;
        }

        public bool Applied { get; set; }

        void IDeviceUpgrade.ApplyUpgrade(Device device)
        {
            if(device is ILoad load)
            {
                load.Max_Ammo += ammo_increase;
            }

            Applied = true;
        }

        void IDeviceUpgrade.RemoveUpgrade(Device device)
        {
            if (device is ILoad load)
            {
                load.Max_Ammo -= ammo_increase;
            }

            Applied = false;
        }
    }
}
