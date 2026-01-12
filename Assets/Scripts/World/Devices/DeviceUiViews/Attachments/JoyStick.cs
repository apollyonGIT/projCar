using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews.Attachments
{
    public class JoyStick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerExitHandler, IPointerEnterHandler
    {
        public DeviceUiView duv;
        public Image jsContainer;
        public Image joystick;

        private bool in_area = false;
        private bool is_dragging = false;


        /// <summary>
        /// 在onDrag中执行逻辑没法保证帧数的统一
        /// </summary>
        /// <param name="ped"></param>
        public void OnDrag(PointerEventData ped)
        {
            if (ped.button != PointerEventData.InputButton.Right)
                return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle
                    (jsContainer.rectTransform,
                    ped.position,
                    ped.pressEventCamera,
                    out var position);

            if (in_area)
            {
                joystick.rectTransform.anchoredPosition = position;
            }
            else
            {
                var dir = position - Vector2.zero;
                joystick.rectTransform.anchoredPosition = dir.normalized * jsContainer.rectTransform.sizeDelta / 2;
            }
        }

        public void OnPointerDown(PointerEventData ped)
        {
            if (ped.button != PointerEventData.InputButton.Right)
                return;
            is_dragging = true;
            duv.JoyStickOper(is_dragging);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            in_area = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            in_area = false;
        }

        public void OnPointerUp(PointerEventData ped)
        {
            if (ped.button != PointerEventData.InputButton.Right)
                return;
            joystick.rectTransform.anchoredPosition = Vector3.zero;
            is_dragging = false;
            duv.JoyStickOper(is_dragging);
        }

        public void tick()
        {
            if (is_dragging)
            {
                if(joystick.transform.localPosition.magnitude > ((jsContainer.rectTransform.sizeDelta / 2).x) * 0.99f)
                {
                    duv?.OperateDrag(joystick.rectTransform.anchoredPosition);
                }
            }
        }

        public bool IsDraging()
        {
            return is_dragging;
        }

        public bool IsInArea()
        {
            return in_area;
        }

        public float GetDistance()
        {
            return joystick.transform.localPosition.magnitude;
        }

        public Vector2 GetDirection()
        {
            return joystick.transform.localPosition.normalized;
        }
    }
}
