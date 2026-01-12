using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Foundations
{
    public class EX_Utility
    {
        public static byte[] object2byte(object obj)
        {
            if (obj == null)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }


        public static object byte2object(byte[] bytes)
        {
            if (bytes == null)
                return null;

            using (var memoryStream = new MemoryStream(bytes))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(memoryStream);
            }
        }


        /// <summary>
        /// 字典添加 - 覆盖
        /// </summary>
        public static void dic_cover_add<K, V>(ref Dictionary<K, V> dic, K k, V v)
        {
            if (dic.ContainsKey(k))
            {
                dic.Remove(k);
            }

            dic.Add(k, v);
        }


        /// <summary>
        /// 字典删除 - 安全型
        /// </summary>
        public static void dic_safe_remove<K, V>(ref Dictionary<K, V> dic, K k)
        {
            if (dic.ContainsKey(k))
            {
                dic.Remove(k);
            }
        }


        /// <summary>
        /// 字典取值 - 安全型
        /// </summary>
        public static V dic_safe_getValue<K, V>(ref Dictionary<K, V> dic, K k, V _default)
        {
            if (!dic.ContainsKey(k))
            {
                dic.Add(k, _default);
                return _default;
            }

            return dic[k];
        }

        /// <summary>
        /// 字符串截取 - 安全型
        /// </summary>
        public static string[] string_safe_split(string raw, string split_str)
        {
            if (raw.Contains(split_str))
            {
                return raw.Split(split_str);
            }

            return new string[] { raw };
        }


        /// <summary>
        /// 转化：弧度 -> 方向
        /// </summary>
        public static Vector2 convert_rad_to_dir(float rad)
        {
            return new Vector2(1, MathF.Tan(rad)).normalized;
        }


        /// <summary>
        /// LookRotation增强
        /// </summary>
        public static Quaternion look_rotation_from_left(Vector2 dir)
        {
            return Quaternion.LookRotation(Vector3.forward, new Vector2(-dir.y, dir.x));
        }

    }
}

