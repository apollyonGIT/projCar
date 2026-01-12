namespace World.Devices.DeviceUpgrades
{
    public class CommonDataUpgrade : IDeviceUpgrade
    {
        private int weight;
        private int def;
        private int hp;
        private int armor_piercing;         //千分之 ,记得除以1000
        private int atk_speed;              //千分之 ,记得除以1000

        public CommonDataUpgrade(int v1, int v2, int v3, int v4, int v5)
        {
            weight = v1;
            def = v2;
            hp = v3;
            armor_piercing = v4;
            atk_speed = v5;
        }

        public bool Applied { get ; set ; }

        void IDeviceUpgrade.ApplyUpgrade(Device device)
        {
            device.desc.weight += weight;
            //device.desc.def += def;
            //device.desc.hp += hp;
            //device.desc.armor_piercing += armor_piercing;

            WorldContext.instance.total_weight += weight;

            //device.desc.atk_speed += atk_speed;
        }

        void IDeviceUpgrade.RemoveUpgrade(Device device)
        {
            device.desc.weight -= weight;
            //device.desc.def -= def;
            //device.desc.hp -= hp;
            //device.desc.armor_piercing -= armor_piercing;

            WorldContext.instance.total_weight -= weight;
        }
    }
}
