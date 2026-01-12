using Commons;
using Foundations;
using System.Collections.Generic;

namespace World.Characters
{
    public class CharacterPD : Producer
    {
        public CharacterMgrView cmv;
        public override IMgr imgr => mgr;
        CharacterMgr mgr;

        //==================================================================================================

        public override void init(int priority)
        {
            if (Saves.Save_DS.instance.need_load_device)
                use_current_data(priority);
            else
                use_default_data(priority);
        }


        void use_default_data(int priority)
        {
            mgr = new(Config.CharacterMgr_Name, priority);
            mgr.add_view(cmv);

            foreach (var id in Config.current.init_roles)
            {
                mgr.AddCharacter((int)id);
            }
        }


        void use_current_data(int priority)
        {
            var datas = Saves.Save_DS.instance.character_datas;

            mgr = new(Config.CharacterMgr_Name, priority);

            foreach (var cell_info in datas)
            {
                var cell = mgr.AddCharacter((int)(uint)cell_info[0]);

               // cell.SetEatingState((EatingState)(int)cell_info[2]); //饱食度

                cell.character_properties = new(); //词条
                var character_property_keys = (List<uint>)cell_info[1];
                foreach (var character_property_key in character_property_keys)
                {
                    var character_property = CharacterUtility.GetProperty(cell, character_property_key);
                    character_property.Init();
                    cell.character_properties.Add(character_property);
                }

                // cell.current_mood = (float)cell_info[1]; //心情
            }

            mgr.add_view(cmv);
        }


        public override void call()
        {
        }
    }
}