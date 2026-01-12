using AutoCodes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews
{
    public class UIView_Shooter_Trebuchet : DeviceUiView
    {
        public TextMeshProUGUI ammoText;
        public Image ammo_progress;
        public GameObject ready_light;
        public GameObject ammo_reloading_indicator;
        public Button btn_reload;
        public Button btn_shoot;
        public Slider dir_controller;
        public Image dir_to_left;
        public Image dir_to_right;

        new private Shooter_Trebuchet_Outdated owner;

        public override void attach(Device owner)
        {
            base.attach(owner);
            if(!(owner is Shooter_Trebuchet_Outdated))
            {
                Debug.Log($"owner is {owner}");
            }
            this.owner = owner as Shooter_Trebuchet_Outdated;

            fire_logics.TryGetValue(owner.desc.fire_logic.ToString(), out var record);
            var max_ammo = (int)record.capacity;

            dir_controller.value = this.owner.Get_Dir_X();
            set_ui_by_dir();
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
                ready_light.SetActive(false);
                ammo_reloading_indicator.SetActive(true);
                
                ammoText.text = $"{owner.Current_Ammo}/{owner.Max_Ammo}";
            }
            else
            {
                ammoText.text = $"{owner.Current_Ammo}/{owner.Max_Ammo}";
                ready_light.SetActive(true);
                ammo_reloading_indicator.SetActive(false);
            }

            ammo_progress.fillAmount = owner.Reloading_Process;

            var has_ammo = owner.Current_Ammo > 0;

            if ((has_ammo || owner.reloading) == btn_reload.gameObject.activeSelf)
                btn_reload.gameObject.SetActive(!btn_reload.gameObject.activeSelf);

            if (has_ammo ^ btn_shoot.gameObject.activeSelf)
                btn_shoot.gameObject.SetActive(!btn_shoot.gameObject.activeSelf);

            set_ui_by_dir();
        }

        public void UI_Trigger_Reload()
        {
            owner.UI_Controlled_Reloading();
        }

        public void UI_Trigger_Shoot()
        {
            owner.UI_Controlled_Shooting();
        }

        public void UI_Trigger_Turn_Dir()
        {
            owner.UI_Controlled_Turn_Dir(dir_controller.value);
        }

        private void set_ui_by_dir()
        {
            var to_right = dir_controller.value >= 0;
            dir_controller.value = to_right ? 1 : -1;
            dir_to_left.gameObject.SetActive(!to_right);
            dir_to_right.gameObject.SetActive(to_right);
        }
    }
}
