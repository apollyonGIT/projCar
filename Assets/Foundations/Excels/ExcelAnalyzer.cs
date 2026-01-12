using Addrs;
using Foundations.Binarys;
using Foundations.Refs;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Foundations.Excels
{
    public class ExcelAnalyzer
    {
        public static object init(string record_class_name)
        {
            object records = default;

            var records_fi = Assembly.Load("AutoCodes").GetType($"AutoCodes.{record_class_name}s").GetField("m_records", BindingFlags.NonPublic | BindingFlags.Static);
            if (records_fi.GetValue(null) != null)
            {
                records = records_fi.GetValue(null);
                return records;
            }

            Addressable_Utility.try_load_asset(record_class_name, out AssetRef asset_ref);
            var binary_asset = (BinaryAsset)(asset_ref.asset);
            var data = (Dictionary<string, List<object>>)EX_Utility.byte2object(binary_asset.bytes);

            var records_type = records_fi.FieldType;
            records = Activator.CreateInstance(records_type);

            var record_type = Assembly.Load("AutoCodes").GetType($"AutoCodes.{record_class_name}");
            foreach (var (key_str, record) in data)
            {
                var entity = Activator.CreateInstance(record_type);
                var fis = entity.GetType().GetFields();

                for (int i = 0; i < fis.Length; i++)
                {
                    if (fis[i].Name == "diy_obj") continue;

                    var fi_value = fis[i].GetValue(entity);
                    fi_value ??= Activator.CreateInstance(fis[i].FieldType);

                    object cell = default;
                    try
                    {
                        cell = Convert.ChangeType(record[i], fis[i].FieldType);
                    }
                    catch
                    {
                        cell = FE_Mapping.ChangeType(record[i], fis[i].FieldType);
                    }

                    fis[i].SetValue(entity, cell);
                }

                records.GetType().GetMethod("Add").Invoke(records, new object[] { key_str, entity });
            }

            records_fi.SetValue(null, records);

            return records;
        }


        public static bool try_get_value<T>(string record_class_name, string key, out T record)
        {
            record = Activator.CreateInstance<T>();

            var records = init(record_class_name);
            if (records == null) return false;

            var objs = new object[2];
            objs[0] = key;
            objs[1] = record;

            var ret = (bool)records.GetType().GetMethod("TryGetValue").Invoke(records, objs);
            record = (T)objs[1];

            return ret;
        }
    }
}

