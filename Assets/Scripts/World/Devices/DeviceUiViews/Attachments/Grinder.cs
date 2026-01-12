using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews.Attachments
{
    public class Grinder : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private NewBasicMelee melee;
        private Vector2 last_tick_pos;

        private bool in_area;

        public Image blade;
        private Vector2 blade_pos_init;
        private float blade_y_max;
        private float blade_y_current;

        public void Init(NewBasicMelee bm)
        {
            melee = bm;
            blade_pos_init = blade.GetComponent<RectTransform>().anchoredPosition;
            blade_y_max = transform.GetComponent<RectTransform>().sizeDelta.y - blade.GetComponent<RectTransform>().sizeDelta.y; //刀片移动范围
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!in_area)
                return;

            if (!melee.Sharping)
                return;

            last_tick_pos = InputController.instance.GetScreenMousePosition();

            blade.gameObject.SetActive(true);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!in_area)
                return;

            if (!melee.Sharping)
                return;

            var current_pos = InputController.instance.GetScreenMousePosition();    //此刻鼠标位置
            var dy = current_pos.y - last_tick_pos.y;

            //RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.GetComponent<RectTransform>(), current_pos, WorldSceneRoot.instance.uiCamera, out var current_ui_pos);    //将屏幕坐标转换为UI坐标

            blade_y_current += dy;
            var y_effective = dy;

            if (blade_y_current > blade_y_max)
            {
                y_effective -= blade_y_current - blade_y_max;
                blade_y_current = blade_y_max;
            }
            else
            {
                y_effective = Mathf.Max(0, dy);
                blade_y_current = Mathf.Max(0, blade_y_current);
            }

            blade.GetComponent<RectTransform>().anchoredPosition = blade_pos_init + new Vector2(0, blade_y_current);

            melee.Sharp(y_effective);
            melee.Play_Or_End_SE_Grinding_By_UI(y_effective > 0);

            //[TO DO] IF y_effective > 0 : Play VFX

            last_tick_pos = InputController.instance.GetScreenMousePosition();
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            blade_y_current = 0;
            blade.GetComponent<RectTransform>().anchoredPosition = blade_pos_init;    //刀片回到初始位置
            melee.Play_Or_End_SE_Grinding_By_UI(false);
            if (!in_area)
                return;
            blade.gameObject.SetActive(false);

            if (!melee.Sharping)
                return;

            blade.gameObject.SetActive(false);
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
