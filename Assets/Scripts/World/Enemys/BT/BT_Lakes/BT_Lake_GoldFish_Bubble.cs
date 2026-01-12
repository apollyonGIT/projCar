using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Lake_GoldFish_Bubble : Enemy_BT
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Move,
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        float e_behit_radius;
        float e_explosion_radius;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_behit_radius = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEHIT_RADIUS"));
            e_explosion_radius = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "EXPLOSION_RADIUS"));
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Move;

            cell.mover.move_type = EN_enemy_move_type.Fly1;

            cell.is_ban_select = true;

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        public override void notify_on_dead(Enemy cell)
        {
            Enemy_BT_VFX_CPN.set_explosion_death_vfx(cell);

            cell.fini();
        }


        #region Move
        public void start_Move(Enemy cell)
        {
        }


        public void do_Move(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

            try_explosion(cell.target);

            #region 子函数 explosion
            bool try_explosion(ITarget target)
            {
                //规则：目标处于爆炸范围内，对范围内的所有玩家part进行伤害
                var dis = Vector2.Distance(target.Position, cell.pos);
                if (dis <= e_behit_radius)
                {
                    Enemy_Hurt_Helper.do_aoe_hurt_by_dis(cell, e_explosion_radius);

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

