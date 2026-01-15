using Commons;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Common_JumpMelee : Enemy_BT, IEnemy_BT_Jump
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Idle,
            Jump,
            Attack
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        float e_jump_cd;
        int m_jump_cd;

        Vector2 e_atk_area;
        #endregion

        #region JUMP
        float IEnemy_BT_Jump.jump_velocity_x_max => e_jump_velocity_x_max;
        float e_jump_velocity_x_max;
        Vector2 IEnemy_BT_Jump.jump_height_area => e_jump_height_area;
        Vector2 e_jump_height_area;

        IEnemy_BT_Jump i_jump => this;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_jump_cd = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "JUMP_CD"));
            e_atk_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "ATK_AREA");

            e_jump_velocity_x_max = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "JUMP_VELOCITY_X_MAX"));
            e_jump_height_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "JUMP_HEIGHT_AREA");
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Idle;

            cell.mover.move_type = EN_enemy_move_type.Slide;

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        #region Idle
        public void start_Idle(Enemy cell)
        {
            m_jump_cd = Mathf.RoundToInt(e_jump_cd * Config.PHYSICS_TICKS_PER_SECOND);
        }


        public void do_Idle(Enemy cell)
        {
            if (m_jump_cd-- <= 0)
            {
                m_state = EN_FSM.Jump;
            }
        }
        #endregion


        #region Jump
        public void start_Jump(Enemy cell)
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

            cell.position_expt = cell.target.Position;

            Enemy_BT_Face_CPN.set_face_dir_to_player_only_x(cell);

            i_jump.@do(cell);
        }


        public void do_Jump(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

            if (i_jump.is_land(cell))
            {
                m_state = EN_FSM.Idle;
                return;
            }

            var pos_delta = cell.pos - cell.target.Position;
            var is_in_suqare_area = Mathf.Abs(pos_delta.x) <= e_atk_area.x && Mathf.Abs(pos_delta.y) <= e_atk_area.y;

            if (is_in_suqare_area)
            {
                m_state = EN_FSM.Attack;
                return;
            }
        }
        #endregion


        #region Attack
        public void start_Attack(Enemy cell)
        {
            Enemy_Hurt_Helper.do_hurt(cell);
        }


        public void do_Attack(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

            if (i_jump.is_land(cell))
            {
                m_state = EN_FSM.Idle;
            }
        }
        #endregion


        public override void notify_on_set_face_dir(Enemy cell)
        {
        }
    }
}

