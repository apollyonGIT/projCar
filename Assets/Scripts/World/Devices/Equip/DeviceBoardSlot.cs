using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Devices.DeviceUiViews.CubiclesUiView;

namespace World.Devices.Equip
{
    public class DeviceBoardSlot :MonoBehaviour
    {
        public string slot_name;

        public EquipDeviceJobView job_view;

        public TextMeshProUGUI device_text;

        public List<CubicleUiView> cubicles;

        public Image name_image;

        public void Init()
        {
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
            var slot_device = dmgr.slots_device.Find(t => t._name == slot_name);

            if(slot_device.slot_device == null)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                device_text.text = Localization_Utility.get_localization(slot_device.slot_device.desc.name);
                job_view.Init(slot_device);

                var rank = slot_device.slot_device.desc.rank;
                device_ranks.TryGetValue(rank.ToString(), out var rank_db);
                Addressable_Utility.try_load_asset<Sprite>(rank_db.img_name_bar, out var icon);
                name_image.sprite = icon;

                foreach (var cv in cubicles)
                {
                    cv.gameObject.SetActive(false);
                }

                for (int i = 0; i < slot_device.slot_device.cubicle_list.Count; i++) {
                    var cub = slot_device.slot_device.cubicle_list[i];
                    cub.add_view(cubicles[i]);
                    cubicles[i].gameObject.SetActive(true);
                }
            }
        }
    }
}
