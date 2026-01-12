using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Devices.DeviceEmergencies.DeviceEmergenciesView
{
    public class SingleAcidView : MonoBehaviour
    {
        public SingleAcid data;
        public DeviceAcidView dav;

        public void Init(SingleAcid sa)
        {
            data = sa;
        }

        public void ReduceAcid()
        {
            dav.ReduceAcid(this);
            Destroy(gameObject);
        }
    }
}
