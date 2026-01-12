using System.Collections.Generic;
using UnityEngine;

namespace World.Devices.DeviceEmergencies.DeviceEmergenciesView
{
    public class DeviceBindView : DeviceEmergencyView
    {
        private int view_count; //当前外观记录的藤蔓数

        public SingleBindView prefab;
        public List<SingleBindView> vine_list = new();

        public override void init()
        {
            init_bind();
        }

        public override void tick()
        {
            foreach(var vine in vine_list)
            {
                vine.tick();
            }
        }

        public override void reinit()
        {
            init_bind();
        }

        private void init_bind()
        {
            foreach(var vine in vine_list)
            {
                Destroy(vine.gameObject);
            }

            vine_list.Clear();


            var bind = owner as DeviceBind;
            view_count = bind.vine_list.Count;
            for (int i = 0; i < view_count; i++)
            {
                set_vine(bind.vine_list[i]);
            }
        }

        private void set_vine(vine v)
        {
            var c =  GetComponent<RectTransform>().rect.width *2 + GetComponent<RectTransform>().rect.height * 2;

            var vp1 = v.p1 * c / 100f;
            var vp2 = v.p2 * c / 100f;
            var point1 = get_point(vp1);
            var point2 = get_point(vp2);

            var b = Instantiate(prefab, transform, false);
            b.gameObject.SetActive(true);
            b.Init(point1, point2,v,this);

            vine_list.Add(b);
        }

        private Vector2 get_point (float vp1)
        {
            Vector2 point1;
            if (vp1 < GetComponent<RectTransform>().rect.height)
            {
                point1 = new Vector2(0, vp1);
            }
            else if (vp1 < GetComponent<RectTransform>().rect.width + GetComponent<RectTransform>().rect.height)
            {
                point1 = new Vector2(vp1 - GetComponent<RectTransform>().rect.height, GetComponent<RectTransform>().rect.height);
            }
            else if (vp1 < GetComponent<RectTransform>().rect.height * 2 + GetComponent<RectTransform>().rect.width)
            {
                point1 = new Vector2(GetComponent<RectTransform>().rect.width, GetComponent<RectTransform>().rect.width + GetComponent<RectTransform>().rect.height * 2 - vp1);
            }
            else
            {
                point1 = new Vector2(GetComponent<RectTransform>().rect.width * 2 + GetComponent<RectTransform>().rect.height * 2 - vp1, 0);
            }
            return point1;
        }
    }
}
