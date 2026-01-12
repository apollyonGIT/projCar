using UnityEngine;

namespace Foundations.Tickers
{
    public class Ticker_Mono : MonoBehaviour
    {
        //==================================================================================================

        public void init(float delta_time)
        {
            var ticker = Ticker._init();
            ticker.init(delta_time);

            ticker.can_start_tick = true;
        }


        private void Update()
        {
            Ticker.instance.update();
        }
    }
}

