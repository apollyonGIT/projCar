using Foundations;
using UnityEngine;

namespace World.VFXs.Damage_PopUps
{
    public class Damage_PopUp : MonoBehaviourSingleton<Damage_PopUp>
    {
        public Damage_PopUp_Mono model;

        //==================================================================================================

        public void create_cell(params object[] prms)
        {
            var cell = Instantiate(model, transform);
            cell.init(prms);            
        }
    }


    
}

