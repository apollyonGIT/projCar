using Camp.Level_Advertises;
using Camp.Techs;
using System.Linq;
using UnityEngine;

namespace Camp
{
    public class Btn_Open_Tech : MonoBehaviour
    {

        //==================================================================================================

        public void @do()
        {
            var pd = CampSceneRoot.instance.producers.Where(t => t is TechPD).First();
            if (pd == null) return;

            pd.call();
        }
    }
}

