using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World.DiceGames
{
    public class Dice_Utility
    {
        public static int calc_score(int[] dice_pn_array, out int[] remain_dices)
        {
            var score = 0;
            List<int> del_dices = new();

            //检索单个1或5
            var single_1_score = dice_pn_array.Where(t => t == 1).Count() * 100;
            var single_5_score = dice_pn_array.Where(t => t == 5).Count() * 50;
            score = single_1_score + single_5_score;

            foreach (var e in dice_pn_array)
            {
                if (e == 1 || e == 5)
                    del_dices.Add(e);
            }

            //检查多重同花
            var multi_score = 0;
            List<int> selected_pn_nums = new();
            for (int i = 1; i < 7; i++)
            {
                var pn_num = dice_pn_array.Where(t => t == i).Count();
                if (pn_num < 3) continue;

                var t_score = i;
                if (i == 1) t_score = 10;

                if (pn_num == 3)
                {
                    multi_score += t_score * 100;
                }
                else if (pn_num == 4)
                {
                    multi_score += t_score * 100 * 2;
                }
                else if (pn_num == 5)
                {
                    multi_score += t_score * 100 * 4;
                }
                else if (pn_num == 6)
                {
                    multi_score += t_score * 100 * 8;
                }

                selected_pn_nums.Add(i);
            }

            if (multi_score >= score)
            {
                score = multi_score;

                del_dices = new();
                foreach (var e in dice_pn_array)
                {
                    if (selected_pn_nums.Contains(e))
                        del_dices.Add(e);
                }
            }

            //检查顺子
            if (new[] { 1, 2, 3, 4, 5, 6 }.All(t => dice_pn_array.Contains(t)))
            {
                if (1500 >= score)
                {
                    score = 1500;

                    del_dices = new() { 1, 2, 3, 4, 5, 6 };
                }
            }

            if (new[] { 1, 2, 3, 4, 5 }.All(t => dice_pn_array.Contains(t)))
            {
                if (500 >= score)
                {
                    score = 500;

                    del_dices = new() { 1, 2, 3, 4, 5 };
                }
            }

            if (new[] { 2, 3, 4, 5, 6 }.All(t => dice_pn_array.Contains(t)))
            {
                if (750 >= score)
                {
                    score = 750;

                    del_dices = new() { 2, 3, 4, 5, 6 };
                }
            }

            var del_array = del_dices.ToArray();
            remain_dices = delete_cell(dice_pn_array, del_array);

            if (score == 0 || !remain_dices.Any())
                return score;

            score += calc_score(remain_dices, out remain_dices);
            return score;
        }


        public static void test()
        {
            int[] a = new[] {1,2,5,5,5,5};

            var r = calc_score(a, out var b);
            Debug.Log(r);

            if (b == null || !b.Any())
            {
                Debug.Log("无剩余骰子");
            }
            else 
            {
                foreach (var e in b)
                {
                    Debug.Log(e);
                }
            }
        }


        public static int[] delete_cell(int[] ori, int[] del)
        {
            var delCounts = del
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());

            List<int> result = new List<int>();
            foreach (int num in ori)
            {
                if (delCounts.TryGetValue(num, out int count) && count > 0)
                {
                    delCounts[num]--;
                }
                else
                {
                    result.Add(num);
                }
            }

            return result.ToArray();
        }
    }
}

