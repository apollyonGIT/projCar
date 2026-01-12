using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Caravans;
using World.Enemys;

namespace World.Devices
{
    public class DeviceCollider : MonoBehaviour
    {
        public Device device;

        public string collider_name;

        public Action<ITarget> enter_event;

        public Action<ITarget> stay_event;

        public Action<ITarget> exit_event;

        public List<DeviceCollider2D> targets_in_colliders = new();

        public class DeviceCollider2D
        {
            public Collider2D collider;
            public bool need_remove;

            public DeviceCollider2D(Collider2D c2d,bool need_remove)
            {
                collider = c2d;
                this.need_remove = need_remove;
            }
        }

        
        public void OnTriggerEnter2D(Collider2D collision)
        {
            if(device.faction == WorldEnum.Faction.player)
            {
                if(collision.TryGetComponent<EnemyHitbox>(out var eb))
                {
                    enter_event?.Invoke(eb.view.owner);
                    targets_in_colliders.Add(new DeviceCollider2D(collision,false));
                }
                    
            }
            else if(device.faction == WorldEnum.Faction.opposite)
            {
                if(collision.TryGetComponent<DeviceHitBox>(out var deviceHitBox))
                {
                    if(deviceHitBox.view.owner.faction == WorldEnum.Faction.player)
                    {
                        enter_event?.Invoke(deviceHitBox.view.owner);
                        targets_in_colliders.Add(new DeviceCollider2D(collision, false));
                    }
                }
               
                if(collision.TryGetComponent<CaravanHitBox>(out var caravanHitBox))
                {
                    if (caravanHitBox.view.owner.faction == WorldEnum.Faction.player)
                    {
                        enter_event?.Invoke(caravanHitBox.view.owner);
                        targets_in_colliders.Add(new DeviceCollider2D(collision, false));
                    }
                }
            }
        }

        public void collider_tick()
        {
            for(int i = targets_in_colliders.Count - 1; i >= 0; i--)
            {
                if (targets_in_colliders[i].need_remove)
                {
                    targets_in_colliders.RemoveAt(i);
                }
            }


            foreach(var device_collider in targets_in_colliders)
            {
                var collision = device_collider.collider;

                if (device.faction == WorldEnum.Faction.player)
                {
                    collision.TryGetComponent<EnemyHitbox>(out var eb);
                    stay_event?.Invoke(eb.view.owner);
                }
                else if (device.faction == WorldEnum.Faction.opposite)
                {
                    if (collision.TryGetComponent<DeviceHitBox>(out var deviceHitBox))
                    {
                        if (deviceHitBox.view.owner.faction == WorldEnum.Faction.player)
                            stay_event?.Invoke(deviceHitBox.view.owner);
                    }

                    if (collision.TryGetComponent<CaravanHitBox>(out var caravanHitBox))
                    {
                        if (caravanHitBox.view.owner.faction == WorldEnum.Faction.player)
                            stay_event?.Invoke(caravanHitBox.view.owner);
                    }
                }
            }
        }


        /*public void OnTriggerStay2D(Collider2D collision)
        {
            if (device.faction == WorldEnum.Faction.player)
            {
                collision.TryGetComponent<EnemyHitbox>(out var eb);
                stay_event?.Invoke(eb.view.owner);
            }
            else if (device.faction == WorldEnum.Faction.opposite)
            {
                if (collision.TryGetComponent<DeviceHitBox>(out var deviceHitBox))
                {
                    if (deviceHitBox.view.owner.faction == WorldEnum.Faction.player)
                        stay_event?.Invoke(deviceHitBox.view.owner);
                }

                if (collision.TryGetComponent<CaravanHitBox>(out var caravanHitBox))
                {
                    if (caravanHitBox.view.owner.faction == WorldEnum.Faction.player)
                        stay_event?.Invoke(caravanHitBox.view.owner);
                }
            }
        }*/

        public void OnTriggerExit2D(Collider2D collision)
        {
            if (device.faction == WorldEnum.Faction.player)
            {
                collision.TryGetComponent<EnemyHitbox>(out var eb);
                exit_event?.Invoke(eb.view.owner);

                var t = targets_in_colliders.FirstOrDefault(d => d.collider == collision);
                if(t != null)
                {
                    t.need_remove = true;
                }
            }
            else if (device.faction == WorldEnum.Faction.opposite)
            {
                if (collision.TryGetComponent<DeviceHitBox>(out var deviceHitBox))
                {
                    if (deviceHitBox.view.owner.faction == WorldEnum.Faction.player)
                    {
                        exit_event?.Invoke(deviceHitBox.view.owner);
                        var t = targets_in_colliders.FirstOrDefault(d => d.collider == collision);
                        if (t != null)
                        {
                            t.need_remove = true;
                        }
                    }
                        
                }

                if (collision.TryGetComponent<CaravanHitBox>(out var caravanHitBox))
                {
                    if (caravanHitBox.view.owner.faction == WorldEnum.Faction.player)
                    {
                        exit_event?.Invoke(caravanHitBox.view.owner);

                        var t = targets_in_colliders.FirstOrDefault(d => d.collider == collision);
                        if (t != null)
                        {
                            t.need_remove = true;
                        }
                    }                }
            }
        }
    }
}
