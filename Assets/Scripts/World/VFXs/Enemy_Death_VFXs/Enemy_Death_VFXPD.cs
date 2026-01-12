using Addrs;
using Foundations;
using Foundations.Tickers;
using UnityEngine;

namespace World.VFXs.Enemy_Death_VFXs
{
    public class Enemy_Death_VFXPD : Producer
    {
        public override IMgr imgr => mgr;
        Enemy_Death_VFXMgr mgr;

        //==================================================================================================

        public override void init(int priority)
        {
            mgr = new("Enemy_Death_VFXMgr", priority);
            mgr.pd = this;
        }


        public override void call()
        {
        }


        public void create_cell(params object[] prms)
        {
            var res_name = (string)prms[0];
            var pos = (Vector2)prms[1];
            var duration = (int)prms[2];

            Addressable_Utility.try_load_asset(res_name, out Enemy_Death_VFXView model);
            var view = Instantiate(model, transform);

            Enemy_Death_VFX cell = new(pos);
            cell.add_view(view);

            mgr.add_cell(cell);

            Request_Helper.delay_do("destroy_enemy_death_vfx", duration, (_) => { mgr.fini_cells.AddLast(cell); });
        }
    }
}