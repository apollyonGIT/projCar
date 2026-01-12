using AutoCodes;
using System.Collections.Generic;
using UnityEngine;
using World.Audio;
using World.Enemys;
using World.Enemys.BT;
using static UnityEngine.GraphicsBuffer;

namespace World.Devices.Device_AI
{
    public class BasicRam : Device, IAttack_Device,IRotate
    {
        #region CONST

        private const string ANIM_IDLE = "idle";
        private const string ANIM_BROKEN = "idle";
        private const string COLLIDER_1 = "collider_1";
        protected const string KEY_POINT_1 = "collider_1";

        private const float ANGLE_LIMIT = 100F;
        #endregion

        protected enum Device_FSM_Ram
        {
            idle,
            broken,
        }
        protected Device_FSM_Ram fsm;

        public Vector2 default_dir = Vector2.up;

        public Vector2 ram_dir;

        protected string se_ram_hit_enemy;
        private string se_ram_rotate;

        #region Table Data
        protected float damage_speed_mod;
        protected float basic_knockback;
        protected float knockback_speed_mod;
        protected float collision_rotate_coef;
        protected float ui_rotate_coef;
        protected float knock_y;
        #endregion

        public float Damage_Increase { get; set; }
        public float Knockback_Increase { get; set; }

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;
        #endregion
        public float turn_angle => Vector2.SignedAngle(Vector2.up, ram_dir);
        public override void InitData(device_all rc)
        {

            device_type = DeviceType.melee;

            ram_dir = default_dir;

            bones_direction.Clear();
            bones_direction.Add(BONE_FOR_ROTATE, ram_dir);

            ram_logics.TryGetValue(rc.ram_logic.ToString(), out var logic);
            damage_speed_mod = logic.damage_v_coef;
            basic_knockback = logic.ft;
            knockback_speed_mod = logic.ft_v_coef;
            collision_rotate_coef = logic.collision_rotate_coef;
            ui_rotate_coef = logic.ui_rotate_coef;
            knock_y = logic.knock_y;


            se_ram_hit_enemy = logic.SE_hit_enemy;
            se_ram_rotate = logic.SE_rotate;

            base.InitData(rc);
        }

        public override void Start()
        {
            base.Start();
            FSM_change_to(Device_FSM_Ram.idle);
        }


        // ========================================================================================

        public override void tick()
        {
            if (!is_validate && fsm != Device_FSM_Ram.broken)       //坏了
                FSM_change_to(Device_FSM_Ram.broken);

            switch (fsm)
            {
                case Device_FSM_Ram.idle:
                    rotate_bone_to_dir(BONE_FOR_ROTATE, ram_dir);
                    break;
                case Device_FSM_Ram.broken:
                    if (is_validate)
                        FSM_change_to(Device_FSM_Ram.idle);
                    break;

                default:
                    break;
            }
            base.tick();
        }

        private void FSM_change_to(Device_FSM_Ram target_fsm)
        {
            fsm = target_fsm;
            switch (target_fsm)
            {
                case Device_FSM_Ram.idle:
                    ChangeAnim(ANIM_IDLE, true);
                    open_collider_this(this);
                    break;
                case Device_FSM_Ram.broken:
                    ChangeAnim(ANIM_BROKEN, true);  
                    break;
                default:
                    break;
            }
        }

        public override void Disable()
        {
            CloseCollider(COLLIDER_1);
            base.Disable();
        }

        // ----------------------------------------------------------------------------------------

        public override List<Attack_Data> ExecDmg(ITarget t, Dictionary<string, int> dmg)
        {
            battle_datas.TryGetValue(desc.id.ToString(), out var battle_data);

            List<Attack_Data> attack_datas = new();
            foreach (var d in dmg)
            {
                var v_relative_parallel = Vector2.Dot((this as ITarget).velocity - t.velocity, bones_direction[BONE_FOR_ROTATE].normalized);
                if (v_relative_parallel <= 0)
                    return attack_datas;    //不应该能走到这里

                int final_dmg = (int)((d.Value + Damage_Increase) * v_relative_parallel * damage_speed_mod);
                Attack_Data attack_data = new()
                {
                    atk = final_dmg,
                    critical_chance = battle_data.critical_chance + BattleContext.instance.global_critical_chance,
                    critical_dmg_rate = battle_data.critical_rate + BattleContext.instance.global_critical_chance,
                    armor_piercing = battle_data.armor_piercing + BattleContext.instance.global_armor_piercing,
                    diy_atk_str = d.Key,
                };

                attack_datas.Add(attack_data);
            }

            return attack_datas;
        }


        protected virtual void collider_enter_event(ITarget t)
        {
            var v_relative_parallel = Vector2.Dot((this as ITarget).velocity - t.velocity, bones_direction[BONE_FOR_ROTATE].normalized);
            if (v_relative_parallel <= 0)
                return;

            float final_ft = (basic_knockback + Knockback_Increase) * v_relative_parallel * knockback_speed_mod;

            ram_logics.TryGetValue(desc.ram_logic.ToString(), out var logic);
            var attack_datas = ExecDmg(t, logic.damage);

            foreach (var attack_data in attack_datas)
            {
                attack_data.calc_device_coef(this);

                t.hurt(this, attack_data, out var dmg_data);
                BattleContext.instance.ChangeDmg(this, dmg_data.dmg);

                t.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);
            }

            if (t.hp <= 0)
            {
                kill_enemy_action?.Invoke(t);
            }

            AudioSystem.instance.PlayOneShot(se_ram_hit_enemy);
            var sign = Mathf.Sign(BattleUtility.get_target_colllider_pos(t).x - key_points[KEY_POINT_1].position.x);
            t.impact(WorldEnum.impact_source_type.melee, Vector2.zero, new Vector2(sign, knock_y), final_ft);
            if (t is Enemy e)
                if (e.old_bt is IEnemy_Can_Jump j)
                    j.Get_Rammed(final_ft);

            var collision_angle = (Vector2.SignedAngle(bones_direction[BONE_FOR_ROTATE], Vector2.up) * v_relative_parallel / (1 + BattleContext.instance.ram_stability / 1000f));
            ram_dir = Quaternion.AngleAxis(collision_angle * collision_rotate_coef, Vector3.forward) * ram_dir;
            Rotate_Limit();
        }

        private void open_collider_this(Device d)
        {
            d.OpenCollider(COLLIDER_1, collider_enter_event);
        }

        protected void Rotate_Limit()
        {
            var r_angle = Vector2.SignedAngle(Vector2.up, ram_dir);

            if (r_angle > ANGLE_LIMIT)
                ram_dir = Quaternion.AngleAxis(ANGLE_LIMIT, Vector3.forward) * Vector2.up;
            else if (r_angle < -ANGLE_LIMIT)
                ram_dir = Quaternion.AngleAxis(-ANGLE_LIMIT, Vector3.forward) * Vector2.up;
        }


        public void Rotate_Ram_Dir_By_UI(float angle)
        {
            angle *= (1 + BattleContext.instance.ram_maneuverability);

            ram_dir = Quaternion.AngleAxis(angle * ui_rotate_coef, Vector3.forward) * ram_dir;

            Rotate_Limit();
        }

        /// <summary>
        /// 根据转轮是否转动，播放或停止音效
        /// </summary>
        public void Play_Or_End_SE_By_UI(bool play)
        {
            if (play)
                AudioSystem.instance.PlayClip(se_ram_rotate, true);
            else
                AudioSystem.instance.StopClip(se_ram_rotate);
        }

        public void Rotate(float angle)
        {
            Rotate_Ram_Dir_By_UI(angle);
        }

        public void TryToAutoRotate()
        {
            
        }
    }
}
