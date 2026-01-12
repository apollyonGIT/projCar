using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Foundations.SavePrefabs
{
    public class SavePrefab : MonoBehaviour
    {
        public GameObject target_prefab;
        public GameObject source_prefab;

        //==================================================================================================

        public void @do()
        {
            #if UNITY_EDITOR
            PrefabUtility.SaveAsPrefabAsset(target_prefab, AssetDatabase.GetAssetPath(source_prefab));
            #endif
        }
    }
}

