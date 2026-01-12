using Commons;
using Foundations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Devices;
using World.Helpers;
using World.Widgets;

namespace World.Caravans
{
    public class CaravanFixView : MonoBehaviour, IPointerClickHandler
    {
        public Transform content;

        private Vector2 target_pos;
        private float move_speed = 10;

        public Slider fix_cd_slider;
        public TextMeshProUGUI fix_text;

        public Image fix_image;
        public Image fix_image_highlight;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                if (Widget_Fix_Context.instance.current_fix_amount == 0)
                    return;

                Widget_Fix_Context.instance.player_oper = true;

                Widget_Fix_Context.instance.fix_cd = 0;

                List<RaycastResult> results = new List<RaycastResult>();

                GraphicRaycaster graphicRaycaster = WorldSceneRoot.instance.uiRoot.GetComponent<GraphicRaycaster>();
                graphicRaycaster.Raycast(eventData, results);

                foreach (var result in results)
                {
                    if (result.gameObject.TryGetComponent<IUiFix>(out IUiFix f))
                    {
                        Widget_Fix_Context.instance.Fix(f);

                        if(Widget_Fix_Context.instance.current_fix_amount == 0)
                        {
                            Widget_Fix_Context.instance.player_oper = false;
                            transform.localPosition = Vector2.zero;
                        }
                        return;
                    }
                }
            }
            else
            {
                Widget_Fix_Context.instance.player_oper = false;
                transform.localPosition = Vector2.zero;
            }
        }

        public void tick()
        {
            var player_oper = Widget_Fix_Context.instance.player_oper;
            if (player_oper)
            {
                var mousePosition = InputController.instance.GetScreenMousePosition();

                RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.transform as RectTransform,mousePosition,WorldSceneRoot.instance.uiCamera,out var pos);
                GetComponent<RectTransform>().anchoredPosition = pos;
            }
            else
            {
                if (Widget_Fix_Context.instance.fix_module.has_worker == false)
                {
                    target_pos = Vector2.zero;
                    transform.localPosition = Vector2.zero;
                    //没人又没有手动操作 睡觉得了
                }
                else
                {
                    if(Widget_Fix_Context.instance.fix_caravan == true)
                    {
                        target_pos = Ui_Pos_Helper.get_caravan_ui_pos();
                        move_to_target_pos();
                        try_to_fix();
                    }
                    else if(Widget_Fix_Context.instance.fix_device!=null)
                    {
                        var need_fix_device = Widget_Fix_Context.instance.fix_device;
                        target_pos = Ui_Pos_Helper.get_device_ui_pos(need_fix_device);
                        move_to_target_pos();
                        try_to_fix();
                    }
                }

            }

            fix_cd_slider.value = Widget_Fix_Context.instance.fix_cd;
            fix_cd_slider.maxValue = Widget_Fix_Context.instance.max_fix_cd;
            fix_text.text = $"{Widget_Fix_Context.instance.current_fix_amount}/{Widget_Fix_Context.instance.max_fix_amount}";

            bool need_fix;
            need_fix = false;
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            foreach (var slot in dmgr.slots_device)
            {
                var device = slot.slot_device;  
                if (device == null)
                    continue;
                if(device.current_hp != device.battle_data.hp)
                {
                    need_fix = true;
                    break;
                }
            }

            fix_image_highlight.color = need_fix? Color.white : Color.clear;
        }

        private void move_to_target_pos()
        {
            var dir = (new Vector3(target_pos.x,target_pos.y,transform.position.z) - transform.position).normalized;
            transform.localPosition += dir * move_speed ;
        }

        private void try_to_fix()
        {
            if((new Vector2(transform.position.x,transform.position.y) - target_pos).magnitude <= 1)
            {
                if(Widget_Fix_Context.instance.fix_caravan == true)
                {
                    var wctx = WorldContext.instance;

                    wctx.caravan_hp += (int)(wctx.caravan_hp_max * Config.current.fix_caravan_job_effect * Config.PHYSICS_TICK_DELTA_TIME);
                    wctx.caravan_hp = Mathf.Min(wctx.caravan_hp, wctx.caravan_hp_max);
                }
                else if(Widget_Fix_Context.instance.fix_device != null)
                {
                    Widget_Fix_Context.instance.fix_device.Fix(Mathf.CeilToInt(Widget_Fix_Context.instance.fix_device.battle_data.hp * Config.current.fix_device_job_effect * Config.PHYSICS_TICK_DELTA_TIME));
                }
            }
        }
    }
}
