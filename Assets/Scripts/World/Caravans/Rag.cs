using Assets.Scripts.World.Widgets;
using Commons;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Devices.DeviceEmergencies.DeviceEmergenciesView;
using World.Widgets;

namespace World.Caravans
{
    public class Rag : MonoBehaviour,IPointerClickHandler
    {
        public Sprite rag_highlight;
        public Sprite rag_normal;
        public Sprite rag_holding;

        public void tick()
        {
            var ctx = Widget_Rag_Context.instance;

            if (ctx.holding)
            {
                GetComponent<Image>().sprite = rag_holding;
            }
            else if(ctx.need_highlight)
            {
                GetComponent<Image>().sprite = rag_highlight;
            }
            else
            {
                GetComponent<Image>().sprite = rag_normal;
            }


            if (ctx.holding)
            {
                var mousePosition = InputController.instance.GetScreenMousePosition();

                RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.transform as RectTransform, mousePosition, WorldSceneRoot.instance.uiCamera, out var pos);
                GetComponent<RectTransform>().anchoredPosition = pos;

                List<RaycastResult> results = new List<RaycastResult>();

                GraphicRaycaster gr = WorldSceneRoot.instance.uiRoot.GetComponent<GraphicRaycaster>();
                PointerEventData eventData = new PointerEventData(EventSystem.current)
                {
                    position = InputController.instance.GetScreenMousePosition()
                };
                gr.Raycast(eventData, results);

                foreach (var result in results)
                {
                    if (result.gameObject.TryGetComponent<SingleAcidView>(out var sav))
                    {
                        sav.ReduceAcid();
                    }
                }

            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            var ctx = Widget_Rag_Context.instance;

            if (!ctx.holding && eventData.button == PointerEventData.InputButton.Left)
            {
                ctx.holding = true;
            }

            if (ctx.holding && eventData.button == PointerEventData.InputButton.Right)
            {
                transform.localPosition = Vector3.zero;
                ctx.holding = false;
            }
        }
    }
}
