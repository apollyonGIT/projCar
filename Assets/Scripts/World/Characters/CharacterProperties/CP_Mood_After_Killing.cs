using AutoCodes;
using World.Devices;
using World.Devices.Device_AI;
using World.Helpers;

namespace World.Characters.CharacterProperties
{
    public class CP_Mood_After_Killing : CharacterProperty
    {
        public float mood_delta;

        public CP_Mood_After_Killing(Character c, properties p) : base(c, p)
        {
        }
        /*
        public override void Init()
        {
            owner.start_work += StartWork;
            owner.end_work += EndWork;

            owner.enter_safe_area += EnterSafeArea;
        }

        private void EnterSafeArea()
        {
            owner.UpdateMood(mood_delta);
            mood_delta = 0;
        }

        private void EndWork()
        {
            Character_Module_Helper.character_module.TryGetValue(owner, out var module);
            if (module is DeviceModule dm)
                if (dm.device != null)
                    dm.device.kill_enemy_action -= KillEnemy;
        }

        private void StartWork()
        {
            Character_Module_Helper.character_module.TryGetValue(owner, out var module);
            if (module is DeviceModule dm)
                if (dm.device != null)
                    dm.device.kill_enemy_action += KillEnemy;
        }

        private void KillEnemy(ITarget t)
        {
            if (t is Device d)
            {
                Character_Module_Helper.character_module.TryGetValue(owner, out var module);
                if (module is DeviceModule md && d.module_list.Contains(md))
                    mood_delta += desc.float_parms[0];
            }
        }
        */
    }
}
