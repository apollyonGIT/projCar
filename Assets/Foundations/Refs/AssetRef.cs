using UnityEngine;

namespace Foundations.Refs
{
    [CreateAssetMenu(fileName = "asset_ref", menuName = "AssetRef")]
    public class AssetRef : ScriptableObject
    {
        public Object asset;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (asset == this)
            {
                asset = null;
            }
        }
#endif
    }

}