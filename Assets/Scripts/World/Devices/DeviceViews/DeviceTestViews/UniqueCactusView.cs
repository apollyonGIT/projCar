using System.Collections.Generic;
using UnityEngine;
using World.Devices.Device_AI;

namespace World.Devices.DeviceViews.DeviceTestViews
{
    public class UniqueCactusView : DeviceView_Spine
    {
        public BoxCollider2D cactus_collider;

        public List<GameObject> cactus_list = new();

        private const float default_size_y =1f;

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            var default_size = cactus_collider.size;
            cactus_collider.size = new Vector2(default_size.x, default_size_y * (owner as Unique_Cactus).cactus_states);
        }
    }
}
