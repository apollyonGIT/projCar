using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class CactusUiView:DeviceUiView
    {
        public void Accelerate()
        {
            if (owner is Unique_Cactus cactus)
            {
                cactus.Accelerate();
            }
        }
    }
}