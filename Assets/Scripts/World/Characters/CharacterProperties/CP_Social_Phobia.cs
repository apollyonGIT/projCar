using AutoCodes;
using Commons;
using Foundations;
using System.Linq;

namespace World.Characters.CharacterProperties
{
    public class CP_Social_Phobia : CharacterProperty
    {
        public CP_Social_Phobia(Character c, properties p) : base(c, p)
        {
        }

        /*
        private float self_mood_delta;


        /// <summary>
        /// 施展这个特性对Character c的影响
        /// </summary>
        /// <param name="c"></param>
        /// <param name="factor"></param>
        private void character_influence(Character c,int factor = 1)
        {
            if (c != owner)
            {
                var reason = $"{Localization_Utility.get_localization(owner.desc.name)} - {Localization_Utility.get_localization(desc.name)}";
                c.UpdateMood(desc.int_parms[3] * factor,reason);

                if (c.character_properties.Any(prop => prop is CP_Social_Phobia))
                {
                    self_mood_delta += desc.int_parms[1] * factor;
                }

                if (c.character_properties.Any(prop => prop is CP_Gregarious))
                {
                    self_mood_delta += desc.int_parms[2] * factor;
                }
            }
        }

        public override void Start()
        {

            var reason = $"{Localization_Utility.get_localization(owner.desc.name)} - {Localization_Utility.get_localization(desc.name)}";
            self_mood_delta = desc.int_parms[0];

            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);

            cmgr.add_character_action += (Character add_character) =>
            {
                owner.UpdateMood(-self_mood_delta,reason);
                character_influence(add_character);
                owner.UpdateMood(self_mood_delta,reason);
            };

            cmgr.remove_character_action += (Character remove_character) =>
            {
                owner.UpdateMood(-self_mood_delta, reason);
                character_influence(remove_character,-1);
                owner.UpdateMood(self_mood_delta, reason);
            };

            foreach (var c in cmgr.characters)
            {
                character_influence(c);
            }

            owner.UpdateMood(self_mood_delta, reason);
        }*/
    }
}
