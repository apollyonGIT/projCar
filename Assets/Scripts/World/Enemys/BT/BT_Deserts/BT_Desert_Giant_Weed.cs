using Commons;
using Foundations;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Desert_Giant_Weed : Enemy_BT, IEnemy_BT_Jump
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Jump,
            Stick
        }
        EN_FSM m_state;
        public override string state => $"{m_state}";
        #endregion

        #region PRM
        float e_slow_rate;
        //float m_slow_rate;

        Vector2 e_jump_deg_area;
        Vector2 e_jump_speed_area;

        Vector2 e_rotate_speed_area;

        float e_stick_rate;

        float m_self_rotate_deg;
        Vector2 m_pos_offset;
        bool m_can_stick = true;
        #endregion

        #region JUMP
        float IEnemy_BT_Jump.jump_velocity_x_max => default;
        Vector2 IEnemy_BT_Jump.jump_height_area => default;

        IEnemy_BT_Jump i_jump => this;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_slow_rate = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "SLOW_RATE"));
            e_jump_deg_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "JUMP_DEG_AREA");
            e_jump_speed_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "JUMP_SPEED_AREA");
            e_rotate_speed_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "ROTATE_SPEED_AREA");

            e_stick_rate = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "STICK_RATE"));
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Jump;

            cell.is_ban_scaleY_slip = true;
            cell.mover.move_type = EN_enemy_move_type.Slide;

            SeekTarget_Helper.select_caravan(out cell.target);

            base.init(cell);
        }

        
        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        public override void notify_on_set_face_dir(Enemy cell)
        {
            
        }


        public override void notify_on_dead(Enemy cell)
        {
            if (m_state == EN_FSM.Stick)
            {
                ref var slow_rate = ref WorldContext.instance.slow_rate;
                slow_rate -= e_slow_rate;
            }

            cell.fini();
        }


        #region Jump
        public void do_Jump(Enemy cell)
        {
            m_self_rotate_deg += Random.Range(e_rotate_speed_area.x, e_rotate_speed_area.y) * Config.PHYSICS_TICK_DELTA_TIME;
            var rad = m_self_rotate_deg * Mathf.Deg2Rad;
            cell.dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            ref var move_type = ref cell.mover.move_type;
            if (move_type == EN_enemy_move_type.Slide)
            {
                move_type = EN_enemy_move_type.Hover;
                rad = Random.Range(e_jump_deg_area.x, e_jump_deg_area.y) * Mathf.Deg2Rad;
                cell.velocity = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized * Random.Range(e_jump_speed_area.x, e_jump_speed_area.y);
                return;
            }

            i_jump.is_land(cell);

            if (m_can_stick && Vector2.Distance(cell.pos, cell.target.Position) < 1f)
            {
                if (Random.Range(0, 100) > e_stick_rate)
                {
                    m_can_stick = false;
                    return;
                }

                m_state = EN_FSM.Stick;
            }
        }
        #endregion


        #region Stick
        public void start_Stick(Enemy cell)
        {
            m_pos_offset = cell.pos - cell.target.Position;
            cell.mover.move_type = EN_enemy_move_type.None;

            ref var slow_rate = ref WorldContext.instance.slow_rate;
            slow_rate += e_slow_rate;
        }


        public void do_Stick(Enemy cell)
        {
            cell.pos = cell.target.Position + m_pos_offset;
        }
        #endregion
    }
}

