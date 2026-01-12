using Commons;
using Foundations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using World.Devices.DeviceUiViews;

namespace World.Caravans
{
    public class CaravanHoveringInfo : MonoBehaviour, IAltUi, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject hovering_info;
        public TextMeshProUGUI caravan_name;
        public TextMeshProUGUI caravan_hp;
        public TextMeshProUGUI caravan_def;
        public TextMeshProUGUI caravan_weight;

        void IAltUi.Begin()
        {

        }

        void IAltUi.End()
        {

        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            hovering_info.SetActive(true);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            hovering_info.SetActive(false);
        }

        public void Init()
        {
            Mission.instance.try_get_mgr(Config.CaravanMgr_Name, out CaravanMgr cmgr);
            var desc = cmgr.cell._desc;
            caravan_name.text = Localization_Utility.get_localization(desc.name);
            caravan_hp.text = "耐久: " + desc.hp.ToString();
            caravan_def.text = "护甲: " + desc.def.ToString();
            caravan_weight.text = "重量: " + desc.weight.ToString() + " kg";
        }
    }
}
