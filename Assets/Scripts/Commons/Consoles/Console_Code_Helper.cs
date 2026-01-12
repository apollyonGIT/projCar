using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Commons.Attributes;

namespace Commons
{
    public class Console_Code_Helper
    {
        static Stack m_redo_stack = new();
        static Stack m_undo_stack = new();
        static string temp = "";

        //==================================================================================================

        public static bool try_do_code(string code, Type code_type)
        {
            MethodInfo mi;

            //带参数指令
            if (code.Contains('(') || code.Contains(')'))
            {
                var strs = code.Split('(');
                var mi_name = strs[0];

                mi = code_type.GetMethod(mi_name);
                if (mi == null)
                {
                    return false;
                }
                else
                {
                    var args = strs[1].TrimEnd(')').Split(',');

                    //规则：参数数量对不上，过滤
                    if (mi.GetParameters().Length != args.Length) return false;

                    mi.Invoke(null, args);
                    return true;
                }
            }

            //直接指令
            mi = code_type.GetMethod(code);
            if (mi == null)
            {
                return false;
            }
            else
            {
                mi.Invoke(null, null);
                return true;
            }
        }


        public static void help(Type code_type)
        {
            var ms = code_type.GetMethods();

            foreach (var mi in ms)
            {
                var attrs = mi.GetCustomAttributes(typeof(DetailAttribute), false);
                foreach (var attr in attrs)
                {
                    DetailAttribute e = (DetailAttribute)attr;
                    Debug.Log($"{e.detail}");
                }
            }
        }


        public static void cloud()
        {
            string filePath = @"\\192.168.50.2\SharedFiles\FT共享文件\控制台命令.txt";

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = filePath,
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(startInfo);
        }


        public static string redo(string current)
        {
            if (m_redo_stack.Count == 0)
                return current;

            if (current.Any())
                m_undo_stack.Push(current);

            return temp = (string)m_redo_stack.Pop();
        }


        public static string undo(string current)
        {
            if (m_undo_stack.Count == 0)
                return current;

            if (current.Any())
                m_redo_stack.Push(current);

            return temp = (string)m_undo_stack.Pop();
        }


        public static void record(string current)
        {
            if (current == "clear")
            {
                temp = "";
                m_redo_stack.Clear();
                m_undo_stack.Clear();
                return;
            }

            if (temp.Any())
            {
                m_redo_stack.Push(temp);
                temp = "";
            }

            foreach (var undo in m_undo_stack)
            {
                m_redo_stack.Push(undo);
            }
            m_undo_stack.Clear();
            
            if (m_redo_stack.Count == 0 || (m_redo_stack.Count > 0 && (string)m_redo_stack.Peek() != current))
                m_redo_stack.Push(current);
        }
    }
}

