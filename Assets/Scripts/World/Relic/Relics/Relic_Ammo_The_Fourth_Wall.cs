using Commons;
using Foundations;
using UnityEngine;
using World.Projectiles;

namespace World.Relic.Relics
{
    public class Relic_Ammo_The_Fourth_Wall : Relic
    {
        public override void Get()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.add_tick_event += check_fourth_wall;
        }

        private void check_fourth_wall(Projectile p)
        {
            if (p.movement_status != MovementStatus.normal || p.faction != WorldEnum.Faction.player)
                return;
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            var pos = new Vector3(p.position.x,p.position.y,10f);
            var cam_pos = WorldSceneRoot.instance.mainCamera.transform.position;
            var obj_dis = (Config.current.desiredResolution.x / (float)Config.current.pixelPerUnit) * (1 + Mathf.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / pos.z);
            var obj_wid = (Config.current.desiredResolution.y / (float)Config.current.pixelPerUnit) * (1 + Mathf.Abs(WorldSceneRoot.instance.mainCamera.transform.position.z) / pos.z);


            if(pos.x  < cam_pos.x - obj_dis / 2)
            {
                var camera_v = WorldContext.instance.camera_velocity;
                var v = p.velocity - camera_v;
                v = new Vector2(-v.x, v.y);
                v += camera_v;

                p.direction = v.normalized;
                p.velocity = v;
                p.position = new Vector2(pos.x + 0.1f, p.position.y);

                pmgr.rebound_event?.Invoke(p);
            }
            else if (pos.x > cam_pos.x + obj_dis / 2)
            {
                var camera_v = WorldContext.instance.camera_velocity;
                var v = p.velocity - camera_v;
                v = new Vector2(-v.x, v.y);
                v += camera_v;

                p.direction = v.normalized;
                p.velocity = v;
                p.position = new Vector2(pos.x - 0.1f, p.position.y);

                pmgr.rebound_event?.Invoke(p);
            }
            else if (pos.y > cam_pos.y + obj_wid / 2)
            {
                var camera_v = WorldContext.instance.camera_velocity;
                var v = p.velocity - camera_v;
                v = new Vector2(v.x, -v.y);
                v += camera_v;

                p.direction = v.normalized;
                p.velocity = v;
                p.position = new Vector2(pos.x, p.position.y -0.1f);

                pmgr.rebound_event?.Invoke(p);
            }
        }


        public override void Drop()
        {
            Mission.instance.try_get_mgr(Config.ProjectileMgr_Name, out ProjectileMgr pmgr);
            pmgr.add_tick_event -= check_fourth_wall;
        }
    }
}
