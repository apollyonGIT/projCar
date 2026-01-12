using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace World.Devices.DeviceUiViews
{
    public class DevicePanelAttachment_DraggableNew : DevicePanelAttachment_Draggable
    {
        public Action value_changing;
        public Action end_changing;

        protected override void Init_Actions(List<Action> action = null)
        {
            base.Init_Actions(action);
            if (action.Count > 0)
                value_changing = action[0];
            if(action.Count > 1)
                end_changing += action[1];
        }

        public override void OnDrag(PointerEventData eventData)
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

            value_changing?.Invoke();
        }

        public float GetMoveAbsRate()
        {
            return topper_move_distance_abs/(pos_max - pos_min);
        }
    }
}
