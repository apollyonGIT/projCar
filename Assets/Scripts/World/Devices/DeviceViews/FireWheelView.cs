using UnityEngine;

namespace World.Devices.DeviceViews
{
    public class FireWheelView :WheelView
    {
        public DeviceCollider fire_collider;

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            var ls = fire_collider.transform.localScale;
            var scale = new Vector3(ls.x,WorldContext.instance.caravan_velocity.x/4f,ls.z);
            fire_collider.transform.localScale = scale;
        }
    }
}
