using System.Collections.Generic;
using UnityEngine;

namespace World.Devices.DeviceEmergencies
{
    public class SingleAcid
    {
        public Vector2 pos;
    }


    public class DeviceAcid : DeviceEmergency
    {
        public List<SingleAcid> acids = new List<SingleAcid>();

        private float def_cut = 0;

        private const int ACID_COUNT_MAX = 20;
        private const float UNIT_ACID_RATE = 0.05f;


        private void instantiate_acid()
        {
            SingleAcid acid = new SingleAcid();
            acid.pos = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            acids.Add(acid);
        }

        public DeviceAcid(Device owner, int acid_count)
        {
            this.owner = owner;

            acid_count = Mathf.Min(acid_count, ACID_COUNT_MAX);
            for(int i =0;i<acid_count; i++)
            {
                instantiate_acid();
            }
        }

        public override void start()
        {
            
        }

        public override void tick()
        {
            (owner as ITarget).def_cut_rate += def_cut;
            def_cut = acids.Count * UNIT_ACID_RATE;
            (owner as ITarget).def_cut_rate -= def_cut;

            if(acids.Count == 0)
            {
                removed = true;
            }

            base.tick();
        }

        public override void end()
        {
            
        }

        protected override void self_recover()
        {
            var rnd = Random.Range(0, acids.Count);
            RemoveAcid(acids[rnd]);
        }

        public void AddAcid(int count)
        {
            if(acids.Count + count >= ACID_COUNT_MAX)
            {
                count = ACID_COUNT_MAX - acids.Count;
            }
            
            for(int i = 0; i < count; i++)
            {
                instantiate_acid();
            }

            foreach(var view in views)
            {
                view.reinit();
            }    
        }

        public  void RemoveAcid(SingleAcid sa)
        {
            acids.Remove(sa);

            foreach (var view in views)
            {
                view.reinit();
            }
        }
    }
}
