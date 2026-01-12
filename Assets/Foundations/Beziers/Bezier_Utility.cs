using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Foundations.Bezier
{
    public class Bezier_Utility
    {
        /// <summary>
        /// 计算贝塞尔点
        /// </summary>
        /// <param name="p0">开始点</param>
        /// <param name="p1">控制点</param>
        /// <param name="p2">结束点</param>
        /// <returns></returns>
        public static IEnumerable<Vector3> calc_points(int count, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)(count - 1);

                float u = 1 - t;
                float tt = t * t;
                float uu = u * u;
                Vector3 p = uu * p0;
                p += 2 * u * t * p1;
                p += tt * p2;

                yield return p;
            }
        }


        public static List<Vector3> calc_points_array(int count, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            List<Vector3> ret = new();

            foreach (var p in calc_points(count, p0, p1, p2))
            {
                ret.Add(p);
            }

            return ret;
        }
    }
}

