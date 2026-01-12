using Foundations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World.DiceGames
{
    public class Dice_Init : MonoBehaviour
    {
        private void Start()
        {
            Share_DS.instance.add("player_score", 0);
            Share_DS.instance.add("player_turn_score", 0);

            Share_DS.instance.add("ai_score", 0);
            Share_DS.instance.add("ai_turn_score", 0);
        }

        
    }
}

