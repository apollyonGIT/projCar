using Foundations.Tickers;
using System.Linq;
using UnityEngine;

namespace World.Enemys
{
    public class Enemy_Buff_Helper
    {
        public static void buff_fire(Enemy cell, int lasting_tick)
        {
            var target = cell as ITarget;
            var end_buff_name = $"end_buff_fire{cell.GUID}";

            Request_Helper.del_requests(end_buff_name);

            var count = cell.buff_list.Where(t => t == "Fire").Count();
            for (int i = 0; i < count; i++)
            {
                cell.buff_list.Remove("Fire");
            }

            cell.buff_list.AddLast("Fire");

            int fire_cd = 120;
            Request req = new(end_buff_name,
                        (req) => { return req.countdown == 0; },
                        (req) => { req.countdown = lasting_tick; },
                        (req) =>
                        {
                            cell.buff_list.Remove("Fire");
                        },
                        (req) => 
                        {
                            if (--fire_cd == 0)
                            {
                                fire_cd = 120;
                                (cell as ITarget).hurt(null, new Attack_Data()
                                {
                                    atk = 2,
                                },
                                out _);
                            }
                            req.countdown--; 
                        }
            );           
        }


        public static void buff_acid(Enemy cell, int lasting_tick)
        {
            var target = cell as ITarget;
            var end_buff_name = $"end_buff_acid{cell.GUID}";

            Request_Helper.del_requests(end_buff_name);

            var count = cell.buff_list.Where(t => t == "Acid").Count();
            for (int i = 0; i < count; i++)
            {
                cell.buff_list.Remove("Acid");
            }

            cell.buff_list.AddLast("Acid");


            Request_Helper.delay_do(end_buff_name, lasting_tick,
                    (_) => {
                        cell.buff_list.Remove("Acid");
                    }
            );
        }


        public static void buff_blind(Enemy cell, int lasting_tick)
        {
            var target = cell as ITarget;
            var end_buff_name = $"end_buff_blind{cell.GUID}";

            var hit_rate_record = cell.hit_rate;
            cell.hit_rate = 0;

            Request_Helper.del_requests(end_buff_name);

            var count = cell.buff_list.Where(t => t == "Blind").Count();
            for (int i = 0; i < count; i++)
            {
                cell.buff_list.Remove("Blind");
            }

            cell.buff_list.AddLast("Blind");

            Request_Helper.delay_do(end_buff_name, lasting_tick,
                    (_) => {
                        cell.hit_rate = hit_rate_record;
                        cell.buff_list.Remove("Blind");
                    }
            );
        }


        public static void buff_stun(Enemy cell, int lasting_tick)
        {
            var target = cell as ITarget;
            var end_buff_name = $"end_buff_stun{cell.GUID}";

            cell.is_use_bt = false;

            Request_Helper.del_requests(end_buff_name);

            var count = cell.buff_list.Where(t => t == "Stun").Count();
            for (int i = 0; i < count; i++)
            {
                cell.buff_list.Remove("Stun");
            }

            cell.buff_list.AddLast("Stun");

            Request_Helper.delay_do(end_buff_name, lasting_tick,
                    (_) => {
                        cell.buff_list.Remove("Stun");
                        cell.is_use_bt = true;
                    }
            );
        }


        public static void buff_bind(Enemy cell)
        {
            var target = cell as ITarget;
            var buff_name = $"buff_bind{cell.GUID}";

            //规则：不会被覆盖
            if (Request_Helper.query_request(buff_name).Any()) return;

            cell.buff_list.AddLast("Bind");

            Request req = new(buff_name, 
                (req) =>
                {
                    req.prms_dic.TryGetValue("is_hurt", out var _is_hurt);
                    return (bool)_is_hurt;
                },
                (req) => {
                    cell.is_use_bt = false;
                    req.prms_dic.Add("is_hurt", false);
                },
                (_) => {
                    cell.is_use_bt = true;

                    cell.buff_list.Remove("Bind");
                }, 
                null);
        }


        public static void buff_flied(Enemy cell)
        {
            var target = cell as ITarget;
            var buff_name = $"buff_flied{cell.GUID}";

            //规则：不会被覆盖
            if (Request_Helper.query_request(buff_name).Any()) return;

            Request req = new(buff_name, 
                (req) =>
                {
                    req.prms_dic.TryGetValue("is_land", out var _is_land);
                    return (bool)_is_land;
                },
                (req) => {
                    req.prms_dic.Add("is_land", false);
                },
                (req) => {
                    buff_stun(cell, Mathf.RoundToInt(Commons.Config.current.monster_land_stun_sec * Commons.Config.PHYSICS_TICKS_PER_SECOND));
                },
                null);
        }
    }
}

