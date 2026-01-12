using AutoCodes;
using World.Devices;

namespace World.Characters.CharacterProperties
{
    public class CP_Device_Disabled : CharacterProperty
    {
        public CP_Device_Disabled(Character c, properties p) : base(c, p) { }

        public override void Init()
        {
            owner.device_can_work_func.Add(CanWork);
        }

        private bool CanWork(Device d)
        {
            if (desc.int_parms.Count == 0)
                return true;

            foreach (var tag in desc.int_parms)
                if (CharacterUtility.CheckDeviceTag(d, (uint)tag))
                    return false;

            return true;
        }
    }
}