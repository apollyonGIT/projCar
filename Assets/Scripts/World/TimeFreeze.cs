using UnityEngine;

namespace World
{
    public class TimeFreeze : MonoBehaviour
    {
        public void ChangeTimeScale(float scale)
        {
            Time.timeScale = scale;
        }
    }
}
