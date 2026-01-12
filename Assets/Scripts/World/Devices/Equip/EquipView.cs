using Addrs;
using AutoCodes;
using Commons;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Business;
using World.Devices.DeviceUiViews;

namespace World.Devices.Equip
{
    public class EquipView : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public GameObject select;
        public Device data;

        [HideInInspector]
        public EquipmentMgr owner;
        public Image name_bar_image;
        public TextMeshProUGUI device_text;

        public DeviceHoveringInfo info;
        public DeviceGoodsInfoView goods_info;

        public GameObject drag_view;

        public Transform drag_content;
        private Transform origin_content;

        public SellMask sell_mask;

        public void Init(Device device,EquipmentMgr owner)
        {
            data = device;
            this.owner = owner;

            var rank = device.desc.rank;
            device_ranks.TryGetValue(rank.ToString(), out var rank_db);

            Addressable_Utility.try_load_asset<Sprite>(rank_db.img_name_bar, out var name_bar);

            if (data != null)
            {
                name_bar_image.sprite = name_bar;

                if(device_text!=null)
                    device_text.text = Localization_Utility.get_localization(device.desc.name);

                if (info!=null)
                    info.data = device;
            }
        }


        public void Select(bool b)
        {
            select.SetActive(b);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (goods_info != null && data!=null)
            {
                goods_info.gameObject.SetActive(true);
                goods_info.Init(data);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (goods_info != null && data != null)
            {
                goods_info.gameObject.SetActive(false);
                goods_info.Init(data);
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (data == null)
                return;

            owner.SelectDevice(data);

            drag_view.transform.GetComponent<Image>().maskable = false;
            sell_mask.gameObject.SetActive(true);

            sell_mask.Init(data);
            origin_content = drag_view.transform.parent;
            drag_view.transform.SetParent(drag_content);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (data == null)
                return;
            var mousePosition = InputController.instance.GetScreenMousePosition();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(drag_view.transform.parent.transform as RectTransform, mousePosition, WorldSceneRoot.instance.uiCamera, out var pos);
            drag_view.GetComponent<RectTransform>().anchoredPosition = pos;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (data == null)
                return;

            List<RaycastResult> results = new List<RaycastResult>();

            GraphicRaycaster graphicRaycaster = WorldSceneRoot.instance.uiRoot.GetComponent<GraphicRaycaster>();
            graphicRaycaster.Raycast(eventData, results);

            foreach (var result in results)
            {
                if (result.gameObject.TryGetComponent<SellMask>(out var sm))
                {
                    sm.Sell(data);
                    break;
                }

                if(result.gameObject.TryGetComponent<EquipSlot>(out var es))
                {
                    es.InstallDevice();
                }
            }

            drag_view.transform.SetParent(origin_content);
            drag_view.transform.localPosition = Vector3.zero;
            drag_view.transform.GetComponent<Image>().maskable = true;

            sell_mask.gameObject.SetActive(false);

            owner.SelectDevice(null);
        }
    }
}
