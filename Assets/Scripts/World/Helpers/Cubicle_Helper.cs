using AutoCodes;
using Commons;
using Foundations;
using System.Collections.Generic;
using World.Characters;
using World.Cubicles;
using World.Devices;

namespace World.Helpers
{
    public class Cubicle_Helper
    {
        public static Dictionary<Character, BasicCubicle> character_cubicle = new Dictionary<Character, BasicCubicle>();
        public static BasicCubicle AddDeviceCubicle(Device device,uint cub_id)
        {
            device_cubicless.TryGetValue(cub_id.ToString(), out var cubicle);
            return add_devicecubicle(device, cubicle);
        }

        private static BasicCubicle add_devicecubicle(Device device, device_cubicles cubicle)
        {
            Mission.instance.try_get_mgr("CubiclesMgr", out CubiclesMgr cmgr);
            var cub = new BasicCubicle(cubicle);
            switch (cubicle.cubicle_behaviour)
            {
                case "DeviceAttackCubicle":
                    cub = new DeviceAttackCubicle(device,cubicle);
                    cmgr.AddCubicle(cub);
                    break;
                case "DeviceLoadCubicle":
                    cub = new DeviceLoadCubicle(device, cubicle);
                    cmgr.AddCubicle(cub);
                    break;
                case "DeviceSharpCubicle":
                    cub = new DeviceSharpCubicle(device, cubicle);
                    cmgr.AddCubicle(cub);
                    break;
                case "DeviceRotateCubicle":
                    cub = new DeviceRotateCubicle(device, cubicle);
                    cmgr.AddCubicle(cub);
                    break;
                case "DeviceCarryCubicle":
                    cub = new DeviceCarryCubicle(device, cubicle);
                    cmgr.AddCubicle(cub);
                    break;
                case "DeviceChargeCubicle":
                    cub = new DeviceChargeCubicle(device, cubicle);
                    cmgr.AddCubicle(cub);
                    break;
                default:
                    break;
            }

            return cub;
        }

        public static void RemoveCubicle()
        {

        }

        public static void SetCubicle(BasicCubicle bc)
        {
            var c = GetSelectCharacter();
            if(c == null) //没有选中的角色，或者角色正在罢工
                return;

            if(bc is DeviceCubicle device_cubicle)
            {
                if(device_cubicle != null)
                {
                    if (!c.CanWork(device_cubicle.device))
                    {
                        return;
                    }
                }
            }

            EmptyCubicle(bc);
            if (character_cubicle.ContainsKey(c))
            {
                EmptyCubicle(character_cubicle[c]);
            }

            character_cubicle.Add(c, bc);
            set_cubicle(c, bc);

            SetSelectCharacter(null);
        }

        private static void set_cubicle(Character c, BasicCubicle bc)
        {
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            bc.set_worker(c);
            cmgr.SetCharacterWorking(c, true);
            cmgr.CancelSelectCharacter();
        }

        public static Character GetSelectCharacter()
        {
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            return cmgr.select_character;
        }

        public static void SetSelectCharacter(Character c)
        {
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            Mission.instance.try_get_mgr("CubiclesMgr", out CubiclesMgr cub_mgr);
            if(c == null)
            {
                cmgr.CancelSelectCharacter();
                cub_mgr.CancelHightLightCubicles();

                return;
            }
            cmgr.SelectCharacter(c);   
            cub_mgr.HighLightCubicles(c);
        }


        public static void EmptyCubicle(BasicCubicle bc)
        {
            var remove_character = new Character();

            remove_character = bc.worker;

            if(remove_character!=null)
                character_cubicle.Remove(remove_character);

            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            if(remove_character!=null)
            cmgr.SetCharacterWorking(remove_character, false);
            bc.set_worker(null);
        }
    }
}
