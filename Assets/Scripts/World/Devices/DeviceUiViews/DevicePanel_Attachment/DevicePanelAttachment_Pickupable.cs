using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 可拿起并拖动的物体。
    /// 可用于制作用于控制近战武器的武器图标。
    /// </summary>
    public class DevicePanelAttachment_Pickupable : DevicePanelAttachment, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image dragged_object;
        public RectTransform canvans_rect;
        public GameObject Highlight_For_Point_Enter;

        // -------------------------------------------------------------------------------

        private RectTransform topper_rt;

        private Vector2 topper_pos_init;
        private Vector2 topper_pos_last_tick;

        private bool _picked_up;
        private bool _can_move = true;

        // ===============================================================================

        public override void Init(List<Action> action = null)
        {
            base.Init(action);
            topper_rt = dragged_object.GetComponent<RectTransform>();
            topper_pos_init = topper_rt.anchoredPosition;
            Highlight_For_Point_Enter.SetActive(false);
        }

        protected override void Init_Actions(List<Action> action = null)
        {

        }

        // ===============================================================================

        public bool Is_Picked_Up()
        {
            return _picked_up;
        }

        public Vector2 Get_Move_Distance_In_A_Tick()
        {
            return topper_rt.anchoredPosition - topper_pos_last_tick;
        }

        public bool Can_Move
        {
            get { return _can_move; }
            set
            {
                _can_move = value;
                topper_pos_last_tick = topper_rt.anchoredPosition;
            }
        }

        // ===============================================================================

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            topper_pos_last_tick = topper_rt.anchoredPosition;
            _picked_up = true;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (_can_move)
            {
                topper_pos_last_tick = topper_rt.anchoredPosition;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvans_rect, eventData.position, eventData.pressEventCamera, out var mouse_position);
                topper_rt.anchoredPosition = mouse_position;
            }
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            topper_rt.anchoredPosition = topper_pos_init;
            _picked_up = false;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            Highlight_For_Point_Enter.SetActive(true);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            Highlight_For_Point_Enter.SetActive(false);
        }
    }
}
