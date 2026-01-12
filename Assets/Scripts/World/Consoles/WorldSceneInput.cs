using Commons;
using System.Linq;
using UnityEngine;

namespace World
{
    public class WorldSceneInput : MonoBehaviour
    {

        //==================================================================================================

        public void OnOpen_console()
        {
            var ctx = WorldContext.instance;
            var root = WorldSceneRoot.instance;
            var ipf = root.console_IPF;

            var state = ipf.gameObject.activeSelf;

            //打开
            if (!state)
            {
                ipf.text = "";
                ipf.Select();
                ipf.ActivateInputField();
                ipf.transform.SetAsLastSibling();
            }
            else //关闭
            {
                var code = ipf.text;
                Console_Code_Helper.try_do_code(code, typeof(World_Console_Code));

                if (code.Any()) 
                    Console_Code_Helper.record(code);
            }

            ipf.gameObject.SetActive(!state);
        }


        public void OnPageup_console()
        {
            var ipf = WorldSceneRoot.instance.console_IPF;
            var state = ipf.gameObject.activeSelf;
            if (!state) return;

            ipf.text = Console_Code_Helper.redo(ipf.text);
        }


        public void OnPagedown_console()
        {
            var ipf = WorldSceneRoot.instance.console_IPF;
            var state = ipf.gameObject.activeSelf;
            if (!state) return;

            ipf.text = Console_Code_Helper.undo(ipf.text);
        }


        public void OnDelete_console()
        {
            var ipf = WorldSceneRoot.instance.console_IPF;
            var state = ipf.gameObject.activeSelf;
            if (!state) return;

            if (ipf.text.Any())
                Console_Code_Helper.record(ipf.text);

            ipf.text = "";
        }
    }
}

