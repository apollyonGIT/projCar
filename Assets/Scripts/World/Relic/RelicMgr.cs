using Addrs;
using AutoCodes;
using Foundations;
using Foundations.MVVM;
using Foundations.Tickers;
using System;
using System.Collections.Generic;
using UnityEngine;
using World.Business;
using World.Relic.Relics;
using World.Relic.RelicStores;

namespace World.Relic
{
    public interface IRelicMgrView : IModelView<RelicMgr>
    {
        void notify_init();
        void notify_add_rellic(Relic relic);
        void notify_remove_rellic(Relic relic);
    }
    public class RelicMgr : Model<RelicMgr, IRelicMgrView>, IMgr
    {
        private Dictionary<string, Func<Relic>> relic_dic = new() {
            { "ZoomMeleeRelic",()=> new ZoomMeleeRelic()},
            { "AccelerateRelic",()=> new AccelerateRelic()},
            { "TitanEnemyRelic",()=> new TitanEnemyRelic()},
            { "DifficultModeRelic",()=> new DifficultModeRelic()},
            { "LuckyDogRelic",()=> new LuckyDogRelic()},
            { "Relic_Modify_Variable",() => new Relic_Modify_Variable()},
            { "Relic_Ammo_Hail",()=> new Relic_Ammo_Hail()},
            { "Relic_Ammo_Division",()=> new Relic_Ammo_Division()},
            { "Relic_Ammo_Turn_Back",()=> new Relic_Ammo_Turn_Back()},
            { "Relic_Ammo_Slow_Down",()=> new Relic_Ammo_Slow_Down()},
            { "Relic_Ammo_The_Fourth_Wall",()=> new Relic_Ammo_The_Fourth_Wall()},
            { "Relic_Ammo_Knock_Ball",()=> new Relic_Ammo_Knock_Ball()},
            { "Relic_Ammo_Speed_Keeper",()=> new Relic_Ammo_Speed_Keeper()},
            { "Relic_Muzzle_Blast_Knock_Off",()=> new Relic_Muzzle_Blast_Knock_Off()},
            { "Relic_Muzzle_Blast",()=> new Relic_Muzzle_Blast()},
            { "Relic_Reloading_Go_Off",()=> new Relic_Reloading_Go_Off()},
            { "Relic_Melee_Ignite",()=> new Relic_Melee_Ignite()},
            { "Relic_Melee_Coin",()=> new Relic_Melee_Coin()},
            { "Relic_Melee_Assassinate",()=> new Relic_Melee_Assassinate()},
            { "Relic_Empty",()=> new Relic_Empty()},
            { "Relic_Muscle_Memory",()=> new Relic_Muscle_Memory()},
            { "Relic_Landing_Trigger_Shoot",() => new Relic_Landing_Trigger_Shoot()},
        };
        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;
        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        public List<Relic> relic_list = new();

        public RelicStore relic_store;

        public RelicMuseum relic_museum = new RelicMuseum();

        public RelicMgr(string mgr_name, int mgr_priority, params object[] args)
        {
            m_mgr_name = mgr_name;
            m_mgr_priority = mgr_priority;

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
            relic_museum.Init();
        }
        void tick()
        {
            for (int i = 0; i < relic_list.Count; i++)
            {
                relic_list[i].Tick();
            }
        }

        void tick1()
        {

        }

        public Relic GetRelic(string id)
        {
            return create_relic(id);
        }
        private Relic create_relic(string id)
        {
            if (relic_dic.ContainsKey(id))
            {
                return relic_dic[id]();
            }

            UnityEngine.Debug.LogWarning($"错误遗物类型 {id}");
            return new Relic();
        }

        private List<Relic> GetRndRelics(List<string> relics_id, int num)
        {
            List<Relic> temp = new();
            for (int i = 0; i < num; i++)
            {
                if (relics_id.Count == 0)
                    break;
                var index = UnityEngine.Random.Range(0, relics_id.Count);
                relics.records.TryGetValue(relics_id[index], out var relic_value);
                var r = create_relic(relic_value.behaviour_script);
                r.desc = relic_value;
                temp.Add(r);
                relics_id.RemoveAt(index);
            }
            return temp;
        }

        public void Init()
        {
            foreach (var view in views)
            {
                view.notify_init();
            }
        }

        public EliteRelicView InstantiateRelicStore(List<uint> relics_id)
        {
            Addressable_Utility.try_load_asset<EliteRelicView>("RelicDropView", out var rdv_view);
            var view = GameObject.Instantiate(rdv_view, WorldSceneRoot.instance.uiRoot.transform, false);
            view.Init(relics_id);

            return view;
        }

        public void ClearRelicStore()
        {
            relic_store.remove_all_views();
            relic_store = null;
        }

        private void add_relic(Relic relic)
        {
            if (relic.desc.upgrade_from != 0)
            {
                RemoveRelic(relic.desc.upgrade_from);
            }

            relic_list.Add(relic);
            relic.Get();
            foreach (var view in views)
            {
                view.notify_add_rellic(relic);
            }
        }

        private void add_relic_directly(Relic relic)
        {
            relic_list.Add(relic);
            relic.Get();
            foreach (var view in views)
            {
                view.notify_add_rellic(relic);
            }
        }

        public bool CreateRelicAndAdd(string id)
        {
            if (relic_museum.TryGetRelic(uint.Parse(id)))
            {
                relics.records.TryGetValue(id, out var relic_value);
                if (relic_value != null)
                {
                    var relic = create_relic(relic_value.behaviour_script);
                    relic.desc = relic_value;

                    add_relic(relic);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 无视前置 直接添加（用于save_data环节)
        /// </summary>
        /// <param name="id"></param>
        public void CreateRelicAndAddDirectly(string id)
        {
            relic_museum.GetRelic(uint.Parse(id));

            relics.records.TryGetValue(id, out var relic_value);
            if (relic_value != null)
            {
                var relic = create_relic(relic_value.behaviour_script);
                relic.desc = relic_value;

                add_relic_directly(relic);
            }
        }

        public void RemoveRelic(Relic relic)
        {
            relic_list.Remove(relic);
            relic.Drop();
            foreach (var view in views)
            {
                view.notify_remove_rellic(relic);
            }
        }

        public void RemoveRelic(uint relic_id)
        {
            var relic = relic_list.Find(r => r.desc.id == relic_id);
            if (relic != null)
            {
                RemoveRelic(relic);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"尝试移除不存在的遗物 {relic_id}");
            }
        }

        public Relic CreateRelic(string id)
        {
            relics.records.TryGetValue(id, out var relic_value);
            if (relic_value != null)
            {
                var relic = create_relic(relic_value.behaviour_script);
                relic.desc = relic_value;

                return relic;
            }

            return null;
        }
    }
}
