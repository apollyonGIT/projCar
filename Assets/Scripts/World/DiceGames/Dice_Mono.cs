using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace World.DiceGames
{
    public class Dice_Mono : MonoBehaviour, IPointerClickHandler
    {
        public Image painting;

        internal int painting_num;

        internal bool is_selected;
        public GameObject selected;

        Dice_Controller m_controller;

        //==================================================================================================

        public void init(Dice_Controller controller)
        {
            m_controller = controller;
        }


        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            is_selected = !is_selected;

            m_controller.check();
        }


        void Update()
        {
            selected.SetActive(is_selected);
        }
    }
}

