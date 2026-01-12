using Commons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Treetop_Berry : Enemy_BT
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Idle
        }
        EN_FSM m_state;
        public override string state => $"{m_state}";
        #endregion

        #region PRM
        public BT_Treetop_Berry_Bush owner;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Idle;

            cell.mover.move_type = EN_enemy_move_type.None;

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            if (cell.is_first_hurt)
            {
                cell.mover.move_type = EN_enemy_move_type.Hover;
            }

            base.tick(cell);
        }


        public override void notify_on_dead(Enemy cell)
        {
            owner?.sub_cell_flags.Remove(cell.GUID);

            Enemy_BT_VFX_CPN.set_explosion_death_vfx(cell);

            cell.fini();
        }


        #region Idle
        public void do_Idle(Enemy cell)
        {
            seek_target();

            #region 子函数 seek_target
            void seek_target()
            {
                //规则：索最近玩家设备
                SeekTarget_Helper.nearest_player_device(cell, out cell.target);
                if (cell.target != null) return;

                //规则：如果全部损坏，转火车体
                SeekTarget_Helper.select_caravan(out cell.target);
            }
            #endregion

            try_explosion(cell.target);

            #region 子函数 explosion
            bool try_explosion(ITarget target)
            {
                //规则：目标处于爆炸范围内，对范围内的玩家part进行伤害
                var dis = Vector2.Distance(target.Position, cell.pos);
                if (dis <= 0.5f)
                {
                    Enemy_Hurt_Helper.do_hurt(cell);

                    notify_on_dead(cell);
                    return true;
                }

                //规则：碰到地面，直接死亡
                var ground_y = Road_Info_Helper.try_get_altitude(cell.pos.x);
                if (cell.pos.y - ground_y <= 0.1f)
                {
                    notify_on_dead(cell);
                    return true;
                }

                return false;
            }
            #endregion
        }
        #endregion
    }
}

