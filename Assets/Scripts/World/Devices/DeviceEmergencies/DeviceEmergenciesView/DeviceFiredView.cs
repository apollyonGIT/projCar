using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using World.Widgets;

namespace World.Devices.DeviceEmergencies.DeviceEmergenciesView
{
    public class DeviceFiredView : DeviceEmergencyView,IPointerEnterHandler,IPointerExitHandler
    {
        public SingleFireView prefab;
        public List<SingleFireView> fire_list = new();

        public override void init()
        {
            init_fire();
        }

        public void init_fire()
        {
            var fire = owner as DeviceFired;
            var f = Instantiate(prefab, transform,false);
            var rect = GetComponent<RectTransform>().rect;
            var rnd_offset = new Vector2(fire.fire_pos.x * rect.width / 2, fire.fire_pos.y * rect.height / 2);

            f.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition + rnd_offset;
            fire_list.Add(f);

            f.gameObject.SetActive(true);
        }

        public override void tick()
        {
            if(fire_list.Count != 0 && owner is DeviceFired)
            {
                fire_list[0].fire_value = (owner as DeviceFired).fire_value;
            }
            foreach(var fire in fire_list)
            {
                fire.tick();
            }
        }

        public void ReduceFire(float delta)
        {
            if(owner is DeviceFired df)
            {
                if (df.ChangeFire(delta))
                {
                    Widget_Extinguisher_Context.instance.panel_in_fire = false;
                }
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Widget_Extinguisher_Context.instance.panel_in_fire = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            Widget_Extinguisher_Context.instance.panel_in_fire = false;
        }
    }
}
