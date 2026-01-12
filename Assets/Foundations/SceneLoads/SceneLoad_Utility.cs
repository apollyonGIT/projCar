using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Foundations.SceneLoads
{
    public class SceneLoad_Utility
    {
        public static void load_scene_with_loading(string key, bool is_loading_scene_shadow = false)
        {
            Share_DS.instance.add("next_scene", key);
            Share_DS.instance.add("is_loading_scene_shadow", is_loading_scene_shadow);
            load_scene("LoadingScene");
        }


        public static void load_scene(string key, LoadSceneMode mode = LoadSceneMode.Single)
        {
            Addressables.LoadSceneAsync(key, mode).Completed += on_load_scene;

            #region 子函数 on_load_scene
            void on_load_scene(AsyncOperationHandle<SceneInstance> handle)
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    if (mode == LoadSceneMode.Additive)
                        Share_DS.instance.add(key, handle.Result);

                    Debug.Log("场景加载成功！");
                }
                else
                {
                    Debug.LogError($"场景加载失败: {handle.OperationException}");
                }
            }
            #endregion
        }


        public static async void load_scene_async(string key, LoadSceneMode mode = LoadSceneMode.Single)
        {
            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(key, mode);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (mode == LoadSceneMode.Additive)
                    Share_DS.instance.add(key, handle.Result);

                Debug.Log("场景加载成功！");
            }
            else
            {
                Debug.LogError($"场景加载失败: {handle.OperationException}");
            }
        }


        public static void unload_additive_scene(string key)
        {
            if (!Share_DS.instance.try_get_value(key, out SceneInstance sceneInstance)) return;

            Addressables.UnloadSceneAsync(sceneInstance).Completed += handle =>
            {
                Share_DS.instance.remove(key);

                Debug.Log("场景已卸载！");
            };
        }
    }
}

