using AutoCodes;
using World.Audio;
using World.Enemys.BT;
using World.Enemys;
using UnityEngine;
using Commons;

namespace World.Devices.Device_AI
{
    //旧时代的设备 死在了伤害更新的时候 不再维护


    public class EntropyIncreaseRam : BasicRam
    {
        public float entropy = 1f;

        public override void InitData(device_all rc)
        {
            base.InitData(rc);
            entropy = 1f;
        }

        public override void Start()
        {
            entropy = 1f;
            base.Start();
        }


        public override void tick()
        {
            if (is_validate && fsm != BasicRam.Device_FSM_Ram.broken)
                entropy += 0.2f * Config.PHYSICS_TICK_DELTA_TIME;

            entropy = Mathf.Clamp(entropy, 0.5f, 3f);

            base.tick();
        }

        protected override void collider_enter_event(ITarget t)
        {
            battle_datas.TryGetValue(desc.id.ToString(), out var battle_data);

            var v_relative_parallel = Vector2.Dot((this as ITarget).velocity - t.velocity, bones_direction[BONE_FOR_ROTATE].normalized);
            if (v_relative_parallel <= 0)
                return;

            var basic_damage = 0;
            int final_dmg = (int)((basic_damage + Damage_Increase) * v_relative_parallel * damage_speed_mod * entropy);
            float final_ft = (basic_knockback + Knockback_Increase) * v_relative_parallel * knockback_speed_mod * entropy;

            entropy *= 0.8f;

            Attack_Data attack_data = new()
            {
                atk = final_dmg,
                critical_chance = battle_data.critical_chance + BattleContext.instance.global_critical_chance,
                critical_dmg_rate = battle_data.critical_rate + BattleContext.instance.global_critical_chance,
                armor_piercing = battle_data.armor_piercing + BattleContext.instance.global_armor_piercing,
            };

            attack_data.calc_device_coef(this);

            t.hurt(this, attack_data, out var dmg_data);
            BattleContext.instance.ChangeDmg(this, dmg_data.dmg);

            if (t.hp <= 0)
            {
                kill_enemy_action?.Invoke(t);
            }

            AudioSystem.instance.PlayOneShot(se_ram_hit_enemy);
            var sign = Mathf.Sign(BattleUtility.get_target_colllider_pos(t).x - key_points[KEY_POINT_1].position.x);
            t.impact(WorldEnum.impact_source_type.melee, Vector2.zero, new Vector2(sign, 0.5f), final_ft);
            if (t is Enemy e)
                if (e.old_bt is IEnemy_Can_Jump j)
                    j.Get_Rammed(final_ft);

            var collision_angle = (Vector2.SignedAngle(bones_direction[BONE_FOR_ROTATE], Vector2.up) * v_relative_parallel / (1 + BattleContext.instance.ram_stability / 1000f));
            ram_dir = Quaternion.AngleAxis(collision_angle * collision_rotate_coef, Vector3.forward) * ram_dir;
            Rotate_Limit();
        }
    }
}
