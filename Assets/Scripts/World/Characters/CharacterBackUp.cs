using System.Collections.Generic;

namespace World.Characters
{
    public class CharacterBackUp
    {
        public List<Character> backup_characters = new List<Character>();

        public void AddCharacter(Character character)
        {
            if (character != null)
            {
                backup_characters.Add(character);
            }
        }

        public void RemoveCharacter(Character character)
        {
            if (character != null && backup_characters.Contains(character))
            {
                backup_characters.Remove(character);
            }
        }
    }
}
