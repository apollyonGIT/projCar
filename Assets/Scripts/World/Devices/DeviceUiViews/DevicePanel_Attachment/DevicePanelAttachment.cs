using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace World.Devices.DeviceUiViews
{
    public abstract class DevicePanelAttachment : MonoBehaviour
    {
        public virtual void Init(List<Action> action = null)
        {
            Init_Actions(action);
        }

        protected abstract void Init_Actions(List<Action> action);
    }

    // #############################################################################################################################

    public abstract class DevicePanelAttachment_Highlightable : DevicePanelAttachment
    {
        public Image img_indicator;
        public Sprite sprite_auto;
        public Sprite sprite_auto_interacting;
        public Sprite sprite_highlighted;
        public Sprite sprite_normal;
        public Sprite sprite_normal_interacting;

        private bool _player_interacting;
        private bool _auto;
        private bool _highlighted;

        // ===============================================================================

        private void update_img_indicator()
        {
            if (_auto)
            {
                if (_player_interacting && set_sprite(sprite_auto_interacting))
                    return;
                set_sprite(sprite_auto);
                return;
            }

            if (_player_interacting)
            {
                if (set_sprite(sprite_normal_interacting))
                    return;
                set_sprite(sprite_normal);
                return;
            }

            if (_highlighted)
            {
                set_sprite(sprite_highlighted);
                return;
            }

            set_sprite(sprite_normal);


            bool set_sprite(Sprite s)
            {
                if (s == null)
                    return false;
                img_indicator.sprite = s;
                return true;
            }
        }

        // ===============================================================================

        protected bool Player_Interacting
        {
            get { return _player_interacting; }
            set
            {
                if (_player_interacting == value)
                    return;
                _player_interacting = value;

                if (img_indicator == null)
                    return;
                update_img_indicator();
            }
        }

        // -------------------------------------------------------------------------------

        public bool Auto
        {
            get { return _auto; }
            set
            {
                if (_auto == value)
                    return;
                _auto = value;

                if (img_indicator == null)
                    return;
                update_img_indicator();
            }
        }

        public bool Highlighted
        {
            get { return _highlighted; }
            set
            {
                if (_highlighted == value)
                    return;
                _highlighted = value;

                if (img_indicator == null)
                    return;
                update_img_indicator();
            }
        }
    }

}
