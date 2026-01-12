using Addrs;
using Commons;
using Foundations.MVVM;
using UnityEngine;

namespace World.Projectiles
{
    public class ProjectileMgrView : MonoBehaviour, IProjectileMgrView
    {
        ProjectileMgr owner;

        private MixedObjectPool<ProjectileView> projectiles_pool;
        public Transform content;

        void IModelView<ProjectileMgr>.attach(ProjectileMgr owner)
        {
            this.owner = owner; 
        }

        void IModelView<ProjectileMgr>.detach(ProjectileMgr owner)
        {
            if (this.owner != null)
                owner = null;
            Destroy(gameObject);
        }

        void IProjectileMgrView.notify_add_projectile(Projectile p)
        {
            if(p.view_prefab_index >= p.desc.path.Count)
            {
                p.view_prefab_index = Random.Range(0, p.desc.path.Count);
            }
            Addressable_Utility.try_load_asset<ProjectileView>(p.desc.path[p.view_prefab_index], out var pv);
            var pv_entity = projectiles_pool.Get(p.desc.projectile_id.ToString(), pv);
            pv_entity.Init(p);
        }

        void IProjectileMgrView.notify_remove_projectile(Projectile p)
        {
            foreach(var d in projectiles_pool.objectsPoped)
            {
                for(int i = d.Value.Count - 1; i >= 0; i--)
                {
                    if (d.Value[i].owner == p)
                    {
                        if (d.Value[i].trail != null)
                            d.Value[i].trail.Clear();
                        d.Value[i].transform.localScale = Vector3.one;

                        projectiles_pool.Recycle(p.desc.projectile_id.ToString(), d.Value[i]);                       
                    }
                }
            }
        }

        void IProjectileMgrView.tick()
        {
            if (projectiles_pool == null)
                return;
            foreach(var d in projectiles_pool.objectsPoped)
            {
                foreach(var p in d.Value)
                {
                    p.tick();
                }
            }
        }

        void IProjectileMgrView.tick1()
        {
            
        }

        void IProjectileMgrView.init()
        {
            projectiles_pool = new MixedObjectPool<ProjectileView>(30, content);
        }

        void IProjectileMgrView.notify_reset_projectile()
        {
            foreach(var l in projectiles_pool.objectsPoped)
            {
                foreach(var p in l.Value)
                {
                    p.ResetPos();
                }
            }
        }
    }
}
