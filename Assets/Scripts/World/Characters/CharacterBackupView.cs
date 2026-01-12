using System.Collections.Generic;
using UnityEngine;

namespace World.Characters
{
    public class CharacterBackupView : MonoBehaviour
    {
        public CharacterBackUp cbu;
        public Transform content;
        public CharacterView prefab;
        public List<CharacterView> characters_view = new List<CharacterView>();

        public void UpdateView()
        {
            foreach(var cv in characters_view)
            {
                Destroy(cv.gameObject);
            }

            characters_view.Clear();
            foreach (var c in cbu.backup_characters)
            {
                var cv = Instantiate(prefab, content, false);
                cv.init(c);
                cv.gameObject.SetActive(true);
            }
        }
        
        public void Init(CharacterBackUp owner)
        {
            cbu = owner;

            foreach (var c in cbu.backup_characters)
            {
                var cv = Instantiate(prefab, content, false);
                cv.init(c);
                cv.gameObject.SetActive(true);
            }
        }
    }
}
