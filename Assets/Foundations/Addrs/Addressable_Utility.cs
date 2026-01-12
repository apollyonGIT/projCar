using Foundations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Addrs
{
    public class Addressable_Utility
    {

        //==================================================================================================

        public static bool try_load_asset<T>(string key, out T asset)
        {
            asset = default;

            if (typeof(Component).IsAssignableFrom(typeof(T)))
            {
                if (!do_load<GameObject>(key, out var _asset)) return false;

                var ret = _asset.TryGetComponent(out asset);
                if (!ret)
                    Debug.LogError("已取得GameObject，但Component转化错误，请检查脚本名");

                return ret;
            }

            return do_load(key, out asset);
        }


        static bool do_load<T>(string key, out T asset)
        {
            asset = default;

            var opHandle = Addressables.LoadAssetAsync<T>(key);
            opHandle.WaitForCompletion();

            if (opHandle.Status != AsyncOperationStatus.Succeeded) return false;
            asset = opHandle.Result;

            return true;
        }
    }
}

