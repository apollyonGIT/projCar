using Commons;
using UnityEngine;
using System.Collections.Generic;

namespace World.Devices.DeviceEmergencies
{
    public class vine
    {
        public vine(float p1,float p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public float p1, p2;
        public List<vine> upper_vines = new();
    }

    //把周长抽象为100%

    public class DeviceBind : DeviceEmergency
    {
        public List<vine> vine_list = new();

        public const int max_vine_num = 100;
        public DeviceBind(Device owner,int vine_num)
        {
            this.owner = owner;

            for(int i = 0; i < vine_num; i++)
            {
                float p1 = Random.Range(0f, 100f);
                var rnd_dis = Random.Range(Config.current.vine_min_gap * 100, (1 - Config.current.vine_min_gap) * 100);
                var p2 = (p1 + rnd_dis) % 100;

                if (p1 > p2)
                {
                    float temp = p1;
                    p1 = p2;
                    p2 = temp;
                }

                vine new_v = new vine(p1, p2);
                foreach (var v in vine_list)
                {
                    if((new_v.p1<v.p1 && new_v.p2 > v.p1 && new_v.p2 < v.p2) || (new_v.p1 >v.p1 && new_v.p1 < v.p2 && new_v.p2> v.p2))
                    {
                        v.upper_vines.Add(new_v);
                    }
                }

                vine_list.Add(new_v);
            }
        }

        public override void start()
        {

        }

        public override void tick()
        {
            if (vine_list.Count == 0)
                removed = true;

            base.tick();
        }

        protected override void self_recover()
        {
            var rnd = Random.Range(0, vine_list.Count);
            RemoveVine(vine_list[rnd]);
        }

        public void RemoveVine(vine v)
        {
            if (v.upper_vines.Count != 0)
            {
                owner.Hurt((int)Config.current.vine_damage);
            }

            foreach(var vine in vine_list)
            {
                vine.upper_vines.Remove(v);
            }

            vine_list.Remove(v);

            foreach (var view in views)
            {
                view.reinit();
            }
        }

        public void AddVine(int vine_num)
        {
            for (int i = 0; i < vine_num; i++)
            {
                float p1 = Random.Range(0f, 100f);
                var rnd_dis = Random.Range(Config.current.vine_min_gap * 100, (1 - Config.current.vine_min_gap) * 100);
                var p2 = (p1 + rnd_dis) % 100;

                if (p1 > p2)
                {
                    float temp = p1;
                    p1 = p2;
                    p2 = temp;
                }

                vine new_v = new vine(p1, p2);
                foreach (var v in vine_list)
                {
                    if ((new_v.p1 < v.p1 && new_v.p2 > v.p1 && new_v.p2 < v.p2) || (new_v.p1 > v.p1 && new_v.p1 < v.p2 && new_v.p2 > v.p2))
                    {
                        v.upper_vines.Add(new_v);
                    }
                }

                vine_list.Add(new_v);
            }

            foreach(var view in views)
            {
                view.reinit();
            }
        }
    }
}
