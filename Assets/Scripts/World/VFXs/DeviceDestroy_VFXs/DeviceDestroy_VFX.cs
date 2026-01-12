using Addrs;
using Commons;
using Foundations;
using UnityEngine;

namespace World.VFXs.DeviceDestroy_VFXs
{
    public class DeviceDestroy_VFX : MonoBehaviourSingleton<DeviceDestroy_VFX>
    {
        public DeviceDestroy_VFX_Mono create_cell(params object[] prms)
        {
            var res_name = (string)prms[0];
            var pos = (Vector2)prms[1];
            var duration = (int)prms[2];

            Addressable_Utility.try_load_asset(res_name, out DeviceDestroy_VFX_Mono model);
            var vfx = Instantiate(model,transform);
            vfx.transform.position = pos;
            vfx.transform.localPosition = new(vfx.transform.localPosition.x, vfx.transform.localPosition.y,0);

            Destroy(vfx.gameObject,duration * Config.PHYSICS_TICK_DELTA_TIME);

            return vfx;
        }
    }
}
