using Foundations;

namespace World.Cubicles
{
    public class CubiclePD : Producer
    {
        public override IMgr imgr => mgr;
        CubiclesMgr mgr;

        public override void init(int priority)
        {
            mgr = new CubiclesMgr("CubiclesMgr", priority);
        }

        public override void call()
        {

        }
    }
}