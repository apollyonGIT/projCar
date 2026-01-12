using AutoCodes;
using World.Characters.CharacterProperties;

namespace World.Characters
{
    public static class CharacterUtility
    {
        public static bool CheckDeviceTag(Devices.Device d,uint tag_int)
        {
            return d.desc.device_tag.Contains(tag_int);
        }

        public static CharacterProperty GetProperty(Character c,uint p_id)
        {
            propertiess.TryGetValue(p_id.ToString(), out properties p);
            switch (p.script)
            {
                case "CP_Device_Skilled":
                    return new CP_Device_Skilled(c, p);
                case "CP_Device_Disabled":
                    return new CP_Device_Disabled(c, p);
                case "CP_Mood_After_Killing":
                    return new CP_Mood_After_Killing(c, p);
                case "CP_Mood_After_Working":
                    return new CP_Mood_After_Working(c, p);
                case "CP_Mood_After_Resting":
                    return new CP_Mood_After_Resting(c, p);
                case "CP_Social_Phobia":
                    return new CP_Social_Phobia(c, p);
                case "CP_Gregarious":
                    return new CP_Gregarious(c, p);
                case "CP_Empty":
                    return new CP_Empty(c, p);
                default:
                    return new CharacterProperty(c, p);
            }
        }
    }
}
