using AutoCodes;
using Commons;
using Foundations;
using System.Linq;

namespace World.Characters.CharacterProperties
{
    public class CP_Gregarious : CharacterProperty
    {
        public CP_Gregarious(Character c, properties p) : base(c, p)
        {
        }
        /*
        private float self_mood_delta;

        // 这块在角色发生变化时需要重新考虑
        public override void Start()
        {
            self_mood_delta = desc.int_parms[0];
            var reason = $"{Localization_Utility.get_localization(owner.desc.name)} - {Localization_Utility.get_localization(desc.name)}";

            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);

            cmgr.add_character_action += (Character add_character) =>
            {
                if(add_character != owner)
                {
                    add_character.UpdateMood(desc.int_parms[2], reason);
                }

                if (add_character.character_properties.Any(prop => prop is CP_Gregarious))
                {
                    owner.UpdateMood(-self_mood_delta, reason);
                    self_mood_delta += desc.int_parms[1];
                    owner.UpdateMood(self_mood_delta, reason);
                }
            };

            cmgr.remove_character_action += (Character remove_character) =>
            {
                if (remove_character != owner)
                {
                    remove_character.UpdateMood(-desc.int_parms[2], reason);
                }
                if (remove_character.character_properties.Any(prop => prop is CP_Gregarious))
                {
                    owner.UpdateMood(-self_mood_delta, reason);
                    self_mood_delta -= desc.int_parms[1];
                    owner.UpdateMood(self_mood_delta, reason);
                }
            };


            foreach (var c in cmgr.characters)
            {
                if (c != owner)
                {
                    c.UpdateMood(desc.int_parms[2],reason);

                    if (c.character_properties.Any(prop => prop is CP_Gregarious))
                    {
                        self_mood_delta += desc.int_parms[1];
                    }
                }
            }

            owner.UpdateMood(self_mood_delta, reason);
        }
        */
    }
}
