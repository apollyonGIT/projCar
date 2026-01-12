using Addrs;
using AutoCodes;
using Foundations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Characters.CharacterStates;
using World.Devices.DeviceUiViews;
using World.Helpers;

namespace World.Characters
{
    public interface IUiCharacter
    {
        void UiCancelCharacter();
        void UiSetCharacter(Character c);
    }

    public class CharacterView : MonoBehaviour,IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Character character;
        public CharacterImage character_image;

        public TextMeshProUGUI role_name;

        public GameObject focus, working;

        public SingleCharacterView single_info;

        public Transform properties_content;
        public PropertyDetail property_prefab;

        public GameObject drag_view;

        public bool can_drag = true;

        public CharacterNameCard namecard;

        public DeviceStationView cubicle;

        public CharacterBuffView buffview;

        public CharacterTalkView talkview;

        public void init(Character c)
        {
            character = c;
            Mission.instance.try_get_mgr(Commons.Config.CharacterMgr_Name, out CharacterMgr cmgr);
            character_image.init(cmgr);

            buffview?.Init(character);

            if (c == null)
            {
                character_image.gameObject.SetActive(false);

                return;
            }

            Addressable_Utility.try_load_asset(c.desc.portrait_small, out Sprite s);
            if (s != null)
            {
                character_image.gameObject.SetActive(true);
                character_image.GetComponent<Image>().sprite = s;
            }

            if (role_name != null)
                role_name.text = Commons.Localization_Utility.get_localization(c.desc.name);

            if (property_prefab != null && properties_content != null)
                foreach (var p in c.character_properties)
                {
                    propertiess.TryGetValue(p.desc.id.ToString(), out var p_record);

                    if (property_prefab == null || properties_content == null)
                        break;

                    var pp = Instantiate(property_prefab, properties_content, false);
                    pp.Init(Commons.Localization_Utility.get_localization(p_record.name), Commons.Localization_Utility.get_localization(p_record.desc));
                    pp.gameObject.SetActive(true);
                }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            Mission.instance.try_get_mgr(Commons.Config.CharacterMgr_Name, out CharacterMgr cmgr);
            if (eventData.button == PointerEventData.InputButton.Right && cmgr.select_character == character)
            {
                Cubicle_Helper.SetSelectCharacter(null);
            }

            if (namecard != null)
            {
                namecard.Init(character);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (character == null || single_info == null)
                return;

            single_info.init(character);
            single_info.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (single_info == null)
                return;
            single_info.gameObject.SetActive(false);
        }

        public void tick()
        {
            /*if(!character.is_working)
                character_image.tick();
            else
            {
                character_image.revert();
            }*/


            if (buffview != null)
                if (character != null && character.state != null)
                {
                    if (character.state.GetType() == typeof(Coma))
                    {
                        buffview.gameObject.SetActive(true);

                        if (talkview != null && !talkview.gameObject.activeSelf)
                        {
                            talkview.gameObject.SetActive(true);
                            if(character.desc.lines_coma != null)
                                talkview.init(character.desc.lines_coma);
                        }   
                    }

                    buffview.tick();
                }
                else
                {
                    buffview.gameObject.SetActive(false);
                    if (talkview != null)
                        talkview.gameObject.SetActive(false);
                }
        }


        private Canvas hoverCanvas;
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!can_drag || character == null||character.state != null)
                return;

            if (namecard != null)
            {
                namecard.Init(character);
            }

            Mission.instance.try_get_mgr(Commons.Config.CharacterMgr_Name, out CharacterMgr cmgr);
            Cubicle_Helper.SetSelectCharacter(character);

            hoverCanvas = drag_view.gameObject.GetComponent<Canvas>() ==null? drag_view.gameObject.AddComponent<Canvas>():drag_view.gameObject.GetComponent<Canvas>();
            hoverCanvas.overrideSorting = true;
            hoverCanvas.sortingOrder = 1000; // 确保最高层级
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!can_drag || character == null || character.state != null)
                return;
            var mousePosition = InputController.instance.GetScreenMousePosition();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(drag_view.transform.parent.transform as RectTransform, mousePosition, WorldSceneRoot.instance.uiCamera, out var pos);
            drag_view.GetComponent<RectTransform>().anchoredPosition = pos;
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!can_drag ||character == null)
                return;
            List<RaycastResult> results = new List<RaycastResult>();

            GraphicRaycaster graphicRaycaster = WorldSceneRoot.instance.uiRoot.GetComponent<GraphicRaycaster>();
            graphicRaycaster.Raycast(eventData, results);

            foreach (var result in results)
            {
                if (result.gameObject.TryGetComponent<IUiCharacter>(out var iuc))
                {
                    iuc.UiSetCharacter(character);
                }
            }

            if (hoverCanvas != null)
            {
                Destroy(hoverCanvas);
            }

            drag_view.transform.localPosition = Vector3.zero;
        }
    }
}
