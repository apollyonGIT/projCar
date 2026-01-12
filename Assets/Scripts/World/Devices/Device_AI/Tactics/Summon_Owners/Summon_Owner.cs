using AutoCodes;
using Commons;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World.Devices
{
    public class Summon_Owner : Device
    {
        protected List<Summon_Mono> summons = new();

        public int energy;
        public int max_energy;

        public bool is_lock;

        public const int C_max_energy = 100 * Config.PHYSICS_TICKS_PER_SECOND;
        public const int C_create_summon_cost = 30 * Config.PHYSICS_TICKS_PER_SECOND;

        public const int C_add_energy_per_tick = 10;
        public const int C_lose_energy_per_tick = 10;

        //==================================================================================================

        public override void InitData(device_all rc)
        {
            base.InitData(rc);

            max_energy = C_max_energy;
            energy = C_max_energy;
        }


        public override void tick()
        {
            foreach (var summon in summons)
            {
                summon.on_tick();

                //规则：存在Summon时，且Summon激活，则不断失去能量
                if (summon.is_active)
                    energy -= C_lose_energy_per_tick;
            }

            //规则：如果不存在Summon，回复能量
            if (!summons.Any() && !is_lock)
            {
                energy += C_add_energy_per_tick;
                Mathf.Min(energy, max_energy);
            }

            //规则：如果能量归0，强制结束所有Summon
            if (energy <= 0)
            {
                energy = 0;
                clear_summon();
            }

            base.tick();
        }


        public void on_outter_end_drag(Vector2 pos)
        {
            create_summon(pos);
        }


        public void create_summon(Vector2 pos)
        {
            if (energy - C_create_summon_cost < 0) return;

            is_lock = true;

            energy -= C_create_summon_cost;
            energy = Mathf.Min(energy, max_energy);

            try_get_summon_model(out Summon_Mono summon_model);

            WorldSceneRoot.instance.try_find_pd(out DevicePD pd);
            var summon = Object.Instantiate(summon_model, pd.transform);

            summon.on_init(this, pos);
            summons.Add(summon);

            is_lock = false;
        }


        public virtual bool try_get_summon_model(out Summon_Mono summon_model)
        {
            summon_model = default;
            return true;
        }


        public void clear_summon()
        {
            foreach (var summon in summons.ToList())
            {
                summon.on_fini();
            }

            summons.Clear();
        }


        public void kill_summon(Summon_Mono summon)
        {
            summons.Remove(summon);
        }
    }
}

