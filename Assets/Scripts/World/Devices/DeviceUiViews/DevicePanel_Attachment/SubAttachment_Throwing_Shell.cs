using UnityEngine;
using UnityEngine.UI;


namespace World.Devices.DeviceUiViews
{
    public class SubAttachment_Throwing_Shell : MonoBehaviour
    {
        private int countdown;
        private RectTransform rt;

        private Vector2 pos;
        private Vector2 vel;
        private float rotate_speed;
        private float rotate_angle;

        public void Init(Sprite sprite, int life, float throwing_force)
        {
            var img = GetComponent<Image>();
            img.sprite = sprite;
            countdown = life;
            rt = img.GetComponent<RectTransform>();
            //rt.anchoredPosition = init_pos;
            var vx = Random.Range(-1f, 1f);
            var vy = Random.Range(1f, 2f);
            vel = new Vector2(vx, vy) * throwing_force;
            rotate_speed = Random.Range(-0.1f, 0.1f) * throwing_force;
        }

        public void tick()
        {
            countdown--;

            vel += new Vector2(0, -0.1f); // Ä£ÄâÖØÁ¦
            pos += vel;
            rt.anchoredPosition = pos;
            rotate_angle += rotate_speed;
            rt.rotation = Quaternion.Euler(0, 0, rotate_angle);
        }

        public bool isDead()
        {
            return countdown <= 0;
        }
    }
}
