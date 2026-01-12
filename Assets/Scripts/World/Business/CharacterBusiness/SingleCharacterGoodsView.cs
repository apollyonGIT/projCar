using Addrs;
using AutoCodes;
using Commons;
using Foundations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Characters;

namespace World.Business
{
    public class SingleCharacterGoodsView : MonoBehaviour
    {
        public CharacterBusiness b;

        public Image icon;
        public TextMeshProUGUI name_text;

        public GoodPriceUi price_ui;

        public Transform properties_content;
        public PropertyDetail property_prefab;

        public GoodsData data;
        public CharacterGoodsInfoView goodsInfo;

        public GameObject view_content;


        public void Init(GoodsData g,CharacterBusiness b)
        {
            this.b = b;
            data = g;

            if(g == null)
            {
                view_content.SetActive(false);
                return;
            }
            else
            {
                view_content.SetActive(true);
            }


            roles.TryGetValue(g.goods_id.ToString(), out role c);
            name_text.text = Localization_Utility.get_localization(c.name);
            Addressable_Utility.try_load_asset<Sprite>(c.UI_Shop_Illustration_viewed, out var s);
            icon.sprite = s;

            loots.TryGetValue(g.price_id.ToString(), out loot l);
            Addressable_Utility.try_load_asset<Sprite>(l.view, out var s1);

            price_ui.Init(g.GetPrice(), g.discount);

            var character = (data.obj as Character);

            foreach (var p in character.character_properties)
            {
                propertiess.TryGetValue(p.desc.id.ToString(), out var p_record);
                var pp = Instantiate(property_prefab, properties_content, false);
                pp.Init(Localization_Utility.get_localization(p_record.name),Localization_Utility.get_localization(p_record.desc));
                pp.gameObject.SetActive(true);
            }

        }

        public void Buy()
        {
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            if (cmgr.characters.Count < 6)
            {
                if (b.BuyGood(data))
                {
                    cmgr.AddCharacter(data.obj as Character);
                }
            }
        }
    }
}
