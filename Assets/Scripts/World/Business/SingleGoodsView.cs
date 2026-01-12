using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Devices;
using World.Devices.DeviceUpgrades;
using World.Devices.Equip;

namespace World.Business
{
    public class SingleGoodsView : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
    {
        public TextMeshProUGUI goods_name;

        public GoodPriceUi good_price;

        public Image device_rank_icon;
        public Image device_photo;

        public GoodsData goods_record;

        public DeviceBusinessView dbv;

        public Animation ani;
        private float time = 0;

        public List<GoodsCubicleView> cubicles_view = new();


        public DeviceGoodsInfoView goods_info;

        public EquipDeviceJobView jobs;

        public GameObject view_content;

        public void Init(GoodsData rec)
        {
            goods_record = rec;

            if(rec == null)
            {
                view_content.SetActive(false);
                Addressable_Utility.try_load_asset<Sprite>("ui_device_shop_shopboard_1", out var rank_icon);
                GetComponent<Image>().sprite = rank_icon;
                return ;
            }
            else
            {
                view_content.SetActive(true);
            }

            var device = (goods_record.obj as Device);
            var rank = device.desc.rank;
            device_ranks.TryGetValue(rank.ToString(), out var rank_db);

            if(goods_name!=null)
                goods_name.text = Localization_Utility.get_localization(device.desc.name);
            if(good_price!=null)
                good_price.Init(rec.GetPrice(),rec.discount);

            if (device != null)
            {    
                Addressable_Utility.try_load_asset<Sprite>(rank_db.img_info_board, out var rank_icon);
                GetComponent<Image>().sprite = rank_icon;

                if(jobs != null)
                {
                    jobs?.Init(device);
                }
            }

            if (device_rank_icon != null)
            {
                Addressable_Utility.try_load_asset<Sprite>(rank_db.img_name_bar, out var icon);
                device_rank_icon.sprite = icon;
            }

            if(device_photo != null)
            {
                Addressable_Utility.try_load_asset<Sprite>(device.desc.icon_photo, out var photo);
                device_photo.sprite = photo;
            }
            
            foreach(var cv in cubicles_view)
            {
                cv.gameObject.SetActive(false);
            }

            if(device.desc.cubicles != null)
            {
                var cub_cnt = device.desc.cubicles.Count;
                for (int i = 0; i < cub_cnt; i++)
                {
                    cubicles_view[i].gameObject.SetActive(true);
                    cubicles_view[i].Init(device.desc.cubicles[i]);
                }
            }

            ani.Play();
        }

        public void Buy()
        {
            Mission.instance.try_get_mgr("EquipMgr", out EquipmentMgr emgr);
            Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);

            if (dbv.main_panel.owner.BuyGood(goods_record))
            {

            }
        }

        public void ShowUpgrades()
        {
            var device = (goods_record.obj as Device);
            Addressable_Utility.try_load_asset<DeviceUpgradesView>("DeviceUpgradesView", out var upgrade_panel);
            var u_view = Instantiate(upgrade_panel, WorldSceneRoot.instance.uiRoot.transform, false);
            u_view.Init(device);
            u_view.IsReadOnly = true;
        }

        private void Update()
        {
            if (ani != null)
            {
                time += Time.unscaledDeltaTime;
                foreach (AnimationState anim in ani)
                {
                    if (ani.IsPlaying(anim.name))
                    {
                        anim.normalizedTime = time / anim.length;
                    }
                }
                ani.Sample();
            }
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (goods_info != null && goods_record!=null)
            {
                goods_info.gameObject.SetActive(true);
                goods_info.Init((goods_record.obj as Device));
            }
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (goods_info != null)
            {
                goods_info.gameObject.SetActive(false);
            }
        }
    }
}
