using System.Collections.Generic;

namespace World.Devices.DeviceUpgrades
{
    public class CompositeUpgrade : IDeviceUpgrade
    {
        private List<IDeviceUpgrade> upgrades = new List<IDeviceUpgrade>();
        public CompositeUpgrade(List<IDeviceUpgrade> upgrades)
        {
            this.upgrades = upgrades;
            Applied = false;
        }

        public bool Applied { get ;set; }

        void IDeviceUpgrade.ApplyUpgrade(Device device)
        {
            foreach(var upgrade in upgrades)
            {
                upgrade.ApplyUpgrade(device);
            }

            Applied = true;
        }

        void IDeviceUpgrade.RemoveUpgrade(Device device)
        {
            foreach (var upgrade in upgrades)
            {
                upgrade.RemoveUpgrade(device);
            }

            Applied = false;
        }
    }
}
