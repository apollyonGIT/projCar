using UnityEditor;
using UnityEditor.SceneManagement;

namespace Foundation_Editors.PlayModes
{
    public class PlayWithFistScene
    {
        private const string MENU_NAME = "PlayMode/FirstScene";
        private const string EDITOR_PREFS_KEY = "play_with_first_scene";

        //==================================================================================================

        [UnityEditor.InitializeOnLoadMethod]
        private static void init()
        {
            EditorApplication.playModeStateChanged += on_play_mode_state_changed;
        }


        private static void on_play_mode_state_changed(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                if (!EditorPrefs.GetBool(EDITOR_PREFS_KEY))
                {
                    EditorSceneManager.playModeStartScene = null;
                    return;
                }

                set_first_scene_as_start_scene();
            }
        }


        static void set_first_scene_as_start_scene()
        {
            string path = null;

            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    path = scene.path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(path)) return;

            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
        }


        [MenuItem(MENU_NAME)]
        public static void toggle()
        {
            if (EditorPrefs.GetBool(EDITOR_PREFS_KEY))
            {
                EditorPrefs.SetBool(EDITOR_PREFS_KEY, false);
            }
            else
            {
                EditorPrefs.SetBool(EDITOR_PREFS_KEY, true);
            }
        }


        [MenuItem(MENU_NAME, true)]
        private static bool toggle_validate()
        {
            Menu.SetChecked(MENU_NAME, EditorPrefs.GetBool(EDITOR_PREFS_KEY));
            return true;
        }

    }
}

