using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Foundations;
using Commons;

namespace Camp.Techs
{
    public class TechOption : MonoBehaviour
    {
        public int tech_id;
        AutoCodes.tech _desc;

        internal bool is_unlock;

        public Color unlock_color;
        public Color lock_color;

        public Image bg;
        public TextMeshProUGUI tech_name;

        //==================================================================================================

        public void init()
        {
            AutoCodes.techs.TryGetValue($"{tech_id}", out _desc);
            tech_name.text = Localization_Utility.get_localization(_desc.ui_name);

            is_unlock = _desc.unlocked;
        }


        public void call()
        {
            bg.color = is_unlock ? unlock_color : lock_color;
        }


        public void btn_show()
        {
            Mission.instance.try_get_mgr("TechMgr", out TechMgr mgr);
            mgr.cell.current_option = this;
            mgr.show_info(tech_id);
        }
    }
}

