using Foundations;
using UnityEngine;
using World.VFXs;

namespace World.Helpers
{
    public class Vfx_Helper
    {
        public static VFX InstantiateVfx(string vfx_path,int duration, Vector2 pos,bool need_tick = false)
        {
            Mission.instance.try_get_mgr("VFX", out VFXMgr vmgr);
            return vmgr.AddVFX(vfx_path,duration,pos,need_tick);
        }

        public static VFX InstantiateVfx(string vfx_path, int duration,Transform kp, bool need_tick = false)
        {
            Mission.instance.try_get_mgr("VFX", out VFXMgr vmgr);
            var vfx = new VFX(vfx_path, duration, kp);
            vmgr.AddVFX(vfx, need_tick);
            return vfx;
        }
    }
}
