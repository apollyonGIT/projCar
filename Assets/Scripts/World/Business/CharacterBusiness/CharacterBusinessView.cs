using Commons;
using Foundations.MVVM;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Helpers;

namespace World.Business
{
    public class CharacterBusinessView : MonoBehaviour, IBusinessView,IPointerClickHandler
    {
        public CharacterBusiness business_owner;

        public Transform business_content;

        public SingleCharacterGoodsView character_goods_prefab;

        public List<SingleCharacterGoodsView> character_goods_list = new();

        public Image coin_image;
        public TextMeshProUGUI coin_text;

        public LootBusinessView lbv;

        public GameObject limit_obj;

        #region BusinessView
        void IModelView<Business>.attach(Business owner)
        {
            business_owner = owner as CharacterBusiness;
        }

        void IModelView<Business>.detach(Business owner)
        {
            if (business_owner != null)
                business_owner = null;
            Destroy(gameObject);
        }

        void IBusinessView.init()
        {
            limit_obj.SetActive(false);
            coin_text.text = Safe_Area_Helper.GetLootCount((int)Config.current.coin_id).ToString();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            // Safe_Area_Helper.TryToSellCharacter(business_owner);
        }

        void IBusinessView.update_goods()
        {
            if(business_owner.purchase_limitation <= 0)
            {
                if(limit_obj.activeInHierarchy == false)
                {
                    limit_obj.SetActive(true);
                }
            }


            foreach (var cg in character_goods_list)
            {
                Destroy(cg.gameObject);
            }
            character_goods_list.Clear();

            foreach (var g in business_owner.goods)
            {
                var cgp = Instantiate(character_goods_prefab, business_content, false);
                cgp.Init(g, business_owner);
                cgp.gameObject.SetActive(true);
                character_goods_list.Add(cgp);
            }

            coin_text.text = Safe_Area_Helper.GetLootCount((int)Config.current.coin_id).ToString();
        }
        #endregion
    }
}
