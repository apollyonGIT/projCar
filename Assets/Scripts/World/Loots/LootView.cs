using Addrs;
using UnityEngine;

namespace World.Loots
{
    public class LootView : MonoBehaviour
    {
        public LootMgr owner;
        public Loot data;
        public void Init(Loot loot,LootMgr owner) 
        {
            this.owner = owner;
            data = loot;

            Addressable_Utility.try_load_asset<Sprite>(loot.desc.view, out var loot_sprite);
            GetComponent<SpriteRenderer>().sprite = loot_sprite;
        }
    }
}
