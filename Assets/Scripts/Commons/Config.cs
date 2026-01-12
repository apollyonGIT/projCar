using System;
using System.Collections.Generic;
using UnityEngine;

namespace Commons
{
    [CreateAssetMenu(menuName = "GameConfig", fileName = "GameConfig")]
    public class Config : ScriptableObject
    {
        #region codes
        public static Config current;


        private void OnDisable()
        {
            if (ReferenceEquals(current, this))
            {
                current = null;
            }
        }
        #endregion

        [Range(1, 256)]
        public int pixelPerUnit = 100;

        public float scaled_pixel_per_unit { get; set; } = 100;

        public Vector2Int desiredResolution = new Vector2Int(1920, 1080);

        public float desiredPerspectiveFOV = 60;

        [Foldout("主界面")] public string main_menu_website_weibo = "https://weibo.com/u/1002143827";
        [Foldout("主界面")] public string main_menu_website_bilibili = "https://space.bilibili.com/423895";
        [Foldout("主界面")] public string main_menu_website_bug_report;

        [Foldout("场景切换/场景名UI")] public float scene_title_fade_out_time;
        [Foldout("场景切换/场景名UI")] public float scene_title_lasting_time;

        [Foldout("YH Backpack 背包")] public float per_overweight_slot_weight = 300;
        [Foldout("YH Backpack 背包")] public int basic_backpack_slot_num = 5;

        [Foldout("YH Base Camp 大本营")] public Vector2 camp_camera_size_range = new(3f, 25f);
        [Foldout("YH Base Camp 大本营")] public float camp_camera_zoom_speed = 0.2f;

        [Foldout("YH Camera 镜头运动")] public float travel_scene_camera_offset_x;  // FT沿用
        [Foldout("YH Camera 镜头运动")] public float cam_pos_offset_y;
        [Foldout("YH Camera 镜头运动")] public float cam_pos_offset_z;
        [Foldout("YH Camera 镜头运动")] public bool free_camera;

        [Foldout("YH Car Movement 车体运动")] public float gravity = -5f;
        [Foldout("YH Car Movement 车体运动")] public float acc_braking = -8f;
        [Foldout("YH Car Movement 车体运动-车身旋转")] public float caravan_rotate_angle_limit = 25f;  // 车身旋转，FT沿用
        [Foldout("YH Car Movement 车体运动-车身旋转")] public float caravan_rotation_damp_1 = 25f; // 车身旋转，FT沿用
        [Foldout("YH Car Movement 车体运动-车身旋转")] public float caravan_rotation_damp_2 = 4f; // 车身旋转，FT沿用
        [Foldout("YH Car Movement 车体运动-碰撞碾压")] public float car_body_collision_vx = 8f;
        [Foldout("YH Car Movement 车体运动-碰撞碾压")] public float car_collision_loss_vel = 5f;
        [Foldout("YH Car Movement 车体运动-碰撞碾压")] public float car_collision_loss_acc = 4f;

        [Foldout("YH Car Widget Blower 车体控件-风箱")] public int blower_stage_num = 6; //加速阶段数
        [Foldout("YH Car Widget Blower 车体控件-风箱")] public int blower_ticks_max = 168; //加速最大帧数
        [Foldout("YH Car Widget Blower 车体控件-风箱")] public float blower_low_lever_bonus = 5f; //低油门奖励
        [Foldout("YH Car Widget Engine Lever 车体控件-动力拉杆")] public float lever_up_speed = 1.2f;
        [Foldout("YH Car Widget Engine Lever 车体控件-动力拉杆")] public float lever_down_speed = -1f;
        [Foldout("YH Car Widget Engine Lever 车体控件-动力拉杆")] public float lever_move_delta = 0.015f;
        [Foldout("YH Car Widget Indicator 车体控件-车速状态灯")] public float car_vx_high_speed = 4.8f; //车体高速显示阈值
        [Foldout("YH Car Widget Indicator 车体控件-车速状态灯")] public float car_vx_low_speed = 1.8f; //车体低速显示阈值
        [Foldout("YH Car Widget Sprint 车体控件-冲刺按钮")] public int sprint_ticks = 480; //冲刺持续时间
        [Foldout("YH Car Widget Sprint 车体控件-冲刺按钮")] public int sprint_cd_ticks = 2400; //冲刺冷却时间
        [Foldout("YH Car Widget Sprint 车体控件-冲刺按钮")] public float sprint_vx_burst = 8f; //冲刺爆发速度
        [Foldout("YH Car Widget Sprint 车体控件-冲刺按钮")] public float sprint_vx_keep = 0.1f; //冲刺维持速度
        [Foldout("YH Car Widget Sprint 车体控件-冲刺按钮")] public float sprint_lever_min = 3.5f; //冲刺维持油门

        [Foldout("YH Device Debuff 设备异常")] public int device_debuff_immue_duration = 1;

        [Foldout("YH Device Debuff 设备异常-燃烧")] public float k_damage_ignite_factor = -4f;
        [Foldout("YH Device Debuff 设备异常-燃烧")] public float max_ignite = 7;
        [Foldout("YH Device Debuff 设备异常-燃烧")] public float extinguish_fire_value_per_s = 30f;

        [Foldout("YH Device Debuff 设备异常-机械故障")] public float k_stupor_rate_factor = -6f;
        [Foldout("YH Device Debuff 设备异常-机械故障")] public float stupor_normal_fix_area = 0.1f;
        [Foldout("YH Device Debuff 设备异常-机械故障")] public float stupor_kick_fix_rate = 0.5f;
        [Foldout("YH Device Debuff 设备异常-机械故障")] public float stupor_kick_damage = 5;

        [Foldout("YH Device Debuff 设备异常-腐蚀")] public float k_damage_acid_count_rate = 2f;
        [Foldout("YH Device Debuff 设备异常-腐蚀")] public float acid_ai_act_delay = 0.5f;

        [Foldout("YH Device Debuff 设备异常-缠绕")] public float vine_damage = 2f;
        [Foldout("YH Device Debuff 设备异常-缠绕")] public float k_damage_to_vine_rate = 0.25f;
        [Foldout("YH Device Debuff 设备异常-缠绕")] public float vine_min_gap = 0.35f;
        [Foldout("YH Device Debuff 设备异常-缠绕")] public float bind_ai_act_delay = 5f;

        [Foldout("YH Hurt 伤害")] public float k_hurt_sharp_default = 1f;
        [Foldout("YH Hurt 伤害")] public float k_hurt_blunt_default = 1f;
        [Foldout("YH Hurt 伤害")] public float k_hurt_fire_default = 1f;
        [Foldout("YH Hurt 伤害")] public float k_hurt_acid_default = 1f;
        [Foldout("YH Hurt 伤害")] public float k_hurt_wrap_default = 0.01f;
        [Foldout("YH Hurt 伤害")] public float k_hurt_flash_default = 0.01f;

        [Foldout("YH Encounter 奇遇事件")] public float trigger_length = 25;
        [Foldout("YH Encounter 奇遇事件")] public float notice_length_1 = 50f;
        [Foldout("YH Encounter 奇遇事件")] public float notice_length_2 = 10f;
        [Foldout("YH Encounter 奇遇事件")] public float event_btn_car_vx_limit = 0.1f;
        [Foldout("YH Encounter 奇遇事件")] public int event_btn_car_parking_ticks = 60;

        [Foldout("YH Enter Scene Non-Initial 进入非初始场景")] public float init_power_lever = 0.5f;
        [Foldout("YH Enter Scene Non-Initial 进入非初始场景")] public Vector2 init_car_velocity;
        [Foldout("YH Enter Scene Non-Initial 进入非初始场景")] public Vector2 init_car_position;
        [Foldout("YH Enter Scene Non-Initial 进入非初始场景")] public EN_config_caravan_move_type init_car_move_type = EN_config_caravan_move_type.Run;
        [Foldout("YH Enter Scene Non-Initial 进入非初始场景")] public EN_config_caravan_status_acc init_car_status_acc = EN_config_caravan_status_acc.driving;
        [Foldout("YH Enter Scene Non-Initial 进入非初始场景")] public EN_config_caravan_status_liftoff init_car_status_liftoff = EN_config_caravan_status_liftoff.ground;

        [Foldout("YH Hookrope 钩索")] public float rope_max_length;
        [Foldout("YH Hookrope 钩索")] public float rope_min_length;
        [Foldout("YH Hookrope 钩索")] public float rope_elasticity;

        [Foldout("YH Init 初始化")] public string first_load_scene = "InitScene"; //首次加载scene
        [Foldout("YH Init 初始化")] public uint caravan_id = 100101u;
        [Foldout("YH Init 初始化")] public uint init_wheel = 1210101u;
        [Foldout("YH Init 初始化")] public uint init_device_in_slot_top = 1320102u;
        [Foldout("YH Init 初始化")] public uint init_device_in_slot_front_top = 1320112u;
        [Foldout("YH Init 初始化")] public uint init_device_in_slot_back_top;
        [Foldout("YH Init 初始化")] public uint init_device_in_slot_front;
        [Foldout("YH Init 初始化")] public uint init_device_in_slot_back;
        [Foldout("YH Init 初始化")] public List<int> init_roles;
        [Serializable] public class InitBackPack { public int item_id; public int item_count; }
        [Foldout("YH Init 初始化")] public List<InitBackPack> init_backpack_items = new List<InitBackPack>();

        [Foldout("YH Loot 掉落物")] public float loot_pickup_distance = 0.1f;
        [Foldout("YH Loot 掉落物")] public float loot_attraction_power = 20f;
        [Foldout("YH Loot 掉落物")] public float coin_surface_bounce_coef = 1f;
        [Foldout("YH Loot 掉落物")] public float caravan_vel2_screen_bounce = 0.5f;

        [Foldout("YH Monster 怪物")] public float monster_grip = 35f;
        [Foldout("YH Monster 怪物")] public float fling_off_dis = 40f;
        [Foldout("YH Monster 怪物")] public float knock_air_threshold_factor = 1f;

        [Foldout("YH Monster 怪物_飞行")] public float breaking_rate = 0.3f;
        [Foldout("YH Monster 怪物_飞行")] public float breaking_force = 2f;

        [Foldout("YH Monster 怪物_飞行盘旋")] public float flyAround_radius_ratio = 0.15f;

        [Foldout("YH Monster 怪物_跳跃")] public int jump_iterate_time = 0;

        [Foldout("YH Monster 怪物_被击飞")] public float monster_land_stun_sec = 2f;

        [Foldout("YH Monster_Tide 兽潮")] public int time_pressure_min = 15;
        [Foldout("YH Monster_Tide 兽潮")] public int time_pressure_max = 60;
        [Foldout("YH Monster_Tide 兽潮")] public int pressure_kill_score = 40;

        [Foldout("YH Monster_Group 怪物组")] public float scene_distance_begin = 40f;
        [Foldout("YH Monster_Group 怪物组")] public float scene_distance_interval = 50f;
        [Foldout("YH Monster_Group 怪物组")] public float scene_distance_end = 100f;

        [Foldout("YH Monster_Tip 怪物标识表现")] public float dis_chase_warning_show = 25f; //追兵显示的最大距离

        [Foldout("YH Projectile 飞射物")] public float bullet_bounce_threshold_ekmin = 100f;
        [Foldout("YH Projectile 飞射物")] public float bullet_surface_bounce_coef = 0.5f;
        [Foldout("YH Projectile 飞射物")] public float bullet_bounce_threshold_vmax = 20f;
        [Foldout("YH Projectile 飞射物")] public float surface_friction = 0.3f;
        [Foldout("YH Projectile 飞射物")] public float arrow_penetration_loss = 10f;
        [Foldout("YH Projectile 飞射物")] public float bullet_penetration_loss = 6f;
        [Foldout("YH Projectile 飞射物")] public float bullet_enemy_bounce_coef = 0.2f;

        [Foldout("YH Repair 维修")] public int repairing_counts = 3;
        [Foldout("YH Repair 维修")] public int repairing_cd_ticks = 720;
        [Foldout("YH Repair 维修")] public float fix_device_job_effect = 0.03f;
        [Foldout("YH Repair 维修")] public float fix_caravan_job_effect = 0.01f;

        [Foldout("YH Role 角色-焦点位置")] public float role_damp_px = 13f;
        [Foldout("YH Role 角色-焦点位置")] public float role_damp_vx = 4f;
        [Foldout("YH Role 角色-焦点位置")] public float role_damp_py1 = 0f;
        [Foldout("YH Role 角色-焦点位置")] public float role_damp_py2 = 30f;
        [Foldout("YH Role 角色-焦点位置")] public float role_damp_vy = 4f;
        [Foldout("YH Role 角色-焦点位置")] public float role_offset_coef_x = -11f;
        [Foldout("YH Role 角色-焦点位置")] public float role_offset_coef_y = 21f;

        [Foldout("YH Role 角色-异常状态-昏迷")] public string role_state_coma_icon;
        [Foldout("YH Role 角色-异常状态-昏迷")] public string role_state_coma_cursor_hover;
        [Foldout("YH Role 角色-异常状态-昏迷")] public string role_state_coma_cursor_down;

        [Foldout("YH Safe Zone 安全区")] public float security_zone_distance = 100f;
        [Foldout("YH Safe Zone 安全区")] public uint coin_id = 10000;
        [Foldout("YH Safe Zone 安全区")] public float shop_relic_f5_cost_base = 1.5f;
        [Foldout("YH Safe Zone 安全区")] public float shop_relic_f5_cost_scene_parm = 0.3f;
        [Foldout("YH Safe Zone 安全区")] public float shop_repair_cost_base = 2.7f;
        [Foldout("YH Safe Zone 安全区")] public float shop_repair_cost_scene_parm = 0.25f;
        [Foldout("YH Safe Zone 安全区")] public int safearea_enter_transition_ticks = 75;
        [Foldout("YH Safe Zone 安全区")] public float shop_device_sell_coef = 0.5f;

        [Foldout("YH Save 存档")] public string save_00_path = @"D:\save_00.dat";

        [Foldout("YH Score 得分结算")] public int score_per_road = 666;
        [Foldout("YH Score 得分结算")] public int score_elite = 666;
        [Foldout("YH Score 得分结算")] public float score_k_lose = 0.6f;
        [Foldout("YH Score 得分结算")] public float score_k_speed = 1.6f;
        [Foldout("YH Score 得分结算")] public float score_max_speed = 20f;
        [Foldout("YH Score 得分结算")] public float score_k_damage = 1.6f;
        [Foldout("YH Score 得分结算")] public float score_k_broken = 1.6f;
        [Foldout("YH Score 得分结算")] public Vector2 k_loot_broken_rate = new Vector2(50, 80);

        [Foldout("YH SFX 音效")] public string SE_target_locked;
        [Foldout("YH SFX 音效")] public string SE_fix;
        [Foldout("YH SFX 音效")] public string SE_fix_fail;
        [Foldout("YH SFX 音效")] public string SE_monster_die;
        [Foldout("YH SFX 音效")] public string SE_encounter_radar;   //接近奇遇时的雷达提示声
        [Foldout("YH SFX 音效")] public string SE_pick_up_item;   //拾取物品的提示声
        [Foldout("YH SFX 音效")] public string SE_device_general_ka_da;   //通用：设备咔哒声
        [Foldout("YH SFX 音效")] public string SE_car_brake;   //刹车

        [Foldout("YH Text Color Dmg 字色-伤害")][SerializeField] public Color blunt_color = new Color(212, 149, 33);
        [Foldout("YH Text Color Dmg 字色-伤害")][SerializeField] public Color fire_color = new Color(210, 104, 23);
        [Foldout("YH Text Color Dmg 字色-伤害")][SerializeField] public Color acid_color = new Color(10, 196, 20);
        [Foldout("YH Text Color Dmg 字色-伤害")][SerializeField] public Color wrap_color = new Color(98, 106, 108);
        [Foldout("YH Text Color Dmg 字色-伤害")][SerializeField] public Color flash_color = new Color(143, 128, 178);
        [Foldout("YH Text Color Dmg 字色-伤害")][SerializeField] public Color normal_ui_color = new Color(49, 35, 5);
        [Foldout("YH Text Color Dmg 字色-伤害")][SerializeField] public Color normal_scene_color = new Color(246, 237, 209);

        [Foldout("YH VFX 特效-设备损毁")] public string devicedestroy_vfx;
        [Foldout("YH VFX 特效-设备损毁")] public string devicesmoke_vfx;

        [Foldout("YH Weather 天气")] public float weather_view_switch_duration = 4f;

        [Foldout("YH测试")] public bool is_load_devices = true;
        [Foldout("YH测试")] public bool is_load_enemys = true;
        [Foldout("YH测试")] public uint test_level_id = 4001;

        #region const
        //帧率
        public const int PHYSICS_TICKS_PER_SECOND = 120;
        public const float PHYSICS_TICK_DELTA_TIME = 1f / PHYSICS_TICKS_PER_SECOND;
        #endregion

        #region internal_setting
        //Mgr tick优先级
        public const int CaravanMgr_Priority = 1;
        public const int DeviceMgr_Priority = 2;
        public const int EnemyMgr_Priority = 3;
        public const int New_EnemyMgr_Priority = 3;
        public const int EnvironmentMgr_Priority = 4;
        public const int MapMgr_Priority = 5;
        public const int CardMgr_Priority = 6;
        public const int ProjectileMgr_Priority = 7;
        public const int ProjectileMgr_Enemy_Priority = 8;
        public const int SisterMgr_Priority = 9;
        public const int BrotherMgr_Priority = 10;
        public const int Base_SlotMgr_Priority = 11;
        public const int Vfx_Priority = 99;

        public const int BuffMgr_Priority = 0;

        public const int CoreMgr_Priority = 20;

        //Mgr注册name(tick)
        public const string PlayerMgr_Name = "Player";
        public const string AdventureMgr_Name = "Adventure";
        public const string ProjectileMgr_Name = "ProjectileMgr";
        public const string GarageMgr_Name = "GarageMgr";
        public const string CaravanMgr_Name = "CaravanMgr";
        public const string DeviceMgr_Name = "DeviceMgr";
        public const string CoreMgr_Name = "CoreMgr";
        public const string EnemyMgr_Name = "EnemyMgr";
        public const string New_EnemyMgr_Name = "New_EnemyMgr";
        public const string BuffMgr_Name = "BuffMgr";
        public const string WorkMgr_Name = "WorkMgr";
        public const string CharacterMgr_Name = "CharacterMgr";
        public const string RelicMgr_Name = "RelicMgr";

        public const string EnvironmentMgr_Name = "enviroment";
        public const string CardMgr_Name = "CardMgr";
        public const string WareHouseMgr_Name = "WareHouse";
        public const string MapMgr_Name = "MapMgr";
        public const string ProjectileMgr_Enemy_Name = "ProjectileMgr_Enemy";
        public const string SisterMgr_Name = "SisterMgr";
        public const string BrotherMgr_Name = "BrotherMgr";
        public const string BgmMgr_Name = "BgmMgr";
        public const string HoldingDeviceMgr_Name = "HoldingDeviceMgr";
        public const string Vfx_Name = "Vfx";

        //Mgr注册name(普通)
        public const string RewardHouse_Name = "RewardHouse";
        public const string Elite_Battle_TimerMgr_Name = "Elite_Battle_TimerMgr";
        public const string SelectLevelMgr_Name = "SelectLevel";

        #endregion
    }


    public enum EN_config_caravan_move_type
    {
        None,
        Run,
        Jump,
        Flying,
        Land
    }


    public enum EN_config_caravan_status_acc
    {
        driving,
        braking,
    }


    public enum EN_config_caravan_status_liftoff
    {
        ground,
        sky,
    }
}
