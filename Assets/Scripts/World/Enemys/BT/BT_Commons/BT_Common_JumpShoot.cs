using Commons;
using Foundations.Tickers;
using UnityEngine;
using World.Helpers;
using static World.Enemys.BT.BT_Common_JumpShoot;

namespace World.Enemys.BT
{
    public class BT_Common_JumpShoot : Enemy_BT, IEnemy_BT_Jump, IEnemy_BT_Attack_Delay<EN_FSM>, IEnemy_BT_Shoot
    {
        #region FSM
        public enum EN_FSM
        {
            Default,
            Idle,
            Jump,
            Attack,
            AttackAfter
        }
        EN_FSM m_state;

        public override string state => $"{m_state}";
        #endregion

        #region PRM
        float e_jump_cd;
        int m_jump_cd;

        float e_atk_dis;
        #endregion

        #region JUMP
        float IEnemy_BT_Jump.jump_velocity_x_max => e_jump_velocity_x_max;
        float e_jump_velocity_x_max;
        Vector2 IEnemy_BT_Jump.jump_height_area => e_jump_height_area;
        Vector2 e_jump_height_area;

        IEnemy_BT_Jump i_jump => this;
        #endregion

        #region Attack_Delay
        float e_before_attack_sec;
        float e_after_attack_sec;

        float IEnemy_BT_Attack_Delay<EN_FSM>.before_attack_sec => e_before_attack_sec;
        float IEnemy_BT_Attack_Delay<EN_FSM>.after_attack_sec => e_after_attack_sec;

        EN_FSM IEnemy_BT_Attack_Delay<EN_FSM>.state { get => m_state; set => m_state = value; }
        EN_FSM IEnemy_BT_Attack_Delay<EN_FSM>.charge_next_state => EN_FSM.Idle;

        IEnemy_BT_Attack_Delay<EN_FSM> i_attack_delay => this;
        #endregion

        #region Shoot
        int e_running_fire_count;
        int m_running_fire_count;

        string IEnemy_BT_Shoot.muzzle_bone_name => "proj_muzzle";

        int IEnemy_BT_Shoot.shoot_cd_max { get => m_shoot_cd_max; set => m_shoot_cd_max = value; }
        int m_shoot_cd_max;

        Vector2 IEnemy_BT_Shoot.projectile_speed_range { get => m_projectile_speed_range; set => m_projectile_speed_range = value; }
        Vector2 m_projectile_speed_range;

        IEnemy_BT_Shoot i_shoot => this;
        #endregion

        //==================================================================================================

        public override void init(Enemy cell, params object[] prms)
        {
            #region 读表参数
            e_jump_cd = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "JUMP_CD"));
            e_atk_dis = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "ATTACK_DIS"));

            e_jump_velocity_x_max = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "JUMP_VELOCITY_X_MAX"));
            e_jump_height_area = Enemy_BT_Core_CPN.read_diy_prm_to_vec2(cell, "JUMP_HEIGHT_AREA");

            e_before_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "BEFORE_ATTACK_SEC"));
            e_after_attack_sec = float.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "AFTER_ATTACK_SEC"));

            e_running_fire_count = int.Parse(Enemy_BT_Core_CPN.read_diy_prm(cell, "RUNNING_FIRE_COUNT"));
            #endregion

            m_state = (EN_FSM)System.Enum.Parse(typeof(EN_FSM), (string)prms[0]);
            if (m_state == EN_FSM.Default)
                m_state = EN_FSM.Idle;

            cell.mover.move_type = EN_enemy_move_type.Slide;

            i_shoot.init_data(cell);

            base.init(cell);
        }


        public override void tick(Enemy cell)
        {
            base.tick(cell);
        }


        #region Idle
        public void start_Idle(Enemy cell)
        {
            SeekTarget_Helper.select_caravan(out cell.target);

            m_jump_cd = Mathf.RoundToInt(e_jump_cd * Config.PHYSICS_TICKS_PER_SECOND);
        }


        public void do_Idle(Enemy cell)
        {
            var pos_delta_x = (cell.pos - cell.target.Position).x;
            var is_in_attack_area = pos_delta_x <= e_atk_dis && pos_delta_x >= -e_atk_dis;

            if (is_in_attack_area)
            {
                m_state = EN_FSM.Attack;
                return;
            }

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


        #region Attack
        public void start_Attack(Enemy cell)
        {
            if (m_running_fire_count >= e_running_fire_count) return;

            i_attack_delay.@do(cell, shoot, () => { m_state = EN_FSM.AttackAfter; });

            #region 子函数 shoot
            void shoot()
            {
                SeekTarget_Helper.select_caravan(out var car);

                i_shoot.do_shoot(cell, cell.target.Position);
                m_running_fire_count++;
            }
            #endregion
        }


        public void do_Attack(Enemy cell)
        {
            if (m_running_fire_count < e_running_fire_count) return;

            m_running_fire_count = 0;
            Request_Helper.del_requests($"end_attack_{cell.GUID}");

            m_state = EN_FSM.Idle;
        }
        #endregion


        #region AttackAfter
        public void do_AttackAfter(Enemy cell)
        {
            m_state = EN_FSM.Attack;
        }
        #endregion


        public override void notify_on_set_face_dir(Enemy cell)
        {
            Enemy_BT_Face_CPN.set_face_dir_to_player_only_x(cell);
        }
    }
}

