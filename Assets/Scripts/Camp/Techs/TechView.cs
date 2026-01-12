using Foundations.MVVM;
using UnityEngine;
using TMPro;
using Commons;
using System.Linq;
using UnityEngine.UI;

namespace Camp.Techs
{
    public class TechView : MonoBehaviour, ITechView
    {
        public GameObject techInfoArea;
        public TextMeshProUGUI tech_name;
        public TextMeshProUGUI tech_desc;
        public TextMeshProUGUI tech_cost;

        public GameObject techOption_parent_node;

        public TechOption option_model;

        public Button btn_lock_mono;
        public TextMeshProUGUI btn_lock_mono_tx;

        Tech owner;

        //==================================================================================================

        void IModelView<Tech>.attach(Tech owner)
        {
            this.owner = owner;

            ref var tech_opr_list = ref CommonContext.instance.tech_opr_list;
            var options = techOption_parent_node.transform.GetComponentsInChildren<TechOption>();

            foreach (var option in options)
            {
                option.init();
                option.call();

                if (option.is_unlock && !tech_opr_list.Contains(option.tech_id))
                    tech_opr_list.Add(option.tech_id);
            }

            //根据表配置自动解锁
            if (tech_opr_list.Any())
            {
                foreach (var tech_opr_id in tech_opr_list)
                {
                    var e = techOption_parent_node.transform.GetComponentsInChildren<TechOption>().Where(t => t.tech_id == tech_opr_id);
                    if (!e.Any()) continue;

                    var _option = e.First();
                    _option.btn_show();
                    btn_unlock(false);
                }
            }

            techInfoArea.SetActive(false);
        }


        void IModelView<Tech>.detach(Tech owner)
        {
            this.owner = null;
        }


        void ITechView.notify_on_tick1()
        {
        }


        public void btn_close()
        {
            gameObject.SetActive(false);
        }


        public void btn_unlock(bool is_manual)
        {
            var option = owner.current_option;
            option.is_unlock = true;
            btn_lock_mono.gameObject.SetActive(false);
            option.call();

            if (is_manual)
            {
                CommonContext.instance.tech_opr_list.Add(option.tech_id);
                owner.do_afford();
            }

            var new_tech_id = owner._desc.set_tech_visible;
            if (new_tech_id == default) return;

            var new_option = Instantiate(option_model, option.transform.parent);
            new_option.tech_id = (int)new_tech_id;
            new_option.init();
            new_option.call();
        }


        public void notify_on_show_info()
        {
            techInfoArea.SetActive(false);

            var _desc = owner._desc;
            tech_name.text = Localization_Utility.get_localization(_desc.ui_name);
            tech_desc.text = Localization_Utility.get_localization(_desc.ui_desc);
            tech_cost.text = owner.calc_cost_str();

            btn_lock_mono.gameObject.SetActive(!owner.current_option.is_unlock);

            var can_afford = owner.valid_afford();
            btn_lock_mono.interactable = can_afford;
            btn_lock_mono_tx.text = can_afford ? "解锁" : "资源不足";

            techInfoArea.SetActive(true);  
        }
    }
}

