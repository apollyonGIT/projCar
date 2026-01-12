using Commons;
using Foundations;
using Foundations.Tickers;
using World.Caravans;
using World.Devices;
using World.Devices.Device_AI;

namespace World.Widgets
{

    /// <summary>
    /// 此处逻辑为   因为车体ui逻辑比较特殊，全部单独拎出来作为小的Model单独处理
    /// </summary>
    public class Widget_Fix_Context : Singleton<Widget_Fix_Context>
    {
        public CaravanFixModule fix_module;

        public bool player_oper;

        public float current_fix_amount;
        public float max_fix_amount = Config.current.repairing_counts;
        public int fix_cd = Config.current.repairing_cd_ticks;
        public int max_fix_cd = Config.current.repairing_cd_ticks;

        public bool fix_caravan = false;
        public Device fix_device;

        public void attach()
        {
            Ticker.instance.do_when_tick_start += tick;
            fix_module = new CaravanFixModule();
        }

        public void detach()
        {
            Ticker.instance.do_when_tick_start -= tick;
        }

        private void tick()
        {
            fix_module.tick();

            if (!player_oper && current_fix_amount != max_fix_amount)
            {
                fix_cd++;
                if(fix_cd >= max_fix_cd)
                {
                    current_fix_amount = max_fix_amount;
                    fix_cd = 0;
                }
            }
        }

        public void Fix(IUiFix f)
        {
            if (current_fix_amount == 0)
                return;
            
            if (f.Fix())
            {
                current_fix_amount--;
                Audio.AudioSystem.instance.PlayOneShot(Config.current.SE_fix);
            }
            else
            {
                Audio.AudioSystem.instance.PlayOneShot(Config.current.SE_fix_fail);
            }
        }

        public void TryToFix(Device d)
        {
            fix_caravan = false;
            fix_device = d;
        }

        public void TryToFixCaravan()
        {
            fix_device = null;
            fix_caravan = true;
        }


        /// <summary>
        /// 尝试把玩家操作设置为ret
        /// </summary>
        /// <param name="ret"></param>
        public void PlayerOper(bool ret)
        {
            if (ret)
            {
                if (current_fix_amount != 0)
                    player_oper = ret;
            }
            else
            {
                player_oper = ret;
            }
        }
    }
}
