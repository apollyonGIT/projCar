using UnityEngine;

namespace World.Encounters.Dialogs
{
    public class Replace : MonoBehaviour, IEncounter_Dialog_CPN
    {
        string m_key_name;
        string IEncounter_Dialog_CPN.key_name { set => m_key_name = value; }

        //==================================================================================================

        void IEncounter_Dialog_CPN.@do(IEncounter_Dialog_Window_UI owner, string[] args)
        {
            CPN_Utility.get_replace_strs(args[0], out var replace_strs);
            CPN_Utility.get_replace_strs(args[1], out var new_strs);

            owner.change(replace_strs, new_strs, null, null);
        }
    }
}

