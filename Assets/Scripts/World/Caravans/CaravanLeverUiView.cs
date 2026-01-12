
using Foundations.Tickers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Widgets;

namespace World.Caravans
{
    /// <summary>
    /// 2025.08.22 注：自从删掉了油门拉杆的物理交互，这个东西似乎就变得无用了。
    /// </summary>
    public class CaravanLeverUiView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        private const ushort DARG_STAGE = 6;
        private const int DRAG_TICKS_MAX = 168;
        private const int DRAG_TICKS_MAX_HALF = DRAG_TICKS_MAX / (DARG_STAGE + 1);
        private const float DRAG_LEVER_SPEED = 8e-6F;
        private const float DRAG_LEVER_SPEED_DOUBLE = DRAG_LEVER_SPEED * DARG_STAGE;

        private const float LOW_SPEED_BONUS_MAX = 5F;

        public Button ui_btn;
        public CaravanUiView caravanUiView;
        private Vector2 start_drag_pos;



        private int ticks_while_drag = 0;
        private float lever_up_speed = 0;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            /*
            start_drag_pos = InputController.instance.GetScreenMousePosition();

            Ticker.instance.do_when_tick_start += drag_tick;

            ticks_while_drag = 0;
            */
        }

        private void drag_tick_by_blower()
        {
            if (ticks_while_drag < DRAG_TICKS_MAX)
                ticks_while_drag++;

            var lever = WorldContext.instance.driving_lever;

            lever_up_speed += ticks_while_drag <= DRAG_TICKS_MAX_HALF ? DRAG_LEVER_SPEED_DOUBLE : -DRAG_LEVER_SPEED;
            if (lever_up_speed < 0)
                lever_up_speed = 0;
            var speed_bonue = Mathf.Max(1, (int)((1 - lever) * LOW_SPEED_BONUS_MAX));

            Widget_DrivingLever_Context.instance.Drag_Lever_by_Blower(lever + speed_bonue * lever_up_speed, true);
        }

        private void drag_tick_manually()
        {
            // 需要拉动拉杆的上一版本
            var current_drag_pos = InputController.instance.GetScreenMousePosition();

            float dy = current_drag_pos.y - start_drag_pos.y;
            Widget_DrivingLever_Context.instance.Drag_Lever(Mathf.Abs(dy) > 64f, dy > 0, true);
        }


        void IDragHandler.OnDrag(PointerEventData eventData)
        {

        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            //Ticker.instance.do_when_tick_start -= drag_tick;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Ticker.instance.do_when_tick_start += drag_tick_by_blower;
            ticks_while_drag = 0;
            lever_up_speed = 0f;
            ui_btn.interactable = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Ticker.instance.do_when_tick_start -= drag_tick_by_blower;
            ui_btn.interactable = true;
        }
    }
}
