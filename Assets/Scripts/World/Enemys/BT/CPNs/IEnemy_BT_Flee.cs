using System;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public interface IEnemy_BT_Flee<T> where T : Enum
    {
        public T state { get; set; }

        //==================================================================================================

        void @do(Enemy cell, string end_state)
        {
            state = (T)System.Enum.Parse(typeof(T), end_state);

            Debug.Log($"怪物逃跑，编号：{cell._desc.id}，逃离状态：{end_state}");
        }


        void flee_on_start(Enemy cell)
        {
            SeekTarget_Helper.select_caravan(out cell.target);
        }


        void flee_on_tick(Enemy cell)
        {
            cell.position_expt = cell.target.Position + new Vector2(0, 1) * 100f;

            if (cell.pos.y >= 20f)
            {
                cell.is_ban_drop = true;
                cell.hp = 0;
            }
        }
    }
}

