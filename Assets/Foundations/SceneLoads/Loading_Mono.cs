using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Collections;

namespace Foundations
{
    public class Loading_Mono : MonoBehaviour
    {
        public Slider loadingSlider;
        public GameObject shadow;

        string m_next_scene_key;

        AsyncOperationHandle<SceneInstance> m_sceneLoadHandle;

        //==================================================================================================

        private void Awake()
        {
            Share_DS.instance.try_get_value("next_scene", out m_next_scene_key);

            //是否开启遮罩
            if (Share_DS.instance.try_get_value("is_loading_scene_shadow", out bool is_loading_scene_shadow))
            {
                Share_DS.instance.add("is_loading_scene_shadow", false);
                shadow.SetActive(is_loading_scene_shadow);
            }
            else
                shadow.SetActive(false);

            StartCoroutine(DownloadAndLoadScene());
        }


        private IEnumerator DownloadAndLoadScene()
        {
            m_sceneLoadHandle = Addressables.LoadSceneAsync(m_next_scene_key, UnityEngine.SceneManagement.LoadSceneMode.Single, activateOnLoad: true);
            while (!m_sceneLoadHandle.IsDone)
            {
                UpdateProgress(m_sceneLoadHandle.PercentComplete);
                yield return null;
            }
        }


        void UpdateProgress(float progress)
        {
            loadingSlider.value = progress;
        }
    }
}
