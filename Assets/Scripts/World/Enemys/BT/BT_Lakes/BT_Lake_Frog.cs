using Commons;
using Foundations.Tickers;
using System.Linq;
using UnityEngine;
using World.Helpers;

namespace World.Enemys.BT
{
    public class BT_Lake_Frog : Enemy_BT, IEnemy_BT_Jump
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Idle,
            Jump,
            Prepare,
            Attack,
            Stick
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region Outter
        public Vector2 tongue_start_pos;
        public Vector2 tongue_end_pos;

        public string muzzle_bone_name = "proj_muzzle";
        #endregion

        #region PRM
        float e_atk_dis;

        float e_before_attack_sec;
        float e_after_attack_sec;
        bool m_is_after_attack;

        float e_jump_cd;
        int m_jump_cd;

        float e_tongue_length;
        float m_tongue_length;

        float e_atk_sec;

        Vector2 m_tongue_to_target_offset;
        float m_self_to_target_x_dis;
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
            e_atk_dis = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATK_DIS"));

            e_before_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEFORE_ATTACK_SEC"));
            e_after_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "AFTER_ATTACK_SEC"));

            e_jump_velocity_x_max = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "JUMP_VELOCITY_X_MAX"));
            e_jump_height_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "JUMP_HEIGHT_AREA");
            e_jump_cd = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "JUMP_CD"));

            e_tongue_length = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "TONGUE_LENGTH"));
            e_atk_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATK_SEC"));
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Idle;

            cell.mover.move_type = EN_enemy_move_type.Slide;

            base.init(cell);
        }


        public override void load_outter_view(Enemy cell, EnemyView view)
        {
            Addrs.Addressable_Utility.try_load_asset("Frog_Outter", out Frog_View outter_view_model);

            var outter_view = Object.Instantiate(outter_view_model, view.transform);
            cell.add_view(outter_view);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        #region Idle
        public void start_Idle(Enemy cell)
        {
            m_jump_cd = Mathf.RoundToInt(e_jump_cd * Config.PHYSICS_TICKS_PER_SECOND);

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
        }


        public void do_Idle(Enemy cell)
        {
            if (m_jump_cd-- > 0) return;

            if (Mathf.Abs(cell.pos.x - cell.target.Position.x) <= e_atk_dis)
            {
                m_state = EN_FSM.Prepare;
                return;
            }

            m_state = EN_FSM.Jump;
        }
        #endregion


        #region Jump
        public void start_Jump(Enemy cell)
        {
            cell.position_expt = cell.target.Position;

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
        }
        #endregion


        #region Prepare
        public void start_Prepare(Enemy cell)
        {
            Request_Helper.delay_do($"end_prepare_{cell.GUID}",
                Mathf.FloorToInt(e_before_attack_sec * Config.PHYSICS_TICKS_PER_SECOND),
                (_) => { m_state = EN_FSM.Attack; });
        }
        #endregion


        #region Attack
        public void do_Attack(Enemy cell)
        {
            if (m_is_after_attack) return;

            bool is_tongue_length_max = false;

            m_tongue_length += e_tongue_length / (e_atk_sec * Config.PHYSICS_TICKS_PER_SECOND);
            cell.try_get_bone_pos(muzzle_bone_name, out tongue_start_pos);
            tongue_end_pos = tongue_start_pos + (cell.target.Position - tongue_start_pos).normalized * m_tongue_length;

            if (m_tongue_length >= e_tongue_length)
            {
                m_tongue_length = e_tongue_length;
                is_tongue_length_max = true;
            }

            //已命中
            var target_dis = Vector2.Distance(cell.target.Position, cell.pos);
            if (target_dis - m_tongue_length <= 0.1f)
            {
                Enemy_Hurt_Helper.do_hurt(cell);
                m_state = EN_FSM.Stick;
                return;
            }

            //舌头落空
            if (is_tongue_length_max)
            {
                destroy_tongue();
                m_is_after_attack = true;

                Request_Helper.delay_do($"to_idle_{cell.GUID}", Mathf.RoundToInt(e_after_attack_sec * Config.PHYSICS_TICKS_PER_SECOND),
                    (_) => {
                        m_is_after_attack = false;
                        m_state = EN_FSM.Idle;
                    });
                return;
            }
        }
        #endregion


        #region Stick
        public void start_Stick(Enemy cell)
        {
            m_tongue_to_target_offset = tongue_end_pos - cell.target.Position;
            m_self_to_target_x_dis = Mathf.Abs(cell.pos.x - cell.target.Position.x);

            Request req = new($"valid_break_stick_{cell.GUID}",
                (req) =>
                {
                    req.prms_dic.TryGetValue("is_hurt", out var _is_hurt);
                    return (bool)_is_hurt;
                },
                (req) =>
                {
                    req.prms_dic.Add("is_hurt", false);
                },
                (_) =>
                {
                    destroy_tongue();
                    m_state = EN_FSM.Idle;
                },
                null);
        }


        public void do_Stick(Enemy cell)
        {
            ref var pos = ref cell.pos;
            if (cell.target.Position.x - pos.x >= m_self_to_target_x_dis)
            {
                pos.x = cell.target.Position.x - m_self_to_target_x_dis;
                pos.y = Road_Info_Helper.try_get_altitude(pos.x);
            }

            cell.try_get_bone_pos(muzzle_bone_name, out tongue_start_pos);
            tongue_end_pos = cell.target.Position;
        }
        #endregion


        void destroy_tongue()
        {
            m_tongue_length = 0;
            tongue_start_pos = new();
            tongue_end_pos = new();
        }


        public override void notify_on_set_face_dir(Enemy cell)
        {
            Enemy_BT_Face_CPN.set_face_dir_to_player_only_x(cell);
        }
    }
}

