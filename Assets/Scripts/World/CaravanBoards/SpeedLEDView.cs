using Commons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace World.CaravanBoards
{
    public class SpeedLEDView : MonoBehaviour
    {
        public GameObject high_on;
        public GameObject low_on;

        //==================================================================================================

        public void tick()
        {
            var config = Config.current;
            var vx = WorldContext.instance.caravan_velocity.x;

            high_on.SetActive(vx >= config.car_vx_high_speed);
            low_on.SetActive(vx <= config.car_vx_low_speed);
        }
    }
}

