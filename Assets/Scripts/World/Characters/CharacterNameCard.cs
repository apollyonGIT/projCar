using Addrs;
using AutoCodes;
using Commons;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace World.Characters
{
    public class CharacterNameCard : MonoBehaviour
    {
        public Image character_icon;
        public TextMeshProUGUI character_name;
        public Image character_ill;

        public Transform properties_content;
        public PropertyDetail property_prefab;

        public List<PropertyDetail> properties_list = new List<PropertyDetail>();    

        public void Init(Character c)
        {
            if (c != null)
            {
                character_icon.gameObject.SetActive(true);
                character_name.gameObject.SetActive(true);
                character_ill.gameObject.SetActive(true);

                Addressable_Utility.try_load_asset(c.desc.portrait_small, out Sprite icon);
                if(icon != null && character_icon!=null)
                    character_icon.sprite = icon;
                Addressable_Utility.try_load_asset(c.desc.illustration_small, out Sprite ill);
                if (ill != null && character_ill != null)
                    character_ill.sprite = ill;
                if (character_name != null)
                {
                    character_name.text = Localization_Utility.get_localization(c.desc.name);
                }

                foreach (var p in c.character_properties)
                {
                    propertiess.TryGetValue(p.desc.id.ToString(), out var p_record);
                    var pp = Instantiate(property_prefab, properties_content, false);
                    pp.Init(Localization_Utility.get_localization(p_record.name), Localization_Utility.get_localization(p_record.desc));
                    pp.gameObject.SetActive(true);

                    properties_list.Add(pp);
                }
            }
            else
            {
                character_icon.gameObject.SetActive(false);
                character_name.gameObject.SetActive(false);
                character_ill.gameObject.SetActive(false);

                foreach(var p in properties_list)
                {
                    Destroy(p.gameObject);
                }

                properties_list.Clear();
            }
        }
    }
}
