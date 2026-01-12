using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Devices.DeviceEmergencies.DeviceEmergenciesView
{
    public class SingleFireView : MonoBehaviour
    {
        public DeviceFiredView dfv;
        public float fire_value;  
        public void tick()
        {
            var scale = Mathf.Clamp(fire_value / 50f,0.2f,1f);
            transform.localScale = Vector3.one * scale * 2;
        }

        public void ReduceFire(float delta)
        {
            dfv.ReduceFire(delta);
        }
    }
}
