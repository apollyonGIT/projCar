using Commons;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace World.Caravans
{
    public class CaravanLever : MonoBehaviour, IDragHandler, IBeginDragHandler
    {
        #region Const
        private const float BRAKE_LEVER_INIT_HEIGHT = 0.2F;
        private const float BRAKE_LEVER_LERP_RANGE = 1F - BRAKE_LEVER_INIT_HEIGHT;
        private const float BRAKE_LEVER_UI_INTERACTION_TRIGGER_HEIGHT = 20F;
        #endregion

        public CaravanUiView cuv;
        public Slider lever_slider;
        public Image Lever;
        public Sprite LeverSprite_Highlight, LeverSprite_Ordinary;

        // -------------------------------------------------------------------------------

        private float start_drag_pos_y;
        private bool lever_triggered;

        // ===============================================================================

        public void tick()
        {
            // 2025.Aug.21 - 滑动条显示规则更新
            var lever_value = WorldContext.instance.driving_lever;

            if (lever_slider != null)
                lever_slider.value = lever_value > 0 ? BRAKE_LEVER_INIT_HEIGHT + lever_value * BRAKE_LEVER_LERP_RANGE : 0;

            Lever.sprite = lever_value > 0 ? LeverSprite_Highlight : LeverSprite_Ordinary;
        }

        // ===============================================================================

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            lever_triggered = false;
            start_drag_pos_y = InputController.instance.GetScreenMousePosition().y;    //此刻鼠标位置;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (lever_triggered)
                return;

            if (InputController.instance.GetScreenMousePosition().y < start_drag_pos_y - BRAKE_LEVER_UI_INTERACTION_TRIGGER_HEIGHT)
            {
                if (WorldContext.instance.driving_lever > 0)
                    Audio.AudioSystem.instance.PlayOneShot(Config.current.SE_car_brake);
                cuv.Brake();
                lever_triggered = true;
            }
        }

    }
}
