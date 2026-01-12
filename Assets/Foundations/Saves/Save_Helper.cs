using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Foundations.Saves
{
    public class Save_Helper
    {
        public static void save(string path, object obj)
        {
            FileStream stream = new(path, FileMode.Create);

            using BinaryWriter bw = new(stream);
            var buffer = EX_Utility.object2byte(obj);

            bw.Write(buffer);
        }


        public static void load<T>(string path, out T ret)
        {
            var buffer = File.ReadAllBytes(path);

            ret = (T)EX_Utility.byte2object(buffer);
        }
    }
}

