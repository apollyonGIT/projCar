using AutoCodes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews
{
    public class BasicShooterUiView : DeviceUiView
    {
        private const int UI_AMMO_MAX = 6;
        private const short BLINK_TICK_MAX = 15;

        public TextMeshProUGUI ammoText;

        public Image reloading_progress_base;
        public Image reloading_progress;

        public Button btn_reload;

        public Image ammo_slot;
        public List<Image> ammo;

        public Image ammo_slot_continuous_base;
        public Image ammo_slot_continuous;

        new private NewBasicShooter owner;

        private short blink_tick;
        private bool blink_on;
        private Image btn_bg;

        private enum Ammo_Capacity_Type
        {
            Discrete,
            Continuous
        }
        private Ammo_Capacity_Type ammo_capacity_type;

        public override void attach(Device owner)
        {
            base.attach(owner);
            this.owner = base.owner as NewBasicShooter;

            btn_bg = btn_reload.GetComponent<Image>();

            fire_logics.TryGetValue(owner.desc.fire_logic.ToString(), out var record);
            var max_ammo = (int)record.capacity;

            set_Ammo_Clip_Type(max_ammo);
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
                ammoText.text = $"{owner.Current_Ammo}/{owner.Max_Ammo}";
            }
            else
            {
                ammoText.text = $"{owner.Current_Ammo}/{owner.Max_Ammo}";
                if (owner.Current_Ammo == 0)
                    btn_blink();
                else
                    btn_bg.color = Color.white;
            }

            reloading_progress_base.gameObject.SetActive(owner.reloading);
            reloading_progress.fillAmount = owner.Reloading_Process;


            if (check_Ammo_Clip_Type(owner.Max_Ammo))
            {
                switch (ammo_capacity_type)
                {
                    case Ammo_Capacity_Type.Discrete:
                        for (int i = 0; i < owner.Max_Ammo; i++)
                            ammo[i].color = i < owner.Current_Ammo ? Color.white : Color.clear;
                        break;
                    case Ammo_Capacity_Type.Continuous:
                        ammo_slot_continuous.fillAmount = owner.Current_Ammo * 20 / owner.Max_Ammo * 0.05f;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                set_Ammo_Clip_Type(owner.Max_Ammo);
            }
        }

        public void reloading_start()
        {
            if (owner.Current_Ammo < owner.Max_Ammo)
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

        private bool check_Ammo_Clip_Type(int max_ammo)
        {
            switch (ammo_capacity_type)
            {
                case Ammo_Capacity_Type.Discrete:
                    if (max_ammo <= UI_AMMO_MAX)
                        return true;
                    break;
                case Ammo_Capacity_Type.Continuous:
                    if (max_ammo > UI_AMMO_MAX)
                        return true;
                    break;
            }
            return false;
        }

        private void set_Ammo_Clip_Type(int max_ammo)
        {                                  
            if (max_ammo <= UI_AMMO_MAX)
            {
                ammo_capacity_type = Ammo_Capacity_Type.Discrete;
                ammo_slot.gameObject.SetActive(true);
                ammo_slot_continuous_base.gameObject.SetActive(false);

                for (int i = 0; i < UI_AMMO_MAX; i++)
                    ammo[i].gameObject.SetActive(i < max_ammo);

                ammoText.gameObject.SetActive(false);
            }
            else
            {
                ammo_capacity_type = Ammo_Capacity_Type.Continuous;
                ammo_slot.gameObject.SetActive(false);
                ammo_slot_continuous_base.gameObject.SetActive(true);

                ammoText.gameObject.SetActive(true);
            }
        }

    }
}
