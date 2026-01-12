using Assets.Scripts.World.Widgets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Devices.DeviceEmergencies.DeviceEmergenciesView
{
    public class DeviceAcidView : DeviceEmergencyView,IPointerEnterHandler,IPointerExitHandler
    {
        public SingleAcidView prefab;
        public List<SingleAcidView> acid_list = new();

        private void init_acid()
        {
            foreach (var a in acid_list)
            {
                Destroy(a.gameObject);
            }
            acid_list.Clear();

            var acid = owner as DeviceAcid;

            for(int i = 0; i < acid.acids.Count; i++)
            {
                var single_acid = acid.acids[i];

                var a = Instantiate(prefab, transform, false);
                var rect = GetComponent<RectTransform>().rect;
                var rnd_offset = new Vector2(single_acid.pos.x * rect.width/2,single_acid.pos.y * rect.height/2);

                a.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition + rnd_offset;
                a.gameObject.SetActive(true);
                a.Init(single_acid);

                acid_list.Add(a);
            }   
        }

        public override void init()
        {
            init_acid();
        }

        public override void reinit()
        {
            init_acid();
        }
        public override void tick()
        {

        }

        public void ReduceAcid(SingleAcidView sav)
        {
            var acid = owner as DeviceAcid;
            acid.RemoveAcid(sav.data);


            if(acid.acids.Count == 0)
                Widget_Rag_Context.instance.panel_in_acid = false;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Widget_Rag_Context.instance.panel_in_acid = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            Widget_Rag_Context.instance.panel_in_acid = false;
        }
    }
}
