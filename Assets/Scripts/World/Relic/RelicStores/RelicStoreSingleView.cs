using Commons;
using Foundations;
using UnityEngine.EventSystems;
using World.Business;

namespace World.Relic.RelicStores
{
    public class RelicStoreSingleView : RelicView, IPointerClickHandler
    {
        public EliteRelicView elite_relic;


        public override void Init(Relic r)
        {
            base.Init(r);
            if (data == null)
                return;

            description_obj.gameObject.SetActive(true);
            if (followed)
                description_obj.transform.position = transform.position;
            if (relic_name != null)
                relic_name.text = Localization_Utility.get_localization(data.desc.name);
            if (relic_description != null)
                relic_description.text = Localization_Utility.get_localization(data.desc.description);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            Mission.instance.try_get_mgr(Commons.Config.RelicMgr_Name, out RelicMgr rmgr);
            rmgr.CreateRelicAndAdd(data.desc.id.ToString());

            elite_relic.SetSelect();
            elite_relic.gameObject.SetActive(false);

            //            Destroy(gameObject);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {

        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            
        }
    }
}
