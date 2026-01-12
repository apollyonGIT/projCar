using UnityEngine;
using UnityEngine.EventSystems;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class ShieldUiAim : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public bool is_dragging = false;
        public ShieldUiAimPanel aimPanel;

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

            if (aimPanel.duv.owner is BasicShield bs)
                bs.Def_Start_By_UI_Control(Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 10 - WorldSceneRoot.instance.mainCamera.transform.position.z)));

            transform.localPosition = Vector3.zero;
        }
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        { }

    }
}
