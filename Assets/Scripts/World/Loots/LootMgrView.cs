using Addrs;
using Commons;
using Foundations.MVVM;
using UnityEngine;

namespace World.Loots
{
    public class LootMgrView : MonoBehaviour, ILootMgrView
    {
        public LootMgr owner;

        private MixedObjectPool<LootView> loot_pool;

        void IModelView<LootMgr>.attach(LootMgr owner)
        {
            loot_pool = new MixedObjectPool<LootView>(30, transform);
            this.owner = owner;    
        }

        void ILootMgrView.destroy_loot(Loot loot)
        {
            foreach(var loots in loot_pool.objectsPoped)
            {
                for(int i = loots.Value.Count - 1; i >= 0; i--)
                {
                    if (loots.Value[i].data == loot)
                    {
                        loot_pool.Recycle(loot.desc.id.ToString(), loots.Value[i]);
                    }
                }
            }
        }

        void IModelView<LootMgr>.detach(LootMgr owner)
        {
            if (this.owner != null)
                owner = null;
            Destroy(gameObject);
        }

        void ILootMgrView.instantiate_loot(Loot loot)
        {
            Addressable_Utility.try_load_asset<LootView>("CoinView",out var loot_view);
            var loot_entity = loot_pool.Get(loot.desc.id.ToString(), loot_view);
            loot_entity.Init(loot,owner);
        }

        void ILootMgrView.update_loots()
        {
            foreach(var loots in loot_pool.objectsPoped)
            {
                foreach(var loot_view in loots.Value)
                {
                    var pos = loot_view.data.pos;
                    var vel = loot_view.data.velocity;

                    loot_view.transform.position = new Vector3(pos.x,pos.y,10) + new Vector3(vel.x,vel.y,10) * Config.PHYSICS_TICK_DELTA_TIME;
                }
            }
        }
    }
}
