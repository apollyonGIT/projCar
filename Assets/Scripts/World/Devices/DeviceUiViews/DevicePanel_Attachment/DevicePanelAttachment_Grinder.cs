using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 磨刀机/磨石
    /// 使用时，请将此脚本挂载在Prefab中不需要拖动的磨石基座上。
    /// </summary>
    public class DevicePanelAttachment_Grinder : DevicePanelAttachment, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public enum Grinder_Dir
        {
            Down_To_Up,
            Right_To_Left,
        }

        // -------------------------------------------------------------------------------

        public Image top_object;
        public Grinder_Dir grinder_dir = Grinder_Dir.Down_To_Up;

        // -------------------------------------------------------------------------------

        private RectTransform downer_rt;
        private RectTransform topper_rt;

        private Vector2 topper_pos_init;
        private Vector2 last_tick_pos;

        private bool in_area;

        private float topper_move_range_max;
        private float topper_xory_current;
        private float effective_move_distance;

        private Action dragging_action;
        private Action end_drag_action;

        private float accumulated_grind;
        private bool fetch_top_object_from_base;

        // ===============================================================================

        public override void Init(List<Action> action)
        {
            base.Init (action);
            topper_rt = top_object.GetComponent<RectTransform>();
            downer_rt = GetComponent<RectTransform>();

            topper_pos_init = topper_rt.anchoredPosition;

            switch (grinder_dir)
            {
                case Grinder_Dir.Right_To_Left:
                    topper_move_range_max = Mathf.Abs(downer_rt.sizeDelta.x - topper_rt.sizeDelta.x); //刀片移动范围
                    break;
                case Grinder_Dir.Down_To_Up:
                default:
                    topper_move_range_max = downer_rt.sizeDelta.y - topper_rt.sizeDelta.y; //刀片移动范围
                    break;
            }
        }

        protected override void Init_Actions(List<Action> action)
        {
            dragging_action += action[0];
            end_drag_action += action[1];
        }

        // ===============================================================================

        public void Fetch_Top_Object_From_Base(bool unlock_it)
        {
            // How to Use: 直接调用此方法来解锁顶部物件的自由移动。
            fetch_top_object_from_base = unlock_it;
        }

        public float Get_Accumulated_Grind(bool keep_accumulated_grind_value = false)
        {
            if (keep_accumulated_grind_value)
                return accumulated_grind;
            var temp = accumulated_grind; //获取当前累积的磨削量
            accumulated_grind = 0; //重置累积的磨削量
            return temp; //返回当前累积的磨削量
        }

        // ===============================================================================

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            topper_xory_current = 0;
            accumulated_grind = 0;
            if (in_area)
                last_tick_pos = InputController.instance.GetScreenMousePosition();
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (fetch_top_object_from_base)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(downer_rt, eventData.position, eventData.pressEventCamera,
                    out var position);
                topper_rt.anchoredPosition = position;
                return;
            }

            if (!in_area)
                return;

            var current_pos = InputController.instance.GetScreenMousePosition();    //此刻鼠标位置
            float ds = 0f;

            switch (grinder_dir)
            {
                case Grinder_Dir.Right_To_Left:
                    ds = -(current_pos.x - last_tick_pos.x);  //从右向左意味着是负数，需要取反
                    topper_xory_current += ds;
                    effective_move_distance = ds;

                    if (topper_xory_current > topper_move_range_max)
                    {
                        effective_move_distance -= topper_xory_current - topper_move_range_max;
                        topper_xory_current = topper_move_range_max;
                    }
                    else
                    {
                        effective_move_distance = Mathf.Max(0, ds);
                        topper_xory_current = Mathf.Max(0, topper_xory_current);
                    }

                    topper_xory_current = -topper_xory_current;//计算坐标时再将取反回正
                    topper_rt.anchoredPosition = topper_pos_init + new Vector2(topper_xory_current, 0);
                    break;
                case Grinder_Dir.Down_To_Up:
                default:
                    ds = current_pos.y - last_tick_pos.y;
                    topper_xory_current += ds;
                    effective_move_distance = ds;

                    if (topper_xory_current > topper_move_range_max)
                    {
                        effective_move_distance -= topper_xory_current - topper_move_range_max;
                        topper_xory_current = topper_move_range_max;
                    }
                    else
                    {
                        effective_move_distance = Mathf.Max(0, ds);
                        topper_xory_current = Mathf.Max(0, topper_xory_current);
                    }

                    topper_rt.anchoredPosition = topper_pos_init + new Vector2(0, topper_xory_current);
                    break;
            }

            accumulated_grind += effective_move_distance;
            last_tick_pos = current_pos;
            dragging_action?.Invoke(); //调用拖动动作
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            topper_xory_current = 0;
            topper_rt.anchoredPosition = topper_pos_init;    //顶部部件回到初始位置
            accumulated_grind = 0;
            fetch_top_object_from_base = false;
            end_drag_action?.Invoke();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            in_area = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            in_area = false;
        }
    }
}
