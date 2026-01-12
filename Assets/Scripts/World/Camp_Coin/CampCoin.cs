using Commons;
using Foundations;
using System.Collections.Generic;

namespace World.Camp_Coin
{
    public class CampCoin : Singleton<CampCoin>
    {
        //public Dictionary<int, int> camp_coins = new Dictionary<int, int>();

        public void ShowCoins()
        {
            foreach (var kv in CommonContext.instance.camp_coins)
            {
                AutoCodes.camp_coins.TryGetValue(kv.Key.ToString(), out var camp_coin_desc);

                UnityEngine.Debug.Log($"{Localization_Utility.get_localization(camp_coin_desc.name)}  {kv.Value} ");
            }
        }
    }
}
