using AutoCodes;
using System.Collections.Generic;
using UnityEngine;
using World.Audio;
using World.Enemys.BT;
using World.Enemys;
using World.Helpers;

namespace World.Devices.Device_AI
{
    public class DScript_Ram : Device,IAttack_Device,IRotate_New
    {
        #region CONST
        private const string ANIM_IDLE = "idle";
        private const string ANIM_BROKEN = "idle";
        private const string COLLIDER_1 = "collider_1";
        protected const string KEY_POINT_1 = "collider_1";

        private const float ANGLE_LIMIT = 100F;
        private const float AUTO_WORK_ROTATE_EFFICIENCY = 0.2F;
        #endregion

        #region 实现IAttack_Device
        int IAttack_Device.ind_atk_pts => BattleContext.instance.melee_atk_pts;
        int IAttack_Device.ind_atk_add => BattleContext.instance.melee_atk_add;
        int IAttack_Device.ind_armor_piercing => BattleContext.instance.melee_armor_piercing;
        int IAttack_Device.ind_critical_chance => BattleContext.instance.melee_critical_chance;
        int IAttack_Device.ind_critical_dmg_rate => BattleContext.instance.melee_critical_dmg_rate;
        #endregion

        public Device_Attachment_Dir_Indicator_2 Ram_Dir_Indicator_Current;
        public Device_Attachment_Dir_Indicator_2 Ram_Dir_Indicator_Target;
        public Device_Attachment_Ratchet Ram_Ratchet_Rotation_Handle = new(-90f);

        protected enum Device_FSM_Ram
        {
            idle,
            broken,
        }

        protected Device_FSM_Ram fsm;

        public Vector2 default_dir = Vector2.up;

        public ram_logic ram_logic_data;

        private int play_se_delay_ticks;

        private string slot_name;

        public override void InitData(device_all rc)
        {
            ram_logics.TryGetValue(rc.ram_logic.ToString(),out ram_logic_data);

            base.InitData(rc);
        }

        public override void Start()
        {
            base.Start();
            FSM_change_to(Device_FSM_Ram.idle);
        }
        public override void InitPos()
        {
            base.InitPos();
            var dir = bones_direction[BONE_FOR_ROTATE];
            var angle_limit = get_angle_limit_from_slot(slot_name);

            Ram_Dir_Indicator_Current = new Device_Attachment_Dir_Indicator_2(dir, angle_limit.x, angle_limit.y);
            Ram_Dir_Indicator_Target = new Device_Attachment_Dir_Indicator_2(dir, angle_limit.x, angle_limit.y);

            Ram_Dir_Indicator_Current.Dir_Vector2 = dir;
            Ram_Dir_Indicator_Target.Dir_Vector2 = dir;
        }

        #region SLOT ABOUT


        private Vector2 get_angle_limit_from_slot(string slot_name)
        {
            switch (slot_name)
            {
                case "slot_front":
                    return new Vector2(-10f, 80f);
                case "slot_back":
                    return new Vector2(100f, 190f);
                case "slot_front_top":
                    return new Vector2(-10f, 80f);
                case "slot_top":
                    return new Vector2(-10f, 190f);
                case "slot_back_top":
                    return new Vector2(100f, 190f);
            }
            return new Vector2(0f, 0f);
        }
        #endregion


        #region tick() & FSM_Change_to()
        public override void tick()
        {
            if (play_se_delay_ticks >= 0)
            {
                play_se_delay_ticks--;
                if (play_se_delay_ticks < 0)
                    AudioSystem.instance.StopClip(ram_logic_data.SE_rotate);
                else
                    AudioSystem.instance.PlayClip(ram_logic_data.SE_rotate, true);
            }

            if (state != DeviceState.stupor)
            {
                switch (fsm)
                {
                    case Device_FSM_Ram.idle:
                        if (!is_validate)       //坏了
                            FSM_change_to(Device_FSM_Ram.broken);
                        break;
                    case Device_FSM_Ram.broken:
                        if (is_validate)
                            FSM_change_to(Device_FSM_Ram.idle);
                        break;

                    default:
                        break;
                }
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

        #endregion

        private void open_collider_this(Device d)
        {
            d.OpenCollider(COLLIDER_1, collider_enter_event);
        }

        public override void Disable()
        {
            CloseCollider(COLLIDER_1);
            base.Disable();
        }

        public override List<Attack_Data> ExecDmg(ITarget t, Dictionary<string, int> dmg)
        {
            battle_datas.TryGetValue(desc.id.ToString(), out var battle_data);

            List<Attack_Data> attack_datas = new();
            foreach (var d in dmg)
            {
                var v_relative_parallel = Vector2.Dot((this as ITarget).velocity - t.velocity, bones_direction[BONE_FOR_ROTATE].normalized);
                if (v_relative_parallel <= 0)
                    return attack_datas;    //不应该能走到这里

                int final_dmg = (int)((d.Value) * v_relative_parallel * ram_logic_data.damage_v_coef);
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
            if (v_relative_parallel <= ram_logic_data.v_threshold)
                return;

            var attack_datas = ExecDmg(t, ram_logic_data.damage);

            foreach (var attack_data in attack_datas)
            {
                attack_data.calc_device_coef(this);
                t.hurt(this, attack_data, out var dmg_data);
                BattleContext.instance.ChangeDmg(this, dmg_data.dmg);

                t.applied_outlier(this, attack_data.diy_atk_str, dmg_data.dmg);
            }

            float final_ft = (ram_logic_data.ft) * v_relative_parallel * ram_logic_data.ft_v_coef;

            if (t.hp <= 0)
            {
                kill_enemy_action?.Invoke(t);
            }

            AudioSystem.instance.PlayOneShot(ram_logic_data.SE_hit_enemy);
            var sign = Mathf.Sign(BattleUtility.get_target_colllider_pos(t).x - key_points[KEY_POINT_1].position.x);
            t.impact(WorldEnum.impact_source_type.melee, Vector2.zero, new Vector2(sign, ram_logic_data.knock_y), final_ft);
            if (t is Enemy e)
                if (e.old_bt is IEnemy_Can_Jump j)
                    j.Get_Rammed(final_ft);

            var collision_angle = Vector2.SignedAngle(bones_direction[BONE_FOR_ROTATE], Vector2.up) * v_relative_parallel / (1 + BattleContext.instance.ram_stability / 1000f);

            Ram_Dir_Indicator_Current.Rotate_Delta_Angle(collision_angle * ram_logic_data.collision_rotate_coef);
            bones_direction[BONE_FOR_ROTATE] = Ram_Dir_Indicator_Current.Dir_Vector2;
        }

        public void TryToAutoRotate()
        {
            var angle = Ram_Dir_Indicator_Current.Dir_Angle_With_Center_Axis_As_Up;
            var angle_target = Ram_Dir_Indicator_Target.Dir_Angle_With_Center_Axis_As_Up;
            var da = angle_target - angle;

            if (Mathf.Abs(da) < 0.1f)
                return;

            DeviceBehavior_Ram_Try_Rotate(false, AUTO_WORK_ROTATE_EFFICIENCY * Mathf.Sign(da));
        }


        #region DeviceBehaviour

        protected virtual bool DeviceBehavior_Ram_Try_Rotate(bool ui_manual, float rotation_input)
        {
            if(fsm!= Device_FSM_Ram.broken)
            {
                Ram_Dir_Indicator_Current.Rotate_Delta_Angle(rotation_input * ram_logic_data.ui_rotate_coef);
                bones_direction[BONE_FOR_ROTATE] = Ram_Dir_Indicator_Current.Dir_Vector2;

                if(ui_manual)
                    Ram_Dir_Indicator_Target.Dir_Vector2 = Ram_Dir_Indicator_Current.Dir_Vector2;
                else
                    Ram_Ratchet_Rotation_Handle.rotate(rotation_input /ram_logic_data.ui_rotate_coef, true);

                return true;
            }

            return false;
        }
        #endregion


        public void Try_Rotating(float input)
        {
            DeviceBehavior_Ram_Try_Rotate(true, input);
            play_se_delay_ticks = 2;
        }
    }
}
