using UnityEngine;

namespace Foundations.Binarys
{
    public class BinaryAsset : ScriptableObject
    {
        public byte[] bytes;

        public string[] fields;
        public string[] type_strs;
    }
}

