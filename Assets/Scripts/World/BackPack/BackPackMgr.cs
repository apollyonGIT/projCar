using AutoCodes;
using Commons;
using Foundations;
using Foundations.MVVM;
using Foundations.Tickers;
using System;
using System.Collections.Generic;
using System.Linq;
using World.Loots;

namespace World.BackPack
{
    public interface IBackPackMgrView : IModelView<BackPackMgr>
    {
        void add_slot(BackPackSlot slot);
        void remove_slot(BackPackSlot slot);
        void select_slot(BackPackSlot slot);
        void unselect_slot(BackPackSlot slot);

        void tick();
    }
    public class BackPackMgr : Model<BackPackMgr, IBackPackMgrView>, IMgr
    {
        public List<BackPackSlot> slots = new List<BackPackSlot>();
        public List<OverweightBackPackSlot> ow_slots = new List<OverweightBackPackSlot>();

        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        public BackPackSlot select_slot;
        private float overweight_sum;

        public int slot_count;


        //就假定掉落物id 10000 为金币吧，不进入背包 直接修改coin_count;
        public int coin_count;

        public BackPackMgr(string name, int priority, params object[] args)
        {
            m_mgr_name = name;
            m_mgr_priority = priority;

            (this as IMgr).init(args);
        }

        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Ticker.instance;
            ticker.remove_tick(m_mgr_name);
            ticker.remove_tick1(m_mgr_name);
        }

        void IMgr.init(params object[] args)
        {
            Mission.instance.attach_mgr(m_mgr_name, this);

            var ticker = Ticker.instance;
            ticker.add_tick(m_mgr_priority, m_mgr_name, tick);
            ticker.add_tick1(m_mgr_priority, m_mgr_name, tick1);
        }
        public void tick()
        {
            RemoveOverweightSlot();

            WorldContext.instance.total_weight -= overweight_sum;
            overweight_sum = Config.current.per_overweight_slot_weight * ow_slots.Count;
            WorldContext.instance.total_weight += overweight_sum;

            foreach (var view in views)
            {
                view.tick();
            }
        }
        public void tick1()
        {

        }
        public void Init(int slot_count)
        {
            this.slot_count = slot_count;

            for (int i = 0; i < slot_count; i++)
            {
                var slot = new BackPackSlot();
                slots.Add(slot);

                foreach (var view in views)
                {
                    view.add_slot(slot);
                }
            }

        }


        public void AddSlot(int num)
        {
            this.slot_count += num;

            for (int i = 0; i < num; i++)
            {
                var slot = new BackPackSlot();
                slots.Add(slot);

                foreach (var view in views)
                {
                    view.add_slot(slot);
                }
            }
        }


        public void AddLoot(Loot loot)
        {
            if (loot.desc.id == 10000)
            {
                coin_count++;
                return;
            }

            var has_space = false;
            foreach (var slot in slots)
            {
                if (slot.loot == null)
                {
                    slot.loot = loot;
                    has_space = true;
                    break;
                }
            }

            if (!has_space)
            {
                var ow_slot = new OverweightBackPackSlot();
                ow_slot.loot = loot;
                ow_slots.Add(ow_slot);

                foreach (var view in views)
                {
                    view.add_slot(ow_slot);
                }
            }
        }

        public void AddLoot(uint loot_id)
        {
            if (loot_id == 10000)
            {
                coin_count++;
                return;
            }

            loots.TryGetValue(loot_id.ToString(), out var loot_desc);

            if (loot_desc == null)
                return;

            AddLoot(new Loot()
            {
                desc = loot_desc,
                velocity = UnityEngine.Vector2.zero,
                pos = UnityEngine.Vector2.zero,
            });
        }

        //把超重物体搬到普通背包槽
        private void sort_loot()
        {
            if (ow_slots.Count == 0)
                return;

            foreach (var slot in slots)
            {
                if (slot.loot == null)
                {
                    foreach (var ow_slot in ow_slots)
                    {
                        if (ow_slot.loot != null)
                        {
                            slot.loot = ow_slot.loot;
                            ow_slot.loot = null;
                            break;
                        }
                    }
                }
            }

            RemoveOverweightSlot();

        }

        public bool RemoveLoot(Loot loot)
        {
            return RemoveLoot((int)loot.desc.id, 1);
        }

        public bool RemoveLoot(int loot_id, int count)
        {
            if (loot_id == 10000)
            {
                if (coin_count >= count)
                {
                    coin_count -= count;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            int i = 0;

            foreach (var slot in ow_slots)
            {
                if (slot.loot != null && slot.loot.desc.id == loot_id)
                {
                    slot.loot = null;
                    i++;
                    if (i == count)
                    {
                        sort_loot();
                        return true;
                    }
                }
            }

            foreach (var slot in slots)
            {
                if (slot.loot != null && slot.loot.desc.id == loot_id)
                {
                    slot.loot = null;
                    i++;
                    if (i == count)
                    {
                        sort_loot();
                        return true;
                    }

                }
            }

            sort_loot();
            return false;
        }
        private void RemoveOverweightSlot()
        {
            for (int i = ow_slots.Count - 1; i >= 0; i--)
            {
                if (ow_slots[i].loot == null)
                {
                    foreach (var view in views)
                    {
                        view.remove_slot(ow_slots[i]);
                    }

                    ow_slots.RemoveAt(i);
                }
            }
        }
        public void SelectSlot(BackPackSlot slot)
        {
            CancelSelectSlot();
            select_slot = slot;
            foreach (var view in views)
            {
                view.select_slot(slot);
            }
        }
        public void CancelSelectSlot()
        {
            foreach (var view in views)
            {
                view.unselect_slot(select_slot);
            }
            select_slot = null;
        }
        public int GetLootAmount(int loot_id)
        {
            int amount = 0;
            foreach (var slot in slots)
            {
                if (slot.loot != null && slot.loot.desc.id == loot_id)
                {
                    amount++;
                }
            }

            foreach (var slot in ow_slots)
            {
                if (slot.loot != null && slot.loot.desc.id == loot_id)
                {
                    amount++;
                }
            }
            return amount;
        }


        public List<uint> get_all_loots()
        {
            List<uint> all_loots = new();

            foreach (var slot in slots.Where(t => t.loot != null))
            {
                all_loots.Add(slot.loot.desc.id);
            }

            foreach (var slot in ow_slots.Where(t => t.loot != null))
            {
                all_loots.Add(slot.loot.desc.id);
            }

            return all_loots;
        }

        public LootSettlementData Settlement(bool success)
        {
            LootSettlementData data = new();

            var camp_coins = CommonContext.instance.camp_coins;

            if (success)
            {
                foreach (var slot in slots)
                {
                    if (slot.loot != null)
                    {
                        foreach (var camp_coin in slot.loot.desc.camp_coin)
                        {
                            if (camp_coins.ContainsKey(camp_coin.Key))
                            {
                                camp_coins[camp_coin.Key] += camp_coin.Value;
                            }
                            else
                            {
                                camp_coins.Add(camp_coin.Key, camp_coin.Value);
                            }
                        }


                        data.loots.Add(new SingelLootSettlementData()
                        {
                            loot = slot.loot,
                            is_broken = false,
                            camp_coins = slot.loot.desc.camp_coin.Select(kvp => (kvp.Key, kvp.Value)).ToList(),
                        });

                        slot.loot = null;
                    }
                }
            }
            else
            {
                var slot_has_loot = slots.FindAll(s => s.loot != null);
                var cnt = slot_has_loot.Count;
                var break_num = (int)(UnityEngine.Random.Range(Config.current.k_loot_broken_rate.x, Config.current.k_loot_broken_rate.y) / 100f * cnt);
                List<Loot> broken_loots = new();
                while (broken_loots.Count < break_num)
                {
                    var index = UnityEngine.Random.Range(0, slot_has_loot.Count);
                    var slot = slot_has_loot[index];
                    data.loots.Add(new SingelLootSettlementData()
                    {
                        loot = slot.loot,
                        is_broken = true,
                    });

                    broken_loots.Add(slot.loot);
                    slot_has_loot.RemoveAt(index);
                }

                foreach (var slot in slot_has_loot)
                {
                    foreach (var camp_coin in slot.loot.desc.camp_coin)
                    {
                        if (camp_coins.ContainsKey(camp_coin.Key))
                        {
                            camp_coins[camp_coin.Key] += camp_coin.Value;
                        }
                        else
                        {
                            camp_coins.Add(camp_coin.Key, camp_coin.Value);
                        }
                    }

                    data.loots.Add(new SingelLootSettlementData()
                    {
                        loot = slot.loot,
                        is_broken = false,
                        camp_coins = slot.loot.desc.camp_coin.Select(kvp => (kvp.Key, kvp.Value)).ToList(),
                    });
                }
            }

            Camp_Coin.CampCoin.instance.ShowCoins();

            return data;
        }
    }

    public class LootSettlementData
    {
        public List<SingelLootSettlementData> loots = new();
    }

    public class SingelLootSettlementData
    {
        public Loot loot;
        public bool is_broken;

        public List<(int, int)> camp_coins = new();
    }
}
