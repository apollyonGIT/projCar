using Foundations;
using Foundations.MVVM;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Caravans;

namespace World.Devices.Equip
{
    public class EquipmentMgrView : MonoBehaviour, IEquipmentMgrView
    {
        public EquipmentMgr owner;

        public RawImage caravan_image;
        public List<EquipSlot> slots = new();

        public TextMeshProUGUI caravan_text;

        public bool can_install = true;
        void IEquipmentMgrView.add_device(Device device)
        {

        }

        void IModelView<EquipmentMgr>.attach(EquipmentMgr owner)
        {
            this.owner = owner;
        }

        void IModelView<EquipmentMgr>.detach(EquipmentMgr owner)
        {
            this.owner = null;
            Destroy(gameObject);
        }

        void IEquipmentMgrView.init()
        {
            Mission.instance.try_get_mgr("CaravanMgr", out CaravanMgr cmgr);

            if (caravan_text != null)
                caravan_text.text = Commons.Localization_Utility.get_localization(cmgr.cell._desc.name);

            foreach (var slot in slots)
            {
                slot.init();
            }
        }

        void IEquipmentMgrView.remove_device(Device device)
        {

        }

        void IEquipmentMgrView.select_device(Device device)
        {
            foreach (var slot in slots)
            {
                slot.update_slot();
            }
        }

        void IEquipmentMgrView.tick()
        {

        }

        void IEquipmentMgrView.update_slot()
        {
            foreach (var slot in slots)
            {
                slot.update_slot();
            }
        }
    }
}
