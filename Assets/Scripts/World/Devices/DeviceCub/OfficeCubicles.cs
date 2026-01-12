using World.Devices.Device_AI;

namespace World.Devices.DeviceCub
{
    public class OfficeCubicles
    {
        public Device owner;

        public DeviceModule module;

        public string prefab_path;

        public OfficeCubicles(Device d,DeviceModule md, string v)
        {
            owner = d;
            module = md;
            prefab_path = v;
        }
    }
}
