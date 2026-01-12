using Commons;
using System;
using UnityEngine;

namespace World.VFXs
{
    public class VFXView :MonoBehaviour
    {
        public VFX data;
        public VFXMgr owner;
        public void Init(VFX v, VFXMgr owner)
        {
            data = v;
            this.owner = owner;

            transform.position = new Vector3(v.pos.x, v.pos.y, 10);
        }


        // 用于每帧跟车移动的特效
        public void UpdatePos()
        {
            var cv = WorldContext.instance.caravan_pos;

            #region 敌人小车销毁后，特效处理(临时)
            if (data.kp == null)
            {
                ref var duration = ref data.duration;
                duration = Mathf.Min(duration, 600);
                
                duration--;
                return;
            }
            #endregion

            transform.position = data.kp.position;
        }
    }
}
