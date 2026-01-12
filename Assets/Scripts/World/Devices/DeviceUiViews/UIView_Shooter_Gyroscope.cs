using AutoCodes;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.DeviceUiViews.Attachments;

namespace World.Devices.DeviceUiViews
{
    public class UIView_Shooter_Gyroscope : DeviceUiView, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        private const int UI_AMMO_MAX = 6;
        private const short BLINK_TICK_MAX = 15;

        private const float DRAG_SPEED_PER_TICK = 0.17F;
        private const float DRAG_FACTOR_COEF = 1.007F;


        public TextMeshProUGUI ammoText;

        public Image reloading_progress_base;
        public Image reloading_progress;

        public Button btn_reload;

        public Image ammo_slot;
        public List<Image> ammo;

        new private Shooter_Gyroscope owner;

        private short blink_tick;
        private bool blink_on;
        private Image btn_bg;
        private bool ammo_ui_exact_num;


        public Image energy_stick;
        public Image invisible_container;
        public Image rope_line;
        public Slider indicator_factor;
        public JoyStick joyStick;

        private float length_current;
        private float length_expected;
        private float length_max;
        private Vector2 stick_dir;

        private bool is_dragging;

        public override void attach(Device owner)
        {
            base.attach(owner);
            this.owner = base.owner as Shooter_Gyroscope;

            btn_bg = btn_reload.GetComponent<Image>();

            fire_logics.TryGetValue(owner.desc.fire_logic.ToString(), out var record);
            var max_ammo = (int)record.capacity;
            ammo_ui_exact_num = max_ammo <= UI_AMMO_MAX;
            if (ammo_ui_exact_num)
                ammo_slot.rectTransform.sizeDelta = new Vector2(max_ammo * 18 + 6, ammo_slot.rectTransform.sizeDelta.y);

            length_max = invisible_container.rectTransform.sizeDelta.x * 0.5f;
            indicator_factor.maxValue = Shooter_Gyroscope.FACTOR_MAX;
        }
        public override void init()
        {
            base.init();
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            if (owner.reloading)
            {
                btn_reload.gameObject.SetActive(false);
               
                ammoText.text = $"{owner.Current_Ammo}/{owner.Max_Ammo}";
            }
            else
            {
                ammoText.text = $"{owner.Current_Ammo}/{owner.Max_Ammo}";
                btn_reload.gameObject.SetActive(owner.Current_Ammo < owner.Max_Ammo);
                if (owner.Current_Ammo == 0)
                    btn_blink();
                else
                    btn_bg.color = Color.white;
            }

            reloading_progress_base.gameObject.SetActive(owner.reloading);
            reloading_progress.fillAmount = owner.Reloading_Process;


            if (ammo_ui_exact_num)
            {
                for (int i = 0; i < owner.Max_Ammo; i++)
                    if (ammo[i].gameObject.activeSelf != (i < owner.Current_Ammo))
                        ammo[i].gameObject.SetActive(!ammo[i].gameObject.activeSelf);
            }
            else
            {
                var ammo_showed = UI_AMMO_MAX * owner.Current_Ammo / owner.Max_Ammo;
                for (int i = 0; i < UI_AMMO_MAX; i++)
                    if (ammo[i].gameObject.activeSelf != (i < ammo_showed))
                        ammo[i].gameObject.SetActive(!ammo[i].gameObject.activeSelf);
            }


            if (is_dragging)
            {
                if (length_current < length_expected)
                {
                    length_current += DRAG_SPEED_PER_TICK * owner.attack_factor;
                    owner.UI_Control_Mul_Factor(DRAG_FACTOR_COEF);
                    rope_line.color = Color.green;
                }
                else
                {
                    length_current = length_expected;
                    rope_line.color = Color.white;
                }

                if (length_current >= length_max)
                    rope_line.color = Color.white;

                energy_stick.rectTransform.anchoredPosition = stick_dir * length_current;
            }

            rope_line.rectTransform.sizeDelta = new Vector2(length_current, rope_line.rectTransform.sizeDelta.y);
            rope_line.rectTransform.localRotation = Quaternion.FromToRotation(Vector3.right, stick_dir);

            indicator_factor.value = owner.attack_factor;
        }

        public void reloading_start()
        {
            owner.UI_Controlled_Reloading();
        }

        private void btn_blink()
        {
            if (--blink_tick <= 0)
            {
                blink_tick = BLINK_TICK_MAX;
                blink_on = !blink_on;
            }

            btn_bg.color = blink_on ? Color.red : new Color(255f, 129f, 129f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
                return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle
                    (invisible_container.rectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var position);

            stick_dir = position.normalized;
            length_expected = Mathf.Min(position.magnitude, length_max);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
                return;
            is_dragging = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Right)
                return;
            energy_stick.rectTransform.anchoredPosition = Vector3.zero;
            length_current = 0;
            is_dragging = false;
            rope_line.color = Color.white;
        }
    }
}
