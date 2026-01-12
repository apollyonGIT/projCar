using Addrs;
using AutoCodes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace World.Characters
{
    public class SingleCharacterView : MonoBehaviour
    {
        public Character character;
        public GameObject character_image;
        public TextMeshProUGUI role_name;


        public Transform properties_content;
        public PropertyDetail property_prefab;

        public List<PropertyDetail> properties_detail = new List<PropertyDetail>();

        public void init(Character c)
        {

            for (int i = properties_detail.Count - 1; i >= 0; i--)
            {
                Destroy(properties_detail[i].gameObject);
            }
            
            properties_detail.Clear();

            character = c;

            Addressable_Utility.try_load_asset(c.desc.portrait_small, out Sprite s);
            if (s != null)
            {
                character_image.GetComponent<Image>().sprite = s;
            }

            if (role_name != null)
                role_name.text = Commons.Localization_Utility.get_localization(c.desc.name);

            if (property_prefab != null && properties_content!=null)
                foreach (var p in c.character_properties)
            {
                propertiess.TryGetValue(p.desc.id.ToString(), out var p_record);

                if (property_prefab == null || properties_content == null)
                    break;

                var pp = Instantiate(property_prefab, properties_content, false);
                pp.Init(Commons.Localization_Utility.get_localization(p_record.name), Commons.Localization_Utility.get_localization(p_record.desc));
                pp.gameObject.SetActive(true);

                properties_detail.Add(pp);
            }
        }
    }
}
