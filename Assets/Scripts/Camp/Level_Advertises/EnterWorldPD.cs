using Commons;
using Commons.Levels;
using Foundations;
using System.Collections.Generic;

namespace Camp.Level_Advertises
{
    public class EnterWorldPD : Producer
    {
        public Btn_Enter_World btn_enter_world_model;

        public override IMgr imgr => null;

        //==================================================================================================

        public override void init(int priority)
        {
            foreach (var btn_enter_world in cells())
            {
                btn_enter_world.gameObject.SetActive(true);
            }

            gameObject.SetActive(false);
        }


        public override void call()
        {
            gameObject.SetActive(!gameObject.activeSelf);
        }


        IEnumerable<Btn_Enter_World> cells()
        {
            Share_DS.instance.try_get_value(Game_Mgr.key_world_and_level_infos, out Dictionary<string, Game_Frame_Mgr.Struct_world_and_level_info> world_and_level_infos);
            foreach (var (world_id, info) in world_and_level_infos)
            {
                var cell = Instantiate(btn_enter_world_model, transform);
                var r_game_world = info.r_game_world;

                var world_name = Localization_Utility.get_localization(r_game_world.world_name);
                //var world_progress = ((List<string>)r_game_world.diy_obj)[info.world_progress];

                cell.init(world_id, world_name);

                yield return cell;
            }
        }
    }
}