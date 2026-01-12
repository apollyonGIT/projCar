using System;
using System.Collections.Generic;
using World.Enemy_Cars;
using World.Enemys;

namespace World.Helpers
{
    public class Enemy_Init_Helper
    {
        public static Enemy init_enemy_instance(uint id)
        {
            AutoCodes.monsters.TryGetValue($"{id}", out var r_monster);
            var sub_table_name = r_monster.sub_table_name;

            if (string.IsNullOrEmpty(sub_table_name)) return new Enemy(id);

            Enemy_Init_Mapping.target_class_dic.TryGetValue(sub_table_name, out var enemy_type);
            return (Enemy)Activator.CreateInstance(enemy_type, new object[] { id });
        }
    }


    public class Enemy_Init_Mapping
    {
        public static Dictionary<string, Type> target_class_dic = new()
        {
            { "sub_monster_car", typeof(Enemy_Car) }
        };
    }        
}

