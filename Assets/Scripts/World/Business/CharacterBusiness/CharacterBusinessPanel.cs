using AutoCodes;
using Commons;
using Foundations;
using UnityEngine;
using World.Characters;
using World.Devices.Equip;
using World.Helpers;

namespace World.Business
{
    public class CharacterBusinessPanel : MonoBehaviour
    {
        public CharacterMgrView character_mgr_view;

        public CharacterBusinessView character_business_view;

        public DeviceBoard deviceBoard;

        public Business owner;

        public void Init(uint business_id)
        {
            Mission.instance.try_get_mgr("BusinessMgr", out BusinessMgr bmgr);
            var character_business = new CharacterBusiness();
            bmgr.AddBusiness(character_business);

            character_business.add_view(character_business_view);

            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            cmgr.add_view(character_mgr_view);


            deviceBoard.Init();
            character_business.Init(business_id);
            character_business.purchase_limitation = 1;
            //character_business.SetRndFree();


            owner = character_business;
        }

        public void Close()
        {
            owner.remove_view(character_business_view);

            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            cmgr.remove_view(character_mgr_view);
        }

        public void Open()
        {
            deviceBoard.Init();
            character_business_view.coin_text.text = Safe_Area_Helper.GetLootCount((int)Commons.Config.current.coin_id).ToString();
        }
    }
}
