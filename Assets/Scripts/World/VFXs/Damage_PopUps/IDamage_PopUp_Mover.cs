using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World.VFXs.Damage_PopUps
{
    public interface IDamage_PopUp_Mover
    {
        void init(Damage_PopUp_Mono owner);
        void move(Damage_PopUp_Mono owner);
    }
}

