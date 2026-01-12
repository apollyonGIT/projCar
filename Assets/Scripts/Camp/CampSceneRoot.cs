using Foundations;

namespace Camp
{
    public class CampSceneRoot : SceneRoot<CampSceneRoot>
    {
        public Camp.Coins.CoinArea coinArea;

        //==================================================================================================

        protected override void on_init()
        {
            base.on_init();

            coinArea.call();
        }


        protected override void on_fini()
        {
            base.on_fini();
        }
    }
}

