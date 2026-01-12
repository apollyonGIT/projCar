using Commons;
using Foundations;
using System.Collections.Generic;
using World.Characters;
using World.Devices;
using World.Devices.Device_AI;

namespace World.Helpers
{
    public class Character_Module_Helper
    {
        public static Dictionary<Character,BasicModule> character_module = new Dictionary<Character, BasicModule>();

        public static Character GetSelectCharacter()
        {
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            return cmgr.select_character;
        }

        public static void SetSelectCharacter(Character c)
        {
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            //cmgr.SelectCharacter(c);
        }

        public static void SetModule(BasicModule dm)
        {
            var c = GetSelectCharacter();
            if (c == null )        //没有选中的角色，或者角色正在罢工
                return;


            if(dm is DeviceModule devicemodule){            //判断一下角色能不能工作这个设备
                if(devicemodule.device != null)
                {
                    if (!c.CanWork(devicemodule.device))
                    {
                        return;
                    }
                }
            }

            EmptyModule(dm);

            if (character_module.ContainsKey(c))
            {
                EmptyModule(character_module[c]);
            }

            character_module.Add(c, dm);
            set_module(c, dm);
        }

        public static Device GetDevice(Character c)
        {
            if(character_module.ContainsKey(c))
            {
                var dm = character_module[c];
                if (dm is DeviceModule devicemodule)
                {
                    return devicemodule.device;
                }
            }

            return null;
        }

        private static void set_module(Character c , BasicModule dm)
        {
            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            dm.SetWorker(true);
            cmgr.SetCharacterWorking(c, true);
            dm.SetModule();
            cmgr.CancelSelectCharacter();

            if (dm is DeviceModule deviceModule)
            {
                Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
                //dmgr.DeviceSetStation(dm as DeviceModule);
            }
        }

        public static void EmptyModule(BasicModule dm)
        {
            var remove_character = new Character();
            var record_module = new BasicModule();

            foreach(var (character,module) in character_module)
            {
                if(module == dm)
                {
                    record_module = module;
                    remove_character = character;
                }
            }

            Mission.instance.try_get_mgr(Config.CharacterMgr_Name, out CharacterMgr cmgr);
            cmgr.SetCharacterWorking(remove_character, false);

            character_module.Remove(remove_character);
            record_module.SetWorker(false);
            record_module.SetModule();

            if (dm is DeviceModule deviceModule)
            {
                Mission.instance.try_get_mgr(Config.DeviceMgr_Name, out DeviceMgr dmgr);
              //  dmgr.DeviceSetStation(dm as DeviceModule);
            }
        }

        public static Character GetModule(BasicModule dm)
        {
            foreach (var (character, module) in character_module)
            {
                if (module == dm)
                {
                    return character;
                }
            }
            return null;
        }

        public static void FreeCharacter(Character c)
        {
            if (character_module.ContainsKey(c))
            {
                EmptyModule(character_module[c]);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"试图让一个不在工作的人{c.desc.id.ToString()}离开工作");
            }
        }
    }
}
