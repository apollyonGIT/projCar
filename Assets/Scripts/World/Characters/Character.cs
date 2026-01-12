using AutoCodes;
using System;
using System.Collections.Generic;
using World.Characters.CharacterProperties;
using World.Characters.CharacterStates;
using World.Devices;

namespace World.Characters
{

    public class Character
    {
        public role desc;
        public List<CharacterProperty> character_properties = new List<CharacterProperty>();

        public bool is_working;

        protected float default_ability = 100f;

        public int tick_time;
        public int work_tick;

        public CharacterState state;

        #region delegate

        public List<Func<Device,float, float>> device_properties_func = new();
        public List<Func<Device, bool>> device_can_work_func = new ();

        public Action start_work;
        public Action end_work;

        public Action enter_safe_area;
        public Action leave_safe_area;

        public float coma_process = 0;

        #endregion
        public void tick()
        {
            tick_time++;
            if (is_working)
            {
                work_tick++;
            }

            if(state!= null)
            {
                if (state.duration > 0)
                {
                    state.duration--;
                    state.tick();
                }
                else
                {
                    state.end();
                    state = null;
                }
            } 
        }

        public virtual float GetAbility(Device d)
        {
            float current_ability;
            current_ability = (default_ability);

            if (device_properties_func != null)
            {
                foreach (var device_properties_func in device_properties_func)
                {
                    current_ability = device_properties_func.Invoke(d, current_ability);
                }
            }

            return current_ability;
        }

        public virtual bool CanWork(Device d)
        {
            if (device_can_work_func != null)
            {
                foreach (var device_can_work_func in device_can_work_func)
                {
                    if (!device_can_work_func.Invoke(d))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public virtual bool StateCanWork()
        {
            if(state != null)
            {
                switch (state)
                {
                    case Coma c:
                        return BattleContext.instance.coma_can_work;
                }
            }

            return true;
        }

        public virtual void EnterSafeArea()
        {
            enter_safe_area?.Invoke();

            state?.end();
            state = null;
        }

        public virtual void LeaveSafeArea()
        {
            leave_safe_area?.Invoke();

            tick_time = 0;
            work_tick = 0;
        }

        public virtual void StartWork()
        {
            is_working = true;
            start_work?.Invoke();
        }

        public virtual void EndWork()
        {
            is_working = false;
            end_work?.Invoke();
        }

        public void Init(role rc)
        {
            desc = rc;

            if (desc.properties != null)
            {
                foreach (var p in desc.properties)
                {
                    var cp = CharacterUtility.GetProperty(this, p);
                    cp.Init();
                    character_properties.Add(cp);
                }
            }
            var rnd_count = UnityEngine.Random.Range(desc.properties_num.Item1, desc.properties_num.Item2 + 1);
            if (rnd_count != 0)
            {
                List<uint> rnd_p = new List<uint>(desc.properties_rnd);

                for (int i = 0; i < rnd_count; i++)
                {
                    var id = UnityEngine.Random.Range(0, rnd_p.Count);

                    var cp = CharacterUtility.GetProperty(this, rnd_p[id]);
                    rnd_p.RemoveAt(id);

                    cp.Init();
                    character_properties.Add(cp);
                }
            }
        }

        public void Start()
        {
            foreach (var p in character_properties)
            {
                p.Start();
            }
        }

        public void AddState(CharacterState cs)
        {
            state = cs;
            state.start();
        }
    }
}
