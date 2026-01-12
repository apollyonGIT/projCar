using Commons;
using System;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class BLL_Utility
    {
        public static void set_delay(int delay_second_min, int delay_second_max, float interrupt_dis)
        {
            var delay_second = UnityEngine.Random.Range(delay_second_min, delay_second_max + 1);

            var cache_dic = Encounter_Dialog.instance.cache_dic;
            cache_dic.Add($"@Delay_delay_second", delay_second);
            cache_dic.Add($"@Delay_interrupt_dis", interrupt_dis);

            if (!cache_dic.ContainsKey("is_delay"))
                cache_dic.Add("is_delay", true);
        }


        public static void delay(Action ac_end)
        {
            var cache_dic = Encounter_Dialog.instance.cache_dic;
            if (!cache_dic.ContainsKey("is_delay"))
            {
                ac_end?.Invoke();
                return;
            }

            cache_dic.TryGetValue($"@Delay_delay_second", out var delay_second);
            cache_dic.TryGetValue($"@Delay_interrupt_dis", out var interrupt_dis);

            var _event = Encounter_Dialog.instance._event;
            _event.lanuch_req = new("add_delay_req",
                (req) => { return req.countdown <= 0; },
                (req) =>
                {
                    req.countdown = (int)delay_second * Config.PHYSICS_TICKS_PER_SECOND;
                    _event.launch_cd = req.countdown;
                },
                (_) =>
                {
                    _event.launch_cd = 0;

                    if (Mathf.Abs(WorldContext.instance.caravan_pos.x - _event.pos.x) <= (float)interrupt_dis)
                        ac_end?.Invoke();

                    var cache_dic = Encounter_Dialog.instance.cache_dic;
                    cache_dic.Remove("is_delay");
                },
                (req) =>
                {
                    _event.launch_cd = req.countdown;
                    req.countdown--;
                });
        }
    }
}

