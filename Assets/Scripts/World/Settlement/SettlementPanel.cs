using Commons;
using Foundations;
using TMPro;
using UnityEngine;

namespace World.Settlement
{
    public class SettlementPanel : MonoBehaviour
    {
        public TextMeshProUGUI time_text;
        public TextMeshProUGUI road_text;
        public TextMeshProUGUI enemy_text;
        public TextMeshProUGUI damage_text;
        public TextMeshProUGUI elite_text;
        public TextMeshProUGUI coin_text;

        public TextMeshProUGUI result_text;
        public TextMeshProUGUI score_text;

        public LootPanel loot_panel;

        private string ToTimeFormat(float time)
        {
            //秒数取整
            int seconds = (int)time;
            //一小时为3600秒 秒数对3600取整即为小时
            int hour = seconds / 3600;
            //一分钟为60秒 秒数对3600取余再对60取整即为分钟
            int minute = seconds % 3600 / 60;
            //对3600取余再对60取余即为秒数
            seconds = seconds % 3600 % 60;
            //返回00:00:00时间格式
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hour, minute, seconds);
        }

        public void Init(bool successs)
        {
            var DS = Share_DS.instance;
            var config = Config.current;

            DS.try_get_value(Game_Mgr.key_scene_index, out int roads_num);
            DS.try_get_value(Game_Mgr.key_game_time, out int game_time);
            DS.try_get_value(Game_Mgr.key_total_distance, out float distance);

            DS.try_get_value(Game_Mgr.key_kill_enemy_count, out int enemy_num);
            DS.try_get_value(Game_Mgr.key_make_damage_count, out int damage_num);
            DS.try_get_value(Game_Mgr.key_total_enemy_hp, out int total_enemy_hp);
            DS.try_get_value(Game_Mgr.key_kill_elite_enemy_count, out int elite_num);

            var speed = distance / (game_time / Config.PHYSICS_TICKS_PER_SECOND);        //每秒多少单位距离

            var speed_factor = speed < config.score_max_speed ? config.score_k_speed - Mathf.Pow((1 - speed / config.score_max_speed), 2) * (config.score_k_speed - 1)
                : config.score_k_speed;

            var damage_factor = config.score_k_damage - Mathf.Pow(1 - damage_num / (total_enemy_hp+1), 2) * (config.score_k_damage - 1);

            var caravan_hp_factor = config.score_k_broken - Mathf.Pow(1 - WorldContext.instance.caravan_hp / WorldContext.instance.caravan_hp_max, 2) * (config.score_k_broken - 1);

            var score = config.score_per_road * roads_num + config.score_elite * elite_num * speed_factor * caravan_hp_factor;

            time_text.text = ToTimeFormat(game_time / Config.PHYSICS_TICKS_PER_SECOND);
            road_text.text = roads_num.ToString();
            enemy_text.text = enemy_num.ToString();
            damage_text.text = damage_num.ToString();
            elite_text.text = elite_num.ToString();
            coin_text.text = Helpers.Safe_Area_Helper.GetLootCount(10000).ToString();
            score_text.text = ((int)score).ToString();


            if (successs)
            {
                result_text.text = "撤离成功";
                result_text.color = Color.green;
            }
            else
            {
                result_text.text = "车辆破损";
                result_text.color = Color.red;
            }


            loot_panel.Init(successs);
        }
    }
}
