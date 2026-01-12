using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 可拖动的物体。只能沿着上下左右四个方向进行拖动。
    /// </summary>
    public class DevicePanelAttachment_Draggable : DevicePanelAttachment_Highlightable, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public enum Drag_Dir
        {
            Down_To_Up,
            Right_To_Left,
            Up_To_Down,
            Left_To_Right,
        }

        // -------------------------------------------------------------------------------

        public RectTransform topper_rt;
        public Drag_Dir drag_dir;

        public RectTransform Nullable_DecoMoveWithTopper;

        // -------------------------------------------------------------------------------

        protected Vector2 topper_pos_init;

        protected float topper_move_range_max;
        protected float topper_move_distance_abs;

        // -------------------------------------------------------------------------------

        protected RectTransform downer_rt;

        protected float pos_min;
        protected float pos_max;

        private bool has_nullable_deco;
        private Vector2 nullable_deco_init_delta_pos = Vector2.zero;

        protected bool _can_drag;

        private Device_Attachment_Triggerable_Spring DATS;

        // ===============================================================================

        public override void Init(List<Action> action = null)
        {
            base.Init(action);
            downer_rt = GetComponent<RectTransform>();

            topper_pos_init = topper_rt.anchoredPosition;

            has_nullable_deco = Nullable_DecoMoveWithTopper != null;
            if (has_nullable_deco)
                nullable_deco_init_delta_pos = Nullable_DecoMoveWithTopper.anchoredPosition - topper_rt.anchoredPosition;

            switch (drag_dir)
            {
                case Drag_Dir.Right_To_Left:
                case Drag_Dir.Left_To_Right:
                    topper_move_range_max = downer_rt.sizeDelta.x - topper_rt.sizeDelta.x;
                    break;
                case Drag_Dir.Down_To_Up:
                case Drag_Dir.Up_To_Down:
                    topper_move_range_max = downer_rt.sizeDelta.y - topper_rt.sizeDelta.y;
                    break;
            }

            switch (drag_dir)
            {
                case Drag_Dir.Right_To_Left:
                    pos_min = topper_pos_init.x - topper_move_range_max;
                    pos_max = topper_pos_init.x;
                    break;
                case Drag_Dir.Left_To_Right:
                    pos_min = topper_pos_init.x;
                    pos_max = topper_pos_init.x + topper_move_range_max;
                    break;
                case Drag_Dir.Up_To_Down:
                    pos_min = topper_pos_init.y - topper_move_range_max;
                    pos_max = topper_pos_init.y;
                    break;
                case Drag_Dir.Down_To_Up:
                default:
                    pos_min = topper_pos_init.y;
                    pos_max = topper_pos_init.y + topper_move_range_max;
                    break;
            }
        }

        protected override void Init_Actions(List<Action> action = null)
        {

        }

        public void Init_Draggable_Object_Position(float pos_01)
        {
            Forced_Set_Relative_Drag_Distance_01(pos_01);
        }

        // ===============================================================================

        public void Attach_Owner_For_Sync_Value(Device_Attachment_Triggerable_Spring dats)
        {
            DATS = dats;
            DATS.on_spring_moved += set_self;
        }

        // -------------------------------------------------------------------------------

        public void Detach_Owner_For_Sync_Value()
        {
            DATS.on_spring_moved -= set_self;
        }

        // -------------------------------------------------------------------------------

        private void set_self()
        {
            Forced_Set_Relative_Drag_Distance_01(DATS.Spring_Value_01);
        }

        // ===============================================================================

        public float Get_Relative_Drag_Distance_01()
        {
            return Mathf.Clamp01(topper_move_distance_abs / topper_move_range_max);
        }

        public void Forced_Set_Relative_Drag_Distance_01(float pos_01)
        {
            float relative_pos_abs = topper_move_range_max * pos_01;
            switch (drag_dir)
            {
                case Drag_Dir.Right_To_Left:
                    topper_rt.anchoredPosition = topper_pos_init - new Vector2(relative_pos_abs, 0);
                    topper_move_distance_abs = pos_max - topper_rt.anchoredPosition.x;
                    break;
                case Drag_Dir.Left_To_Right:
                    topper_rt.anchoredPosition = topper_pos_init + new Vector2(relative_pos_abs, 0);
                    topper_move_distance_abs = topper_rt.anchoredPosition.x - pos_min;
                    break;
                case Drag_Dir.Up_To_Down:
                    topper_rt.anchoredPosition = topper_pos_init - new Vector2(0, relative_pos_abs);
                    topper_move_distance_abs = pos_max - topper_rt.anchoredPosition.y;
                    break;
                case Drag_Dir.Down_To_Up:
                    topper_rt.anchoredPosition = topper_pos_init + new Vector2(0, relative_pos_abs);
                    topper_move_distance_abs = topper_rt.anchoredPosition.y - pos_min;
                    break;
            }
            deco_move_with_topper();
        }

        public void Forced_Drop_Drag()
        {
            _can_drag = false;
        }

        // ===============================================================================

        private void deco_move_with_topper()
        {
            if (has_nullable_deco)
                Nullable_DecoMoveWithTopper.anchoredPosition = topper_rt.anchoredPosition + nullable_deco_init_delta_pos;
        }

        // ===============================================================================

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            _can_drag = true;
            Player_Interacting = true;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!_can_drag)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(downer_rt, eventData.position, eventData.pressEventCamera, out var mouse_position);

            float new_pos_x;
            float new_pos_y;

            switch (drag_dir)
            {
                case Drag_Dir.Right_To_Left:
                    new_pos_x = Mathf.Clamp(mouse_position.x, pos_min, pos_max);
                    new_pos_y = topper_pos_init.y;
                    topper_move_distance_abs = pos_max - new_pos_x;
                    break;
                case Drag_Dir.Left_To_Right:
                    new_pos_x = Mathf.Clamp(mouse_position.x, pos_min, pos_max);
                    new_pos_y = topper_pos_init.y;
                    topper_move_distance_abs = new_pos_x - pos_min;
                    break;
                case Drag_Dir.Up_To_Down:
                    new_pos_x = topper_pos_init.x;
                    new_pos_y = Mathf.Clamp(mouse_position.y, pos_min, pos_max);
                    topper_move_distance_abs = pos_max - new_pos_y;
                    break;
                case Drag_Dir.Down_To_Up:
                default:
                    new_pos_x = topper_pos_init.x;
                    new_pos_y = Mathf.Clamp(mouse_position.y, pos_min, pos_max);
                    topper_move_distance_abs = new_pos_y - pos_min;
                    break;
            }

            topper_rt.anchoredPosition = new Vector2(new_pos_x, new_pos_y);
            deco_move_with_topper();
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            deco_move_with_topper();
            Player_Interacting = false;
        }
    }
}
