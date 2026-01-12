using Foundations;
using UnityEngine;
using World.Enemys;

public class TitanBuff: MonoBehaviour
{
    public float size = 2f;

    public void ChangeScale()
    {
        Mission.instance.try_get_mgr("EnemyMgr", out EnemyMgr emgr); 
        foreach(var e in emgr.cells)
        {
            //e.change_scale(size);
        }
    }
}

