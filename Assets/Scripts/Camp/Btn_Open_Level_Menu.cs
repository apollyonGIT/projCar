using Camp.Level_Advertises;
using System.Linq;
using UnityEngine;

namespace Camp
{
    public class Btn_Open_Level_Menu : MonoBehaviour
    {

        //==================================================================================================

        public void @do()
        {
            var pd = CampSceneRoot.instance.producers.Where(t => t is EnterWorldPD).First();
            if (pd == null) return;

            pd.call();
        }
    }
}

