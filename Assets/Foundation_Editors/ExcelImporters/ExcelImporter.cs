using UnityEditor.AssetImporters;
using UnityEngine;
using Foundations.Binarys;
using ExcelDataReader;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Foundations.Excels;
using System;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Reflection;
using Foundations;

namespace Foundation_Editors.ExcelImporters
{
    [ScriptedImporter(1, "xlsx", AllowCaching = false)]
    public class ExcelImporter : ScriptedImporter
    {

        //==================================================================================================

        enum EN_row_type
        {
            none,
            field,
            type,
            desc,
            data
        }


        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = ScriptableObject.CreateInstance<ExcelFileAsset>();
            ctx.AddObjectToAsset("_", asset, FE_Res.instance.excelAssetIcon);
            ctx.SetMainObject(asset);

            List<BinaryAsset> _binarys = new();

            IsFileInUse(ctx.assetPath);

            try
            {
                using (var stream = File.Open(ctx.assetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            //初始化
                            var sheet_name = reader.Name;
                            //规则：-开头的sheet代表不导入
                            if (sheet_name.First() == '-') continue; 

                            int row = -1;
                            var row_type = EN_row_type.none;

                            List<string> fields = new();
                            List<string> type_strs = new();
                            List<int> masks = new();
                            List<int> keys_pos = new();

                            Dictionary<string, List<object>> datas = new();

                            while (reader.Read())
                            {
                                row++;

                                List<object> record = new();
                                List<object> keys = new();

                                for (int column = 0; column < reader.FieldCount; column++)
                                {
                                    if (masks.Any() && masks.Contains(column)) continue;

                                    var data = reader.GetValue(column);
                                    var data_str = data?.ToString();

                                    if (row_type == EN_row_type.none)
                                    {
                                        if (data == null) continue;

                                        //枚举行
                                        if (data_str.First().Equals('#'))
                                        {
                                            continue;
                                        }
                                    }

                                    if (column == 0 && row_type != EN_row_type.data)
                                    {
                                        row_type = (EN_row_type)((int)row_type + 1);
                                    }

                                    if (row_type == EN_row_type.field)
                                    {
                                        if (data == null)
                                        {
                                            masks.Add(column);
                                            continue;
                                        }

                                        if (data_str.Contains('.'))
                                            data_str = data_str.Replace('.', '_');

                                        fields.Add(data_str);
                                    }

                                    if (row_type == EN_row_type.type)
                                    {
                                        var type_str = data_str;
                                        if (data_str.Contains("key"))
                                        {
                                            keys_pos.Add(column);
                                            type_str = data_str.Split(' ')[0];
                                        }

                                        type_strs.Add(type_str);
                                    }

                                    if (row_type == EN_row_type.desc) break;

                                    //组装record
                                    if (row_type == EN_row_type.data)
                                    {
                                        record.Add(data);
                                    }

                                    if (keys_pos.Contains(column))
                                    {
                                        keys.Add(data);
                                    }
                                }

                                if (!record.Any() || record[0] == null) continue;

                                //组装key列
                                var key_str = "";
                                foreach (var key in keys)
                                {
                                    var temp_str = key.ToString();
                                    if (!key_str.Any())
                                    {
                                        key_str = temp_str;
                                    }
                                    else
                                    {
                                        key_str = $"{key_str},{temp_str}";
                                    }
                                }

                                datas.Add(key_str, record);
                            }

                            //组装binaryAsset
                            var binary = ScriptableObject.CreateInstance<BinaryAsset>();
                            binary.name = sheet_name;
                            binary.bytes = EX_Utility.object2byte(datas);

                            _binarys.Add(binary);

                            ctx.AddObjectToAsset(sheet_name, binary, FE_Res.instance.binaryAssetIcon);

                            //录入
                            binary.fields = fields.ToArray();
                            binary.type_strs = type_strs.ToArray();
                        }
                        while (reader.NextResult());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return;
            }

            asset.binarys = _binarys.ToArray();
        }


        public static void create_autocode(string sheet_name, string[] fields, string[] type_strs)
        {
            var target_code = Assembly.Load("AutoCodes").GetType($"AutoCodes.{sheet_name}s");
            if (target_code != null)
            {
                target_code.GetField("m_records", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
                
            }

            //初始化
            CodeCompileUnit unit = new();
            CodeNamespace _namespace = new("AutoCodes");

            #region 容器类，类名与sheet一致
            CodeTypeDeclaration container_class = new(sheet_name)
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public
            };

            var sum = fields.Length;
            for (int i = 0; i < sum; i++)
            {
                FE_Mapping.type_mapping.TryGetValue(type_strs[i], out var type);

                CodeMemberField member = new(type, fields[i])
                {
                    Attributes = MemberAttributes.Public
                };

                if (type == typeof(string))
                    member.InitExpression = new CodePrimitiveExpression(string.Empty);

                container_class.Members.Add(member);
            }

            //成员diy_obj
            {
                CodeMemberField m_diy_obj = new("System.Object", "diy_obj")
                {
                    Attributes = MemberAttributes.Public
                };

                container_class.Members.Add(m_diy_obj);
            }
            #endregion

            #region 集合类，类名为sheet名+s
            CodeTypeDeclaration ienumer_class = new($"{sheet_name}s")
            {
                IsClass = true,
                TypeAttributes = TypeAttributes.Public
            };

            //成员m_records
            {
                CodeMemberField m_records = new($"System.Collections.Generic.Dictionary<string, {sheet_name}>", "m_records")
                {
                    Attributes = MemberAttributes.Private | MemberAttributes.Static
                };

                ienumer_class.Members.Add(m_records);
            }

            //属性records
            {
                CodeMemberProperty property = new CodeMemberProperty()
                {
                    Name = "records",
                    Type = new CodeTypeReference($"System.Collections.Generic.Dictionary<string, {sheet_name}>"),
                    Attributes = MemberAttributes.Public | MemberAttributes.Static
                };

                CodeSnippetExpression get_records_call = new($"return (System.Collections.Generic.Dictionary<string, {sheet_name}>)Foundations.Excels.ExcelAnalyzer.init(\"{sheet_name}\")");
                property.GetStatements.Add(get_records_call);

                ienumer_class.Members.Add(property);
            }

            //函数TryGetValue
            {
                CodeMemberMethod method = new()
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Static,
                    Name = "TryGetValue",
                    ReturnType = new CodeTypeReference(typeof(bool))
                };

                CodeParameterDeclarationExpression prm_0 = new(typeof(string), "key");
                CodeParameterDeclarationExpression prm_1 = new($"{sheet_name}", "record")
                {
                    Direction = FieldDirection.Out
                };

                method.Parameters.Add(prm_0);
                method.Parameters.Add(prm_1);

                {
                    CodeSnippetExpression snippetExpression = new($"return Foundations.Excels.ExcelAnalyzer.try_get_value(\"{sheet_name}\", key, out record)");
                    method.Statements.Add(snippetExpression);
                }

                ienumer_class.Members.Add(method);
            }
            #endregion

            //代码设置
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new()
            {
                BracingStyle = "C",
                BlankLinesBetweenMembers = true
            };

            //装载
            _namespace.Types.Add(container_class);
            _namespace.Types.Add(ienumer_class);
            
            unit.Namespaces.Add(_namespace);

            //生成
            string outputPath = Application.dataPath + $"/AutoCodes/{sheet_name}.cs";

            using (StreamWriter sw = new(outputPath))
            {
                provider.GenerateCodeFromCompileUnit(unit, sw, options);
            }
        }


        public static bool IsFileInUse(string filePath)
        {
            try
            {
                using (File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    // 文件不被占用
                    return false;
                }
            }
            catch (IOException)
            {
                // 文件被占用
                return true;
            }
        }
    }
}

