using Foundations.MVVM;
using System.Collections.Generic;
using UnityEngine;

namespace World.Characters
{
    public class CharacterMgrView : MonoBehaviour, ICharacterMgrView
    {
        CharacterMgr owner;
        public Transform content;
        public List<CharacterView> characters_view = new List<CharacterView>();
        public CharacterView prefab;

        public CharacterBackupView backup_view;

        //==================================================================================================

        void IModelView<CharacterMgr>.attach(CharacterMgr owner)
        {
            this.owner = owner;

            foreach(var c in owner.characters)
            {
                var cv = Instantiate(prefab, content, false);
                cv.init(c);
                cv.gameObject.SetActive(true);
                characters_view.Add(cv);
            }

            if (backup_view != null)
            {
                backup_view.Init(owner.backup);
            }}


        void IModelView<CharacterMgr>.detach(CharacterMgr owner)
        {
            this.owner = null;

            Destroy(gameObject);
        }

        void ICharacterMgrView.notify_add_character(Character c)
        {
            foreach(var cv in characters_view)
            {
                if(cv.character == null)
                {
                    cv.init(c);
                    return;
                }
            }

            // 不应该走到这里
            var new_cv = Instantiate(prefab, content, false);
            new_cv.init(c);
            new_cv.gameObject.SetActive(true);
            characters_view.Add(new_cv);
        }

        void ICharacterMgrView.notify_on_tick()
        {
            foreach (var cv in characters_view)
            {
                cv.tick();
            }

            if (backup_view != null)
            {
                backup_view.UpdateView();
            }
        }

        void ICharacterMgrView.notify_select_character(Character c)
        {
            foreach (var cv in characters_view)
            {
                if (cv.character == c && cv.focus != null)
                {
                    cv.focus.gameObject.SetActive(true);
                }
            }
        }

        void ICharacterMgrView.notify_cancel_select_character()
        {
            foreach (var cv in characters_view)
            {
                if (cv.focus != null)
                {
                    cv.focus.gameObject.SetActive(false);
                }
            }
        }

        void ICharacterMgrView.notify_character_is_working(Character c, bool working)
        {
            foreach (var cv in characters_view)
                if (cv.character == c)
                    if (cv.working!=null && cv.working.gameObject.activeSelf != working)
                        cv.working.gameObject.SetActive(working);
        }

        void ICharacterMgrView.notify_remove_character(Character c)
        {
            for(int i = characters_view.Count - 1; i >= 0; i--)
            {
                if (characters_view[i].character == c)
                {
                    characters_view[i].init(null);
                    /*Destroy(characters_view[i].gameObject);
                    characters_view.RemoveAt(i);*/
                }
            }
        }

        void ICharacterMgrView.notify_update_character(Character c)
        {
            
        }

        void ICharacterMgrView.notify_backup_character()
        {
            backup_view.UpdateView();
        }
    }
}

