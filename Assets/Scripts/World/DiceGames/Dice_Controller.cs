using System.Collections.Generic;
using UnityEngine;
using World.Encounters.Dialogs;
using TMPro;
using System.Linq;
using Foundations;

namespace World.DiceGames
{
    public class Dice_Controller : MonoBehaviour
    {
        public Encounter_Dialog_Window owner;

        public Sprite[] paintings;
        public Dice_Mono[] dices;

        public TextMeshProUGUI player_score;
        public TextMeshProUGUI player_trun_score;

        public TextMeshProUGUI ai_score;
        public TextMeshProUGUI ai_trun_score;

        //==================================================================================================

        public void load_painting(int n, out Sprite sprite)
        {
            sprite = paintings[--n];
        }


        public void turn()
        {
            List<Vector2> old_pos_list = new();

            foreach (var dice in dices)
            {
                dice.init(this);

                var painting_num = Random.Range(1, 7);
                load_painting(painting_num, out var sprite_painting);
                dice.painting.sprite = sprite_painting;
                dice.painting_num = painting_num;

                dice.transform.localPosition = calc_pos();

                var angle = Random.Range(0f, 360f);
                dice.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }


            #region 子函数 cals_pos
            Vector2 calc_pos()
            {
                Vector2 pos = new(Random.Range(-400f, 400f), Random.Range(-400f, 400f));

                var is_success = true;
                foreach (var old_pos in old_pos_list)
                {
                    if (Vector2.Distance(pos, old_pos) <= 150f)
                    {
                        is_success = false;
                        break;
                    }
                }

                if (!is_success)
                {
                    pos = calc_pos();
                }
                else
                {
                    old_pos_list.Add(pos);
                }
                
                return pos;
            }
            #endregion
        }


        private void Start()
        {
            turn();

            //玩家
            if (!Share_DS.instance.try_get_value("player_score", out int _player_score))
            {
                Share_DS.instance.add("player_score", 0);
            }

            if (!Share_DS.instance.try_get_value("player_trun_score", out int _player_trun_score))
            {
                Share_DS.instance.add("player_trun_score", 0);
            }
            else
            {
                player_score.text = $"{_player_score + _player_trun_score}";
                Share_DS.instance.add("player_score", _player_score + _player_trun_score);
                Share_DS.instance.add("player_trun_score", 0);
            }

            //ai
            if (!Share_DS.instance.try_get_value("ai_score", out int _ai_score))
            {
                Share_DS.instance.add("ai_score", 0);
            }

            if (!Share_DS.instance.try_get_value("ai_trun_score", out int _ai_trun_score))
            {
                Share_DS.instance.add("ai_trun_score", 0);
            }
            else
            {
                ai_score.text = $"{_ai_score + _ai_trun_score}";
                Share_DS.instance.add("ai_score", _ai_score + _ai_trun_score);
                Share_DS.instance.add("ai_trun_score", 0);
            }

            //ai行为
            if (Share_DS.instance.try_get_value("dice_game_turn", out string dice_game_turn))
            {
                if (dice_game_turn == "ai")
                {
                    ai_turn();
                }

                Share_DS.instance.add("dice_game_turn", "player");
            }
        }


        public void check()
        {
            var _dices = dices.Where(t => t.is_selected).Select(t => t.painting_num).ToArray();
            var trun_score = Dice_Utility.calc_score(_dices, out var ramin_dices);

            if (ramin_dices.Any())
                trun_score = 0;

            player_trun_score.text = $"{trun_score}";

            Share_DS.instance.add("player_trun_score", trun_score);
        }


        public void ai_turn()
        {
            var _dices = dices.Select(t => t.painting_num).ToArray();
            var trun_score = Dice_Utility.calc_score(_dices, out var ramin_dices);

            var select_dices = Dice_Utility.delete_cell(_dices, ramin_dices).ToList();
            foreach (var dice in dices)
            {
                if (select_dices.Contains(dice.painting_num))
                {
                    dice.is_selected = true;
                    select_dices.Remove(dice.painting_num);
                    continue;
                }
            }

            ai_trun_score.text = $"{trun_score}";

            Share_DS.instance.add("ai_trun_score", trun_score);
        }

    }
}

