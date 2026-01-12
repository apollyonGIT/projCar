using UnityEngine;

namespace World.Enemys.BT
{
    public class Enemy_BT_Core_CPN
    {
        public static void valid_fsm(Enemy_BT bt, Enemy cell)
        {
            if (bt.last_state != bt.state)
            {
                bt.GetType().GetMethod($"end_{bt.last_state}")?.Invoke(bt, new object[] { cell });
                bt.GetType().GetMethod($"start_{bt.state}")?.Invoke(bt, new object[] { cell });

                bt.last_state = bt.state;
            }

            bt.GetType().GetMethod($"do_{bt.state}")?.Invoke(bt, new object[] { cell });
        }


        public static string read_diy_prm(Enemy cell, string designer_prm)
        {
            if (!cell._desc.diy_prms.TryGetValue(designer_prm, out var coder_prm))
            {
                Debug.LogError($"怪物diy参数读取失败，ID:{cell._desc.id}, 输入参数名:{designer_prm}");
                return "";
            }

            return coder_prm;
        }


        public static Vector2 read_diy_prm_to_vec2(Enemy cell, string designer_prm)
        {
            var strs = read_diy_prm(cell, designer_prm).Split(',');
            return new(float.Parse(strs[0]), float.Parse(strs[1]));
        }


        public static void valid_speed_expt_change(Enemy cell)
        {
            cell.is_speed_expt_change = cell.old_speed_expt != cell.speed_expt;
            cell.old_speed_expt = cell.speed_expt;
        }
    }
}

