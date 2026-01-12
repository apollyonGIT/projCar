using UnityEngine;

namespace Camp
{
    public class CameraEdgeMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 10f;       // 摄像机移动速度
        public float edgeThreshold = 30f;  // 屏幕边缘触发距离（像素）
        public bool usePercentage = false; // 是否按屏幕比例计算边缘区域
        [Range(0f, 0.5f)] public float edgePercentage = 0.05f; // 屏幕边缘比例

        [Header("Boundary Settings")]
        public Vector2 minPosition; 
        public Vector2 maxPosition; 

        //==================================================================================================

        private void Update()
        {
            Vector2 mousePos = Input.mousePosition;
            Vector3 moveDirection = Vector3.zero;

            // 计算实际边缘阈值
            float actualThreshold = usePercentage ?
                edgePercentage * Screen.width :
                edgeThreshold;

            // 检测鼠标是否在屏幕边缘
            if (mousePos.x < actualThreshold)
            {
                moveDirection.x = -1; // 向左移动
            }
            else if (mousePos.x > Screen.width - actualThreshold)
            {
                moveDirection.x = 1; // 向右移动
            }

            if (mousePos.y < actualThreshold)
            {
                moveDirection.y = -1; // 向下移动
            }
            else if (mousePos.y > Screen.height - actualThreshold)
            {
                moveDirection.y = 1; // 向上移动
            }

            // 归一化方向并应用速度
            Vector3 movement = moveDirection.normalized * moveSpeed * Time.deltaTime;

            var pos = transform.position;
            pos += movement;
            transform.position = pos;
        }
    }
}
