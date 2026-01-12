using Addrs;
using Commons;
using Foundations.MVVM;
using UnityEngine;

namespace World.VFXs
{
    public class VFXMgrView : MonoBehaviour, IVFXMgrView
    {
        VFXMgr owner;

        private MixedObjectPool<VFXView> vfx_pool;
        void IModelView<VFXMgr>.attach(VFXMgr owner)
        {
            vfx_pool = new MixedObjectPool<VFXView>(30, transform);
            this.owner = owner;
        }

        void IModelView<VFXMgr>.detach(VFXMgr owner)
        {
            if (this.owner != null)
                this.owner = null;
            Destroy(gameObject);
        }

        void IVFXMgrView.notify_add_vfx(VFX vfx)
        {
            Addressable_Utility.try_load_asset<VFXView>(vfx.path, out var vfx_view);
            var vfx_entity = vfx_pool.Get(vfx.path, vfx_view);
            vfx_entity.Init(vfx, owner);
        }

        void IVFXMgrView.notify_remove_vfx(VFX vfx)
        {
            foreach(var vfxs in vfx_pool.objectsPoped)
            {
                for (int i = vfxs.Value.Count - 1; i >= 0; i--)
                {
                    if (vfxs.Value[i].data == vfx)
                    {
                        vfx_pool.Recycle(vfx.path, vfxs.Value[i]);
                    }
                }
            }
        }

        void IVFXMgrView.notify_reset_vfx()
        {
            foreach(var vfxs in vfx_pool.objectsPoped)
            {
                for (int i = vfxs.Value.Count - 1; i >= 0; i--)
                {
                    vfxs.Value[i].transform.position = new Vector3(vfxs.Value[i].data.pos.x, vfxs.Value[i].data.pos.y, 10);
                }
            }
        }

        void IVFXMgrView.notify_update_vfx(VFX vfx)
        {
            foreach (var vfxs in vfx_pool.objectsPoped)
            {
                for (int i = vfxs.Value.Count - 1; i >= 0; i--)
                {
                    if (vfxs.Value[i].data == vfx)
                    {
                        vfxs.Value[i].UpdatePos();
                    }
                }
            }
        }
    }
}
