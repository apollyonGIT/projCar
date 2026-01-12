using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.DeviceUiViews.Attachments;

namespace World.Devices.DeviceUiViews.DeviceUIView_AddOn
{
    public class DeviceUIViewAddOn_DShooter_Pistol : DeviceUIViewAddOn_DShooter
    {
        public Transform content;
        public TempBullet bullet_prefab;

        public List<TempBullet> bullet_list = new List<TempBullet>();

        public Slider reloading_start_slider;


        private int last_ammo_cnt;

        public override void attach(Device owner)
        {
            base.attach(owner);

            last_ammo_cnt = this.owner.current_ammo;
        }

        public override void notify_on_tick()
        {
            base.notify_on_tick();

            reloading_start_slider.value = owner.reload_start_process;

            trigger_for_shooting.Interactable = owner.CanShoot();
            trigger_for_shooting.Highlighted = owner.CanShoot();

            hold_for_reloading.Interactable = owner.CanReload();
            hold_for_reloading.Highlighted = owner.CanReload();

            if (owner.current_ammo > last_ammo_cnt)
            {
                //发生上弹
                var add_cnt = owner.current_ammo - last_ammo_cnt;

                for(int i=0; i<add_cnt; i++)
                {
                    var b = Instantiate(bullet_prefab, content,false);
                    b.gameObject.SetActive(true);
                    b.init(360);
                    bullet_list.Add(b);
                }
            }


            foreach(var b in bullet_list)
            {
                b.tick();
            }

            bullet_list.RemoveAll(b => 
            {
                if (b.current_tick <= 0)
                {
                    Destroy(b.gameObject);
                    return true;
                }
                return false;
            });


            last_ammo_cnt = owner.current_ammo;
        }

    }
}
