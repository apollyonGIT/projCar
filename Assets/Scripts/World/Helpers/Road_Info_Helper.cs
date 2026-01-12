using UnityEngine;
using World.Environments.Roads;
using World.Environments;
using Foundations;

namespace World.Helpers
{
    public class Road_Info_Helper
    {
        static RoadMgr m_roadMgr;
        static RoadMgr roadMgr
        {
            get
            {
                if (m_roadMgr == null)
                {
                    Mission.instance.try_get_mgr(Commons.Config.EnvironmentMgr_Name, out EnvironmentMgr emgr);
                    m_roadMgr = emgr.roadMgr;
                }
                return m_roadMgr;
            }
        }

        //==================================================================================================

        /// <summary>
        /// 获取高度
        /// </summary>
        public static float try_get_altitude(float x, int road_type = 0)
        {
            if (try_get_altitude(x, out var y, road_type))
                return y;

            return 0;
        }


        /// <summary>
        /// 获取高度
        /// </summary>
        public static bool try_get_altitude(float x, out float y, int road_type = 0)
        {
            y = 0;

            if (roadMgr == null)
                return false;

            y = roadMgr.road_height(x, road_type);
            return true;
        }


        /// <summary>
        /// 获取斜率
        /// </summary>
        public static bool try_get_slope(float x, out float slope, int road_type = 0)
        {
            slope = 0;

            if (roadMgr == null)
                return false;

            slope = roadMgr.road_slope(x, road_type);
            return true;
        }


        /// <summary>
        /// 获取路面倾斜弧度
        /// </summary>
        public static bool try_get_leap_rad(float x, out float rad, int road_type = 0)
        {
            rad = 0;
            if (try_get_slope(x, out float slope, road_type))
            {
                rad = Mathf.Atan(slope);
                return true;
            }

            return false;
        }


        /// <summary>
        /// 获取路面凹凸（二阶导）
        /// </summary>
        public static bool try_get_concavity(float x, out float value, int road_type = 0)
        {
            value = 0;

            if (roadMgr == null)
                return false;

            value = roadMgr.road_bump(x, road_type);
            return true;
        }


        /// <summary>
        /// 获取路面曲率半径
        /// </summary>
        public static bool try_get_ground_p(float x, out float value, int road_type = 0)
        {
            value = 0;

            if (roadMgr == null)
                return false;

            value = roadMgr.road_radius(x, road_type);
            return true;
        }


        /// <summary>
        /// 获取路面是否隐藏
        /// </summary>
        public static bool try_get_road_hidden(float x, int road_type, out bool value)
        {
            value = default;
            if (roadMgr == null || x == 0) return false;

            value = roadMgr.road_hidden(x, road_type);
            return true;
        }


        public static void @reset()
        {
            m_roadMgr = null;
        }
    }
}

