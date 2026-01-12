using World.Devices.Device_AI;

namespace World.Devices.DeviceUpgrades
{
    public class AttackUpgrade : IDeviceUpgrade
    {
        private float damage_increase;  //百分之几的伤害增加
        private float armor_ignore;
        private float attack_speed;
        private float knock_increase;
        public AttackUpgrade(float dmg,float am,float atkspeed,float ki)
        {
            damage_increase = dmg;
            armor_ignore = am;
            attack_speed = atkspeed;
            knock_increase = ki;

            Applied = false;
        }

        public bool Applied{ get; set; }
        void IDeviceUpgrade.ApplyUpgrade(Device device)
        {
            if(device is IAttack attack)
            {
                attack.Damage_Increase += damage_increase;
                attack.Knockback_Increase += knock_increase;
            }
            Applied = true;
        }

        void IDeviceUpgrade.RemoveUpgrade(Device device)
        {
            if(device is IAttack attack)
            {
               attack.Damage_Increase -= damage_increase;
               attack.Knockback_Increase -= knock_increase;
            }
            Applied = false;
        }
    }
}
