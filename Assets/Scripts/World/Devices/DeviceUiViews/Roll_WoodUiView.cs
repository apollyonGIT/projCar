using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class Roll_WoodUiView : BasicShooterUiView
    {
        public void Shoot()
        {
            (owner as Roll_Wood).Shoot();
        }
    }
}
