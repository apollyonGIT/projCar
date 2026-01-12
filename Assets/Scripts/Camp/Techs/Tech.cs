using Commons;
using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;

namespace Camp.Techs
{
    public interface ITechView : IModelView<Tech>
    {
        void notify_on_tick1();

        void notify_on_show_info();
    }


    public class Tech : Model<Tech, ITechView>
    {
        public AutoCodes.tech _desc;

        public TechOption current_option;

        //==================================================================================================


        public void tick()
        {

        }


        public void tick1()
        {
            foreach (var view in views)
            {
                view.notify_on_tick1();
            }
        }


        public string calc_cost_str()
        {
            var ret = "";
            var cost_dic = _desc.cost;

            foreach (var (coin_id, num) in cost_dic)
            {
                AutoCodes.camp_coins.TryGetValue(coin_id.ToString(), out var r_camp_coin);
                var camp_coin_name = Localization_Utility.get_localization(r_camp_coin.name);
                ret += $"{camp_coin_name} x{num}\n\n";
            }

            return ret;
        }


        public bool valid_afford()
        {
            var camp_coins = CommonContext.instance.camp_coins;
            var cost_dic = _desc.cost;

            foreach (var (coin_id, num) in cost_dic)
            {
                var _coin_id = (int)coin_id;
                if (!camp_coins.TryGetValue(_coin_id, out var current_num)) return false;
                if (current_num < num) return false;
            }

            return true;
        }


        public void do_afford()
        {
            ref var camp_coins = ref CommonContext.instance.camp_coins;
            var cost_dic = _desc.cost;

            foreach (var (cost_coin_id, cost_num) in cost_dic)
            {
                camp_coins[(int)cost_coin_id] -= cost_num;
            }

            CampSceneRoot.instance.coinArea.call();
        }
    }
}





