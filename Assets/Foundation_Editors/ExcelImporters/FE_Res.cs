using UnityEngine;

namespace Foundation_Editors.ExcelImporters
{
    public class FE_Res : ScriptableObject
    {
        public Texture2D excelAssetIcon;
        public Texture2D binaryAssetIcon;

        //==================================================================================================

        static FE_Res _instance = null;
        public static FE_Res instance 
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateInstance<FE_Res>();
                }

                return _instance;
            }
        }
    }
}

