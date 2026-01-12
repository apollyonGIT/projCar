using UnityEngine;

namespace World.Enemys.BT
{
    public class Enemy_BT_Audio_CPN
    {

        //==================================================================================================

        public static void play_attack_audio(Enemy cell)
        {
            Audio.AudioSystem.instance.PlayOneShot(cell._desc.SE_attack);
        }


        
    }
}

