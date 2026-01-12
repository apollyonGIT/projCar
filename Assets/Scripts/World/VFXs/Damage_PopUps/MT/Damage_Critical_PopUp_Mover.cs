using Commons;
using Foundations;
using UnityEngine;

namespace World.VFXs.Damage_PopUps
{
    public class Damage_Critical_PopUp_Mover : Singleton<Damage_Critical_PopUp_Mover>, IDamage_PopUp_Mover
    {
        const float NUM_RELATIVE_SCALE_COEF = 1.5F;

        const int LIFE_TICK_BASE = 45;
        const float LIFE_TICK_COEF = 2F;

        const int NUM_SCALE_DMG_MIN = 5;
        const int NUM_SCALE_DMG_MAX = 100;

        //==================================================================================================

        void IDamage_PopUp_Mover.init(Damage_PopUp_Mono owner)
        {
            owner.upd_field("velocity", Random.Range(9f, 11f));
            owner.upd_field("decay_coef", Random.Range(0.945f, 0.955f));

            var dir_angle_in_rad = Random.Range(0.25f, 0.75f) * Mathf.PI;
            owner.upd_field("dir_angle_cos", Mathf.Cos(dir_angle_in_rad));
            owner.upd_field("dir_angle_sin", Mathf.Sin(dir_angle_in_rad));

            var dmg_parm = Mathf.Clamp(owner.dmg_data.dmg - NUM_SCALE_DMG_MIN, 1, NUM_SCALE_DMG_MAX);
            var t = Mathf.Log10(dmg_parm);
            owner.transform.localScale *= Mathf.LerpUnclamped(1, NUM_RELATIVE_SCALE_COEF, t);
            owner.exist_delta = (int)(LIFE_TICK_BASE * Mathf.LerpUnclamped(1, LIFE_TICK_COEF, t));
        }


        void IDamage_PopUp_Mover.move(Damage_PopUp_Mono owner)
        {
            owner.try_query_field("velocity", out float velocity);
            owner.try_query_field("dir_angle_cos", out float dir_angle_cos);
            owner.try_query_field("dir_angle_sin", out float dir_angle_sin);
            owner.try_query_field("decay_coef", out float decay_coef);

            var dir = new Vector3(dir_angle_cos, dir_angle_sin, 0f);
            owner.transform.localPosition += dir * velocity * Config.PHYSICS_TICK_DELTA_TIME;

            owner.upd_field("velocity", velocity * decay_coef);
        }
    }
}

