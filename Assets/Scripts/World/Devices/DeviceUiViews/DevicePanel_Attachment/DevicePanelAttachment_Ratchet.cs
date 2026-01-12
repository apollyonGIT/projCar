using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    /// <summary>
    /// 棘轮。
    /// </summary>
    public abstract class DevicePanelAttachment_Ratchet : DevicePanelAttachment_Highlightable, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        #region Const
        private const float DRAG_DISTANCE_MAX = 100F; //曲柄产生最大拉力时的相对距离
        private const float DRAG_DISTANCE_DETERMINED_OUTPOWER = 1F / DRAG_DISTANCE_MAX;
        private const int DRAGGING_DELAY_TICKS_MAX = 60; //拖动停止后，仍可保持60帧的响应时间
        #endregion

        public Transform crank_rotate_center; // 曲柄基座

        // -------------------------------------------------------------------------------

        protected Transform dragging_point; // 曲柄头部
        protected Vector2 mouse_pos;

        // -------------------------------------------------------------------------------

        private int dragging_delay_ticks;
        private bool is_dragging_by_right_dir; // 是否正在正向拖动棘轮
        private Device_Attachment_Ratchet ratchet;

        // ===============================================================================

        /// <summary>
        /// 需要在Init()执行之前，在View中执行。否则Init()中的Synchronize无数据来源。
        /// </summary>
        /// <param name="ratchet_of_owner_device"></param>
        public void Pre_Init_Ratchet_Data(Device_Attachment_Ratchet ratchet_of_owner_device)
        {
            ratchet = ratchet_of_owner_device;
        }

        public override void Init(List<Action> action = null)
        {
            dragging_point = GetComponent<Transform>();
            Synchronize_Dir();
        }

        protected override void Init_Actions(List<Action> action)
        {

        }

        public void Synchronize_Dir()
        {
            crank_rotate_center.rotation = Quaternion.Euler(0, 0, ratchet.Dir_Angle);
        }

        // ===============================================================================

        /// <summary>
        /// 1.存在一定时长的“拖拽粘滞”。当玩家拖动时，“拖拽粘滞”会重置。当按下鼠标但未拖动时，“拖拽粘滞”会逐渐降低。
        /// 2.通过外部输入来决定棘轮正向转速的上限。
        /// 3.向外输出当前的功率值，功率输出范围0f~1f。
        /// </summary>
        /// <param name="speeding_up"></param>
        /// <param name="crank_rotate_speed_limit"></param>
        /// <param name="power_output01"></param>
        /// <returns>棘轮是否在有效转动。</returns>
        public bool Rotate_To_Output_Power_Per_Tick(bool only_one_dir, out float power_output01)
        {
            power_output01 = 0f;
            if (!is_dragging_by_right_dir && dragging_delay_ticks <= 0)
                return false;

            dragging_delay_ticks--;

            // 各元素在屏幕中的位置，取Vector2是为了把z轴置为0
            Vector2 dragging_start_pos = WorldSceneRoot.instance.uiCamera.WorldToScreenPoint(dragging_point.position);
            Vector2 ratchet_rotate_center_pos = WorldSceneRoot.instance.uiCamera.WorldToScreenPoint(crank_rotate_center.position);

            // 各元素的差向量
            var Center_to_startPos = dragging_start_pos - ratchet_rotate_center_pos;
            var Center_to_mousePos = mouse_pos - ratchet_rotate_center_pos;
            var cross = Vector3.Cross(Center_to_startPos, Center_to_mousePos);

            power_output01 = Mathf.Min(DRAG_DISTANCE_MAX, cross.magnitude / Center_to_startPos.magnitude) * DRAG_DISTANCE_DETERMINED_OUTPOWER;

            var anti_clockwise = cross.z >= 0;

            if (only_one_dir)
                is_dragging_by_right_dir = anti_clockwise;
            else
                power_output01 = anti_clockwise ? power_output01 : -power_output01;

            ratchet.rotate(power_output01, !only_one_dir || is_dragging_by_right_dir);

            return is_dragging_by_right_dir;
        }

        // ===============================================================================

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            mouse_pos = InputController.instance.GetScreenMousePosition();
            is_dragging_by_right_dir = true;
            Player_Interacting = true;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            mouse_pos = InputController.instance.GetScreenMousePosition();
            dragging_delay_ticks = DRAGGING_DELAY_TICKS_MAX;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            dragging_delay_ticks = 0;
            is_dragging_by_right_dir = false;
            Player_Interacting = false;
        }

    }
}
