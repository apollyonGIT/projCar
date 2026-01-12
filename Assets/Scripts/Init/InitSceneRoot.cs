using Commons;
using Commons.Levels;
using Foundations;
using Foundations.SceneLoads;
using UnityEngine;

namespace Init
{
    public class InitSceneRoot : SceneRoot<InitSceneRoot>
    {
        public GameObject go_btn_continue_game;

        bool m_is_first_interactive;

        //==================================================================================================

        protected override void on_init()
        {
            Share_DS.instance.try_get_value(Game_Mgr.key_is_continue_game, out bool is_continue_game);
            //go_btn_continue_game.SetActive(is_continue_game);

            go_btn_continue_game.SetActive(false); //临时：无论如何，都不去显示继续游戏

            m_is_first_interactive = true;

            base.on_init();
        }


        protected override void on_fini()
        {
            base.on_fini();
        }


        public void btn_start_new_game()
        {
            if (!m_is_first_interactive) return;
            m_is_first_interactive = false;

            Game_Mgr.on_start_new_game();

            SceneLoad_Utility.load_scene("CampScene");
        }


        public void btn_continue_game()
        {
            if (!m_is_first_interactive) return;
            m_is_first_interactive = false;

            Game_Mgr.on_continue_game();

            SceneLoad_Utility.load_scene("CampScene");
        }


        public void btn_exit()
        {
            if (!m_is_first_interactive) return;
            m_is_first_interactive = false;

            Game_Mgr.on_exit_game();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }


        public void btn_start_test_game()
        {
            Game_Frame_Mgr.init();
            Game_Mgr.on_enter_world($"{Config.current.test_level_id}");

            SceneLoad_Utility.load_scene_with_loading("TestScene");
        }
    }
}

