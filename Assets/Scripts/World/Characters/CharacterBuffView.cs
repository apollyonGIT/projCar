using Addrs;
using Foundations.Refs;
using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Characters
{ 

    /// <summary>
    /// 因为没有其他异常 无法总结出共性，先特殊处理
    /// </summary>
    public class CharacterBuffView :MonoBehaviour,IPointerEnterHandler, IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,IPointerClickHandler
    {
        public Character owner;

        private bool change_cursor = false;

        public void Init(Character c)
        {
            owner = c;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(owner!=null && owner.state!=null)
                owner.state.trigger();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var path = Commons.Config.current.role_state_coma_cursor_down;
            Addressable_Utility.try_load_asset<AssetRef>(path, out var asset);
            if(asset!= null)
            {
                change_cursor = true;
                Cursor.SetCursor((Texture2D)asset.asset, Vector2.zero, CursorMode.Auto);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var path = Commons.Config.current.role_state_coma_cursor_hover;
            Addressable_Utility.try_load_asset<AssetRef>(path, out var asset);
            if (asset != null)
            {
                change_cursor = true;
                Cursor.SetCursor((Texture2D)asset.asset, Vector2.zero, CursorMode.Auto);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            change_cursor = false;
            Cursor.SetCursor(null,Vector2.zero, CursorMode.Auto);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var path = Commons.Config.current.role_state_coma_cursor_hover;
            Addressable_Utility.try_load_asset<AssetRef>(path, out var asset);
            if (asset != null)
            {
                change_cursor = true;
                Cursor.SetCursor((Texture2D)asset.asset, Vector2.zero, CursorMode.Auto);
            }
        }

        private void OnDisable()
        {
            if (change_cursor)
            {
                change_cursor = false;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }


        public void tick()
        {

        }
    }
}
