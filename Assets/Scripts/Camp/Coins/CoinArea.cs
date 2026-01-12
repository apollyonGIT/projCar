using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Camp.Coins
{
    public class CoinArea : MonoBehaviour
    {
        public TextMeshProUGUI[] infos;

        //==================================================================================================

        public void call()
        {
            var camp_coins = Commons.CommonContext.instance.camp_coins;
            var id = 6111;

            for (int i = 0; i < 4; i++)
            {
                camp_coins.TryGetValue(id + i, out var num);
                infos[i].text = $"{num}";
            }
            
        }
    }
}

