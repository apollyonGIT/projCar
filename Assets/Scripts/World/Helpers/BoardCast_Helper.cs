using System;

namespace World.Helpers
{
    public class BoardCast_Helper
    {
        public static void to_all_target(Action<ITarget> request)
        {
            var all_target = SeekTarget_Helper.all_targets();
            foreach (var target in all_target)
            {
                target.tick_handle(request);
            }
        }


        public static void to_all_projectile(Action<ITarget> request)
        {
            var all_target = SeekTarget_Helper.all_projectiles();
            foreach (var target in all_target)
            {
                target.tick_handle(request);
            }
        }
    }
}

