using Foundations;
using Foundations.MVVM;
using Foundations.Tickers;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using World.Devices.Device_AI;

namespace World.Devices
{
    public interface IDeviceMgrView : IModelView<DeviceMgr>
    {
        void notify_install_device(string slot_name, Device device);
        void notify_remove_device(string slot_name);
        void notify_tick();
        void notify_tick1();
        void notify_select_device(Device device);
    }

    public class DeviceSlot
    {
        public string _name;
        public int width;           //槽位宽度
        public Device slot_device;

        public DeviceSlot(string slot_name, Device device, int width)
        {
            _name = slot_name;
            this.width = width;
            slot_device = device;
        }
    }

    public class DeviceMgr : Model<DeviceMgr, IDeviceMgrView>, IMgr
    {
        public DevicePD pd;
        string IMgr.name => m_mgr_name;
        readonly string m_mgr_name;

        int IMgr.priority => m_mgr_priority;
        readonly int m_mgr_priority;

        //==================================================================================================
        public List<DeviceSlot> slots_device = new();         //初始化就应该添加所有kp，保证后续不会进行remove/add 操作

        private static Dictionary<string, Func<Device>> devices_dic = new() {
            { "Shooter_Trebuchet_Old",() => new Shooter_Trebuchet_Outdated() },
            { "Shooter_Gyroscope",()=> new Shooter_Gyroscope()},
            { "BasicWheel" ,() => new BasicWheel() },
            { "Wheel_Track" ,() => new Wheel_Track() },
            { "BasicShield",() => new BasicShield()},
            { "War_Drum",() => new Unique_War_Drum()},
            { "Catching_Flower",() => new Unique_Catching_Flower()},
            { "NewBasicShooter",()=> new NewBasicShooter()},
            { "NewBasicMelee",()=>new NewBasicMelee()},
            { "NewBasicHook",()=> new NewBasicHook()},
            { "BasicRam",()=> new BasicRam()},
            { "EntropyIncreaseRam",()=> new EntropyIncreaseRam()},
            { "BlackHole",()=> new BlackHole()},
            { "Shield_Generator",()=> new Shield_Generator()},
            { "Unique_Cactus",()=> new Unique_Cactus()},
            { "LaserGun",()=> new LaserGun()},
            { "Rattle",()=> new Rattle()},
            { "JetWheel",()=> new JetWheel()},
            { "FireWheel",()=> new FireWheel()},
            { "Shooter_Quiver" ,()=> new Shooter_Quiver()},
            { "AnimSharpMelee_Click",()=> new AnimSharpMelee_Click()},
            { "Melee_Tentacle",()=> new Melee_Tentacle()},

            { "DScript_Shooter",()=> new DScript_Shooter()},
            { "DScript_Shooter_Gatlin",()=> new DScript_Shooter_Gatlin()},
            { "DScript_Shooter_Revolver",()=> new DScript_Shooter_Revolver_XY()},
            { "DScript_Shooter_Revolver_AMY",()=> new DScript_Shooter_Revolver()},
            { "DScript_Shooter_Shotgun",()=> new DScript_Shooter_Shotgun()},

            { "DScript_Melee",()=> new DScript_Melee()},
            { "DScript_Melee_Baton",()=> new DScript_Melee_Baton()},
            { "DScript_Melee_Blade",()=> new DScript_Melee_Blade()},
            { "DScript_Melee_Claymore",()=> new DScript_Melee_Claymore()},
            { "DScript_Melee_Charge",()=> new DScript_Melee_Charge()},
            { "DScript_Melee_Charge_CircularSaw",()=> new DScript_Melee_Charge_CircularSaw()},
            { "DScript_Melee_Charge_Rocket",()=> new DScript_Melee_Charge_Rocket()},
            { "DScript_Special_CarBomb",()=> new DScript_Special_CarBomb()},



            { "DScript_Ram",()=> new DScript_Ram()},



            #region WWD TEST 2025.Jul
            { "BasicMelee_Click",()=> new BasicMelee_Click()},   //2025.Jul.09
            { "BasicRam_Click",()=> new BasicRam_Click()},   //2025.Aug.05
            { "BasicShield_Click",()=> new BasicShield_Click()},   //2025.Aug.11
            { "Shooter_AutoGun_Pistol" ,()=> new Shooter_AutoGun_Pistol()}, //2025.Aug.20
            { "Shooter_AutoGun_Rifle" ,()=> new Shooter_AutoGun_Rifle()}, //2025.Jul.29
            { "Shooter_Bow",()=> new Shooter_Bow()}, //2025.Aug.07
            { "Shooter_Gatlin" ,()=> new Shooter_Gatlin()}, //2025.Jul.22
            { "Shooter_Huge_Ballista" ,()=> new Shooter_Huge_Ballista()}, //2025.Jul.16
            { "Shooter_Revolver" ,()=> new Shooter_Revolver()}, //2025.Jul.24
            { "Shooter_Rocket" ,()=> new Shooter_Rocket()}, //2025.Jul.28
            { "Shooter_Roll_Wood",()=> new Roll_Wood()},
            { "Shooter_Shotgun",()=> new Shooter_Shotgun()}, //2025.Aug.29
            { "Shooter_Trebuchet",()=> new Shooter_Trebuchet()}, //2025.Aug.08
            #endregion
        };
        public static Device create_device(string id)
        {
            if (devices_dic.ContainsKey(id))
            {
                return devices_dic[id]();
            }

            var type = Assembly.Load("World").GetType($"World.Devices.{id}");
            if (type != null)
            {
                return (Device)Activator.CreateInstance(type);
            }

            UnityEngine.Debug.LogWarning($"错误设备类型 {id}");
            return new Device();
        }
        public DeviceMgr(string name, int priority, params object[] args)
        {
            m_mgr_name = name;
            m_mgr_priority = priority;

            (this as IMgr).init(args);
        }


        /// <summary>
        /// 增加槽位名，宽度,和设备，一般只在初始化进行
        /// </summary>
        /// <param name="slot_name"></param>
        /// <param name="device"></param>
        public void AddDevice(string slot_name, Device device,int width =1)
        {
            slots_device.Add(new DeviceSlot(slot_name, device,width));
        }

        /// <summary>
        /// 安装设备 不增加槽位
        /// </summary>
        /// <param name="slot_name"></param>
        /// <param name="device"></param>
        public void InstallDevice(string slot_name, Device device)
        {
            slots_device.Find(sd=>sd._name == slot_name).slot_device = device;
            device.equip_time = 0;

            foreach (var view in views)
            {
                view.notify_install_device(slot_name, device);
            }
        }

        public void RemoveDevice(string slot_name)
        {
            slots_device.Find(sd => sd._name == slot_name).slot_device = null;

            foreach (var view in views)
            {
                view.notify_remove_device(slot_name);
            }
        }


        public Device GetDevice(string script_name)
        {
            return create_device(script_name);
        }

        void IMgr.init(params object[] args)
        {
            Mission.instance.attach_mgr(m_mgr_name, this);

            var ticker = Ticker.instance;
            ticker.add_tick(m_mgr_priority, m_mgr_name, tick);
            ticker.add_tick1(m_mgr_priority, m_mgr_name, tick1);
        }
        void IMgr.fini()
        {
            Mission.instance.detach_mgr(m_mgr_name);

            var ticker = Ticker.instance;
            ticker.remove_tick(m_mgr_name);
            ticker.remove_tick1(m_mgr_name);
        }
        void tick()
        {
            foreach (var slot in slots_device)
            {
                var device = slot.slot_device;
                var slot_name = slot._name;
                if (device == null)
                    continue;
                WorldContext.instance.caravan_slots.TryGetValue(slot_name, out var slot_t);
                if (slot_t != null)
                {
                    var v = new Vector2(slot_t.x, slot_t.y);
                    var angle = Vector2.SignedAngle(Vector2.right, WorldContext.instance.caravan_dir);
                    var new_v = Quaternion.AngleAxis(angle, Vector3.forward) * v;

                    var cp = WorldContext.instance.caravan_pos;
                    Debug.DrawLine(new Vector3(cp.x, cp.y, 10), new Vector3(cp.x + new_v.x, cp.y + new_v.y, 10), Color.blue);

                    device.position = WorldContext.instance.caravan_pos + new Vector2(new_v.x, new_v.y);
                }

                device.tick();
            }

            //设备不需要reset位置，因为车的位置已经Reset过了
            /*if (WorldContext.instance.is_need_reset)
            {
                foreach (var (_, device) in slots_device)
                {
                    if (device == null)
                        continue;
                    device.position -= new Vector2(WorldContext.instance.reset_dis, 0);
                }
            }*/

            foreach (var view in views)
            {
                view.notify_tick();
            }
        }
        void tick1()
        {
            foreach (var view in views)
            {
                view.notify_tick1();
            }
        }

        public void Init()
        {
            foreach (var slot in slots_device)
            {
                var slot_name = slot._name;
                var device = slot.slot_device;

                WorldContext.instance.caravan_slots.TryGetValue(slot_name, out var slot_t);
                if (slot_t != null && device != null)
                {
                    var v = new Vector2(slot_t.x, slot_t.y);
                    var angle = Vector2.SignedAngle(Vector2.right, WorldContext.instance.caravan_dir);
                    var new_v = Quaternion.AngleAxis(angle, Vector3.forward) * v;

                    var cp = WorldContext.instance.caravan_pos;

                    device.position = WorldContext.instance.caravan_pos + new Vector2(new_v.x, new_v.y);
                }
                device?.InitPos();
            }
        }

        public void SelectDevice(Device device)
        {
            foreach (var view in views)
            {
                view.notify_select_device(device);
            }
        }
    }
}