using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.Device_AI;

namespace World.Devices.DeviceUiViews.DeviceUIView_AddOn
{
    public class DeviceUIViewAddOn_DShooter_Gatlin : DeviceUIViewAddOn_Shooter
    {
        #region Const
        private const int RELOADER_BLINK_TICKS_MAX = 90; //装填时的最大闪烁时长
        #endregion


        public TextMeshProUGUI ammoText;

        public Image barrel;

        public DevicePanelAttachment_Trigger_Press reload_ammo_box;
        public DevicePanelAttachment_Trigger_Hold reload_ratchet;

        public DevicePanelAttachment_Ratchet_Crank widget_ratchet;

        protected new DScript_Shooter_Gatlin owner;


        private readonly DeviceUI_Component_Blinker reloader_blinker = new();
        private int reloader_blinker_ticks; // 用于闪烁的计时器


        public Transform ammo_content;
        public GameObject ammo_prefab;
        public List<GameObject> ammo_list = new();
        public Image reload_filling;

        private int view_carry_count = 0;

        public override void attach(Device owner)
        {
            base.attach(owner);
            //   ammo_belt.Init();
            reload_ammo_box.Init(new() { carry_ammo });
            reload_ratchet.Init(new() { });
            widget_ratchet.Pre_Init_Ratchet_Data(this.owner.ratchetShoot);
            widget_ratchet.Init();
        }

        protected override void attach_owner(Device device_owner)
        {
            base.attach_owner(device_owner);
            owner = device_owner as DScript_Shooter_Gatlin;
            owner.ratchetShoot.on_rotate += widget_ratchet.Synchronize_Dir;
        }

        protected override void attach_highlightable()
        {
            //autoWorkHighlight_reload.Add(reload_ammo_box);
            autoWorkHighlight_shoot.Add(widget_ratchet);
        }

        public override void detach()
        {
            base.detach();
            owner.ratchetShoot.on_rotate -= widget_ratchet.Synchronize_Dir;
        }


        public override void notify_on_tick()
        {
            base.notify_on_tick();

            // Trigger: Per Tick Check
            if (widget_ratchet.Rotate_To_Output_Power_Per_Tick(true, out var rotation_power))
                owner.Speed_Up_Spining(rotation_power);


            // Update View by Owner
            barrel.transform.rotation = Quaternion.Euler(0, 0, owner.Barrel_Rotation_Angle);
            ammoText.text = $"{owner.current_ammo}/{owner.fire_logic_data.capacity}";

            if (owner.carry_count > view_carry_count)
            {
                for (int i = view_carry_count; i < owner.carry_count; i++)
                {
                    var ammo_instance = GameObject.Instantiate(ammo_prefab, ammo_content, false);
                    ammo_instance.gameObject.SetActive(true);
                    ammo_list.Add(ammo_instance);
                }
            }
            else if (owner.carry_count < view_carry_count)
            {
                for (int i = view_carry_count - 1; i >= owner.carry_count; i--)
                {
                    GameObject.Destroy(ammo_list[i]);
                    ammo_list.RemoveAt(i);
                }

            }

            view_carry_count = owner.carry_count;

                // Update View by Panel Self 
            if (reloader_blinker_ticks > 0)
            {
                reloader_blinker_ticks--;
                reloader_blinker.update_blink(ref barrel, Color.red);
            }
            else if (reloader_blinker_ticks == 0)
            {
                reloader_blinker_ticks--;
                barrel.color = Color.white;
            }

            if (reload_ratchet.Is_Holding(false))
            {
                start_reloading();
            }
            // Set DevicePanelAttachment_Highlightable Status
            widget_ratchet.Highlighted = owner.current_ammo > 0;

            reload_filling.fillAmount = owner.reload_start_process;
        }

        // ===============================================================================


        private void carry_ammo()
        {
            owner.Try_Carrying();
        }
        private void blink_on_reload_banned()
        {
            reloader_blinker_ticks = RELOADER_BLINK_TICKS_MAX;
        }
    }
}
