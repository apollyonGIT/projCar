using Commons;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Devices.DeviceEmergencies.DeviceEmergenciesView;
using World.Widgets;

namespace World.Caravans
{
    public class Extinguisher : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Sprite extinguisher_highlight;
        public Sprite extinguisher_normal;
        public Sprite extinguisher_holding;
        public GameObject bubble;


        public bool press_extinguisher;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            var ctx = Widget_Extinguisher_Context.instance;

            if (!ctx.holding && eventData.button == PointerEventData.InputButton.Left)
            {
                GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 45f);
                ctx.holding = true;
            }

            if(ctx.holding && eventData.button == PointerEventData.InputButton.Right)
            {
                transform.localPosition = Vector3.zero;
                GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 0);
                ctx.holding = false;
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            press_extinguisher = true;
            bubble.gameObject.SetActive(true);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            press_extinguisher = false;
            bubble.gameObject.SetActive(false);
        }


        public void tick()
        {
            var ctx = Widget_Extinguisher_Context.instance;

            if (ctx.need_highlight)
            {
                GetComponent<Image>().sprite = extinguisher_highlight;
            }
            else 
            {
                GetComponent<Image>().sprite = Widget_Extinguisher_Context.instance.holding ? extinguisher_holding : extinguisher_normal;
            }

            if (ctx.holding)
            {
                var mousePosition = InputController.instance.GetScreenMousePosition();

                RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.transform as RectTransform, mousePosition, WorldSceneRoot.instance.uiCamera, out var pos);
                GetComponent<RectTransform>().anchoredPosition = pos;
            }


            if (press_extinguisher)
            {
                List<RaycastResult> results = new List<RaycastResult>();

                GraphicRaycaster gr= WorldSceneRoot.instance.uiRoot.GetComponent<GraphicRaycaster>();
                PointerEventData eventData = new PointerEventData(EventSystem.current)
                {
                    position = InputController.instance.GetScreenMousePosition()
                };
                gr.Raycast(eventData, results);

                foreach (var result in results)
                {
                    if (result.gameObject.TryGetComponent<SingleFireView>(out var sfv))
                    {
                        sfv.ReduceFire(-20 * Config.PHYSICS_TICK_DELTA_TIME);
                    }
                }
            }
        }
    }
}
