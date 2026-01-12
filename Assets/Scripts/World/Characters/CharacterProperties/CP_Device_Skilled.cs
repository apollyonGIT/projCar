using AutoCodes;
using World.Devices;

namespace World.Characters.CharacterProperties
{
    public class CP_Device_Skilled : CharacterProperty
    {
        public CP_Device_Skilled(Character c, properties p) : base(c, p)
        {

        }

        public override void Init()
        {
            owner.device_properties_func.Add(AddAbility);
        }

        private float AddAbility(Device device, float current_ability)
        {
            if(device == null)
                return current_ability;

            var result = current_ability;
            if (check_device_tag(device))
                result += desc.float_parms[0];
            return result;
        }

        private bool check_device_tag(Device device)
        {
            if (desc.int_parms.Count == 0)
                return false;

            foreach (var tag in desc.int_parms)
                if (CharacterUtility.CheckDeviceTag(device, (uint)tag))
                    return true;

            return false;
        }
    }
}
