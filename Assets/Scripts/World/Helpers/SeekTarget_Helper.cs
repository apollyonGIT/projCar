using Commons;
using Foundations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World.Helpers
{
    public class SeekTarget_Helper
    {

        //==================================================================================================

        public static IEnumerable<ITarget> all_targets()
        {
            Mission.instance.try_get_mgr("DeviceMgr", out Devices.DeviceMgr deviceMgr);
            foreach (var target in deviceMgr.slots_device.Where(t => t.slot_device != null).Select(t => t.slot_device))
            {
                if (target.current_hp <= 0) continue;
                yield return target;
            }

            Mission.instance.try_get_mgr("CaravanMgr", out Caravans.CaravanMgr caravanMgr);
            yield return caravanMgr.cell;

            Mission.instance.try_get_mgr("EnemyMgr", out Enemys.EnemyMgr enemyMgr);
            foreach (var target in enemyMgr.cells)
            {
                if (target.hp <= 0) continue;
                yield return target;
            }
        }


        public static IEnumerable<ITarget> player_parts()
        {
            Mission.instance.try_get_mgr("DeviceMgr", out Devices.DeviceMgr deviceMgr);
            foreach (var target in deviceMgr.slots_device.Where(t => t.slot_device != null).Select(t => t.slot_device))
            {
                if (target.current_hp <= 0) continue;
                if (target.desc.device_type.value == Foundations.Excels.Device_Type.EN_Device_Type.Wheel) continue;
                yield return target;
            }

            Mission.instance.try_get_mgr("CaravanMgr", out Caravans.CaravanMgr caravanMgr);
            yield return caravanMgr.cell;
        }


        public static IEnumerable<ITarget> all_player_devices()
        {
            Mission.instance.try_get_mgr("DeviceMgr", out Devices.DeviceMgr deviceMgr);
            foreach (var target in deviceMgr.slots_device.Where(t => t.slot_device != null).Select(t => t.slot_device))
            {
                if (target.current_hp <= 0) continue;
                if (target.desc.device_type.value == Foundations.Excels.Device_Type.EN_Device_Type.Wheel) continue;
                yield return target;
            }
        }


        public static void seek_wheel(out ITarget target)
        {
            target = default;

            Mission.instance.try_get_mgr("DeviceMgr", out Devices.DeviceMgr deviceMgr);
            foreach (var _target in deviceMgr.slots_device.Where(t => t.slot_device != null).Where(t => t.slot_device.desc.device_type.value == Foundations.Excels.Device_Type.EN_Device_Type.Wheel).Select(t => t.slot_device))
            {
                if (_target.current_hp > 0)
                    target = _target;
            }
        }


        public static void nearest_player_part(ITarget self, out ITarget target)
        {
            target = default;
            float dis = 99999f;

            foreach (var temp_target in player_parts())
            {
                var temp_dis = temp_target.distance(self);
                if (temp_dis >= dis) continue;

                dis = temp_dis;
                target = temp_target;
            }
        }


        public static void nearest_player_device(ITarget self, out ITarget target)
        {
            target = default;
            float dis = 99999f;

            foreach (var temp_target in all_player_devices())
            {
                var temp_dis = temp_target.distance(self);
                if (temp_dis >= dis) continue;

                dis = temp_dis;
                target = temp_target;
            }
        }


        public static void nearest_player_device_without_wheel(ITarget self, out ITarget target)
        {
            target = default;
            float dis = 99999f;

            foreach (var temp_target in all_player_devices())
            {
                if ((temp_target as Devices.Device).desc.device_type.value == Foundations.Excels.Device_Type.EN_Device_Type.Wheel)
                    continue;

                var temp_dis = temp_target.distance(self);
                if (temp_dis >= dis) continue;

                dis = temp_dis;
                target = temp_target;
            }
        }


        public static void nearest_player_device_using_dir(ITarget self, int dir_flag, out ITarget target)
        {
            target = default;
            float dis = 99999f;

            foreach (var temp_target in all_player_devices())
            {
                if ((temp_target.Position.x - self.Position.x) * dir_flag < 0) continue;

                var temp_dis = temp_target.distance(self);
                if (temp_dis >= dis) continue;

                dis = temp_dis;
                target = temp_target;
            }
        }


        public static void random_player_part(out ITarget target)
        {
            var e = player_parts();
            var index = Random.Range(0, e.Count());

            target = e.ElementAt(index);
        }


        public static void select_caravan(out ITarget target)
        {
            Mission.instance.try_get_mgr("CaravanMgr", out Caravans.CaravanMgr caravanMgr);
            target = caravanMgr.cell;
        }


        public static IEnumerable<ITarget> all_projectiles()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out Projectiles.ProjectileMgr projectileMgr);
            foreach (var projectile in projectileMgr.projectiles)
            {
                var t = projectile as ITarget;

                if (t.hp <= 0) continue;
                yield return t;
            }


        }
    }
}

