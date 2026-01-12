using Spine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Widgets;

namespace World.CaravanBoards
{
    public class BellowView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Animator anim;

        #region Const
        private const float FLAME_ALPHA_COEF = 5E2F;
        private const float FLAME_ALPHA_COEF_BY_LEVER = 0.4F;
        private const float SMOOTH_DAMP_TIME = 0.09F;
        #endregion

        public Image Oven_Flame;

        private float flame_alpha;
        private float flame_alpha_v;

        //==================================================================================================

        private void Start()
        {
            anim.speed = 0;
        }


        public void tick()
        {
            var target_flame_alpha_by_blower = Widget_Blower_Context.instance.lever_up_rate * FLAME_ALPHA_COEF;
            var target_flame_alpha_by_lever = WorldContext.instance.driving_lever * FLAME_ALPHA_COEF_BY_LEVER;
            var target_flame_alpha = target_flame_alpha_by_lever + target_flame_alpha_by_blower;

            flame_alpha = Mathf.SmoothDamp(flame_alpha, target_flame_alpha, ref flame_alpha_v, SMOOTH_DAMP_TIME);
            Oven_Flame.color = new Color(1, 1, 1, Mathf.Clamp01(flame_alpha));
        }


        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            Widget_Blower_Context.instance.notify_on_start_blower();

            anim.StopPlayback();
            anim.speed = 1f;
            anim.Play(0);
        }


        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            Widget_Blower_Context.instance.notify_on_end_blower();

            anim.StartPlayback();
            anim.speed = -2f;
            anim.Play(0);
        }

    }
}
