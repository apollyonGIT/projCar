using Foundations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Ui
{
    public class DragableUi : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public Transform window;

        private Vector3 offset;

        private float width, height;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(window.GetComponent<RectTransform>(), eventData.position, WorldSceneRoot.instance.uiCamera, out var pos);
            offset = window.GetComponent<RectTransform>().position - pos;

            width = window.GetComponent<RectTransform>().rect.width;
            height = window.GetComponent<RectTransform>().rect.height;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(window.GetComponent<RectTransform>(), eventData.position, WorldSceneRoot.instance.uiCamera, out var pos);
            window.GetComponent<RectTransform>().position = pos + offset;

           /* var anchoredPosition = window.GetComponent<RectTransform>().anchoredPosition;
            var tx = Mathf.Clamp(anchoredPosition.x, - Screen.width / 2 + width / 2 , Screen.width / 2 - width / 2 );
            var ty = Mathf.Clamp(anchoredPosition.y, 0 + height / 2 , Screen.height - height / 2 );
            window.GetComponent<RectTransform>().anchoredPosition = new Vector2(tx, ty);*/

            Share_DS.instance.add("device_ui_drag", true);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            Share_DS.instance.add("device_ui_drag", false);
        }
    }
}
