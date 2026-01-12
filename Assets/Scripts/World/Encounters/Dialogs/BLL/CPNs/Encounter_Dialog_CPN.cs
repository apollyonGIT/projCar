using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World.Encounters.Dialogs
{
    public interface IEncounter_Dialog_CPN
    {
        string key_name { set; }

        void @do(IEncounter_Dialog_Window_UI owner, string[] args);
    }
}

