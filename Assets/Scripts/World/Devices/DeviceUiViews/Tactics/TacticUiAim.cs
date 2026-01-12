using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Devices.DeviceUiViews
{
    public class TacticUiAim : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public bool is_dragging = false;
        public TacticUiAimPanel aimPanel;

        //==================================================================================================

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            is_dragging = true;
        }


        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(aimPanel.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var position);
            GetComponent<RectTransform>().anchoredPosition = position;
        }


        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            is_dragging = false;           

            var owner = aimPanel.duv.owner;
            Vector2 target_pos = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 10));

            owner.GetType().GetMethod("on_outter_end_drag")?.Invoke(owner, new object[] { target_pos });

            //光标复位
            transform.localPosition = Vector3.zero;
        }


        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
        }

    }
}
