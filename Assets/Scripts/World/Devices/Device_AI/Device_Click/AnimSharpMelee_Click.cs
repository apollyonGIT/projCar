using AutoCodes;
using Commons;
using Foundations.Tickers;

namespace World.Devices.Device_AI
{
    public class AnimSharpMelee_Click:BasicMelee_Click
    {
        public enum Stone_State
        {
            idle,
            sharpening,
        }

        public Stone_State stone_state; 
        public float sharpen_percent;
        public const float SHARPENING_SECOND = 3f;

        public override void InitData(device_all rc)
        {
            base.InitData(rc);
            stone_state = Stone_State.idle;
        }

        public bool Sharpening()
        {
            if(stone_state!=Stone_State.idle) return false;

            stone_state = Stone_State.sharpening;

            Request request = new Request("sharpen",
                (_) => { return sharpen_percent >= 1; },
                (_) => { sharpen_percent = 0; },
                (_) => { stone_state = Stone_State.idle; DeviceBehavior_Melee_Sharpen(1f); },
                (_) => { sharpen_percent += 1f / (SHARPENING_SECOND * Config.PHYSICS_TICKS_PER_SECOND); }
                );

            return true;
        }
    }
}
