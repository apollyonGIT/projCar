using Commons;
using System.Collections.Generic;
using UnityEngine;

namespace World.VFXs.Damage_PopUps
{
    public class Damage_PopUp_Mono : MonoBehaviour
    {
        public List<SpriteRenderer> dmg_list = new List<SpriteRenderer>();
        public List<Sprite> normal_textures = new List<Sprite>();
        public List<Sprite> heal_textures = new List<Sprite>();

        public GameObject heal_flag;

        internal int exist_delta;
        bool is_init;

        internal Dmg_Data dmg_data;

        internal IDamage_PopUp_Mover mover;

        Dictionary<string, object> m_diy_fields = new();

        //==================================================================================================

        public void init(params object[] prms)
        {
            dmg_data = (Dmg_Data)prms[0];

            var content = $"{dmg_data.dmg}";
            var pos = (Vector2)prms[1];

            SetDmgSprites(dmg_data);

            transform.position = pos;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);

            if (dmg_data.is_critical)
            {
                transform.localScale *= 1.5f;
                mover = Damage_Critical_PopUp_Mover.instance;
            }

            else
            {
                mover = Damage_PopUp_Mover.instance;
            }

            set_color(dmg_data.diy_dmg_str);
            mover.init(this);

            //治疗
            if (dmg_data.is_heal)
            {
                //heal_flag.SetActive(true);

                foreach (var spr in dmg_list)
                {
                    spr.color = Color.white;
                }
            }
                

            is_init = true;
        }

        private void set_color(string dmg_type)
        {
            Color spr_color = Config.current.normal_scene_color; // Ensure spr_color is initialized with a default value
            switch (dmg_type)
            {
                case "sharp":
                    break;
                case "blunt":
                    spr_color = Config.current.blunt_color;
                    break;
                case "fire":
                    spr_color = Config.current.fire_color;
                    break;
                case "acid":
                    spr_color = Config.current.acid_color;
                    break;
                case "wrap":
                    spr_color = Config.current.wrap_color;
                    break;
                case "flash":
                    spr_color = Config.current.flash_color;
                    break;
                default:
                    spr_color = Config.current.normal_scene_color;
                    break;
            }

            foreach (var spr in dmg_list)
            {
                spr.color = spr_color;
            }
        }

        private void SetDmgSprites(Dmg_Data dmg_data)
        {
            var dmg = dmg_data.dmg;
            var is_critical = dmg_data.is_critical;
            var is_heal = dmg_data.is_heal;

            if (is_heal)
                dmg = Mathf.RoundToInt(dmg_data.heal_power);

            if (dmg != 0 && is_critical)
            {
                dmg_list[9].gameObject.SetActive(true);
            }
            else
            {
                dmg_list[9].gameObject.SetActive(false);
            }

            int end_num = 0;
            for (int i = 0; i < 9; i++)
            {
                int temp;

                if (dmg < Mathf.Pow(10, i))
                {
                    dmg_list[i].gameObject.SetActive(false);
                    continue;
                }

                temp = (int)(dmg / Mathf.Pow(10, i));
                temp %= 10;

                if (is_heal)
                    dmg_list[i].sprite = heal_textures[temp];
                else
                    dmg_list[i].sprite = normal_textures[temp];
                
                dmg_list[i].gameObject.SetActive(true);

                end_num = i;
            }

            if (is_heal)
            {
                heal_flag.transform.localPosition = dmg_list[end_num].transform.localPosition + new Vector3(-0.15f, 0, 0);
                heal_flag.SetActive(true);
            }
        }

        private void Update()
        {
            if (!is_init) return;

            if (exist_delta <= 0)
            {
                DestroyImmediate(gameObject);
                return;
            }

            exist_delta--;

            mover.move(this);
        }


        public void destroy()
        {
            DestroyImmediate(gameObject);
        }


        public bool try_query_field<T>(string field_name, out T value)
        {
            value = default;

            if (!m_diy_fields.TryGetValue(field_name, out var obj))
                return false;

            if (obj is not T) return false;

            value = (T)obj;
            return true;
        }


        public void upd_field<T>(string field_name, T value)
        {
            if (!m_diy_fields.TryGetValue(field_name, out var obj))
            {
                m_diy_fields.Add(field_name, value);
                return;
            }

            if (obj is not T)
            {
                Debug.LogError($"伤害跳字Mono，试图更新的字段类型错误，字段名:{field_name}");
                return;
            }

            m_diy_fields[field_name] = value;
        }


        public void del_field(string field_name)
        {
            if (!m_diy_fields.TryGetValue(field_name, out var obj)) return;

            m_diy_fields.Remove(field_name);
        }
    }
}

