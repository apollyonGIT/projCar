using Commons;
using System;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class Delay_Drop_Loot : MonoBehaviour, IEncounter_Dialog_CPN
    {
        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(IEncounter_Dialog_Window_UI owner, string[] args)
        {
            var delay_second_min = int.Parse(args[1]);
            var delay_second_max = int.Parse(args[2]);
            var delay_second = UnityEngine.Random.Range(delay_second_min, delay_second_max + 1);

            //var interrupt_dis = float.Parse(args[3]);
            var interrupt_dis = Config.current.trigger_length;

            var cache_dic = Encounter_Dialog.instance.cache_dic;
            if (!cache_dic.ContainsKey(m_key_name))
            {
                cache_dic.Add($"@Delay_delay_second", delay_second);
                cache_dic.Add($"@Delay_interrupt_dis", interrupt_dis);

                cache_dic.Add(m_key_name, "data_end");
            }

            if (!cache_dic.ContainsKey("is_delay"))
                cache_dic.Add("is_delay", true);
        }


        public static void delay(string loot_name, Action ac_end)
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
            _event.lanuch_req = new("add_loot_req",
                (req) => { return req.countdown <= 0; },
                (req) =>
                {
                    req.countdown = (int)delay_second * Config.PHYSICS_TICKS_PER_SECOND;
                    _event.launch_cd = req.countdown;

                    _event.launch_loot_name = loot_name;
                },
                (_) =>
                {
                    _event.launch_cd = 0;
                    _event.launch_loot_name = "";

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

