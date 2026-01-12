using Commons;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using World.Audio;
using World.Enemys;

namespace World.Devices.DeviceUiViews.Attachments
{
    public class UiAim : MonoBehaviour, IDragHandler,IBeginDragHandler, IEndDragHandler,IPointerClickHandler
    {
        public bool is_dragging = false;
        public UiAimPanel aimPanel;
        public TextMeshProUGUI aim_text;

        private List<ITarget> search_targets = new();

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            aimPanel.duv.owner.ShowRadius(true);

            is_dragging = true;
            aim_text.gameObject.SetActive(true);

            Debug.Log("BeginDrag");
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(aimPanel.GetComponent<RectTransform>(),eventData.position,eventData.pressEventCamera,out var position);
            GetComponent<RectTransform>().anchoredPosition = position;

            RectTransformUtility.ScreenPointToWorldPointInRectangle(aimPanel.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var wp);

            var v2 = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, 10 - WorldSceneRoot.instance.mainCamera.transform.position.z));

            var st = BattleUtility.select_target_in_circle(v2, 1, WorldEnum.Faction.player);
            var device = aimPanel.duv.owner;
            if (st!= null && !search_targets.Contains(st) && search_targets.Count < device.desc.target_counts && !device.target_list.Contains(st) && !device.outrange_targets.ContainsKey(st))
            {
                search_targets.Add(st);
                (st as Enemy)?.PreSelect(true);
                AudioSystem.instance.PlayOneShot(Config.current.SE_target_locked);
            }

            aimPanel.duv.owner.ShowRadius(true);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            is_dragging = false;
            aim_text.gameObject.SetActive(false);
            transform.localPosition = Vector3.zero;

            aimPanel.duv.owner.ShowRadius(false);

            if (search_targets.Count != 0)
            {
                foreach(var search_target in search_targets)
                {
                    aimPanel.SetTarget(search_target);

                    (search_target as Enemy)?.PreSelect(false);
                    (search_target as Enemy)?.Select(true);
                }

                search_targets.Clear();
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
           /* if(eventData.button == PointerEventData.InputButton.Right)
            {
                if (search_targets.Count != 0)
                {
                    foreach (var search_target in search_targets)
                    {
                        (search_target as Enemy)?.PreSelect(false);
                    }

                    search_targets.Clear();
                }
            }
            else if(eventData.button == PointerEventData.InputButton.Left && !is_dragging)
            {
                var device = aimPanel.duv.owner;

                // 理论上可能要放到逻辑层好一些
                var targets = BattleUtility.select_all_target_in_circle(device.position, device.desc.basic_range.Item2, WorldEnum.Faction.player, (ITarget) =>
                {
                    return (ITarget.Position - device.position).magnitude >= device.desc.basic_range.Item1;
                });

                for(int i = 0; i < targets.Count; i++) {
                    var target = targets[i];
                    if (target != null && !device.target_list.Contains(target) && !device.outrange_targets.ContainsKey(target))
                    {
                        device.target_list.Add(target);
                        (target as Enemy)?.Select(true);
                    }
                    if((device.target_list.Count + device.outrange_targets.Count) >= device.desc.target_counts)
                    {
                        break;
                    }
                }
            }*/
        }

        private void Update()
        {
            if (is_dragging)
            {
                var device = aimPanel.duv.owner;
                var remain = device.desc.target_counts - search_targets.Count;
                if (remain > 0)
                {
                    aim_text.text = $"{remain}/{device.desc.target_counts}";
                }
                else
                {
                    aim_text.text = $"<color=red>{remain}</color>/{device.desc.target_counts}";
                }
            }
        }
    }
}
