using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using World.Environments.Roads;


[ExecuteInEditMode]
public class RoadRoot : MonoBehaviour
{
    public SpriteRenderer road_sprite;

    public RoadData roadData;

    [Range(1,100)]
    public int division = 20;

    public Color curveColor = Color.green;

    public RoadPoint startP, endP;

    public List<RoadPoint> fixed_points = new();

    public void Clear()
    {
        fixed_points.Clear();
    }

    public void DrawCurve()
    {
        if (startP == null || endP == null)
            return;
        if (fixed_points.Count == 0)
        {
            CubicBezier(startP.transform.position, startP.rightP.transform.position, endP.leftP.transform.position, endP.transform.position);
        }
        else
        {
            CubicBezier(startP.transform.position, startP.rightP.transform.position, fixed_points[0].leftP.transform.position, fixed_points[0].transform.position);
            CubicBezier(fixed_points[fixed_points.Count - 1].transform.position, fixed_points[fixed_points.Count - 1].rightP.transform.position, endP.leftP.transform.position, endP.transform.position);

            for (int i = 0; i < fixed_points.Count - 1; i++)
            {
                CubicBezier(fixed_points[i].transform.position, fixed_points[i].rightP.transform.position, fixed_points[i + 1].leftP.transform.position, fixed_points[i + 1].transform.position);
            }
        }
    }

    private  void SortControlPoint()
    {
        if (startP == null || endP == null)
            return;
        if (fixed_points.Count == 0)
        {
            var sp = startP.rightP.transform.position;
            var ep = endP.leftP.transform.position;

            var x1 = startP.transform.position.x + (endP.transform.position.x - startP.transform.position.x)/3f;
            var x2 = startP.transform.position.x + (endP.transform.position.x - startP.transform.position.x) / 3f * 2f;

            startP.rightP.transform.position = new Vector3(x1,sp.y,0) ;
            endP.leftP.transform.position = new Vector3(x2, ep.y, 0);
        }
        else
        {
            var last = fixed_points.Count - 1;
            var sp = startP.rightP.transform.position;
            var ep = endP.leftP.transform.position;

            var x1 = startP.transform.position.x + (fixed_points[0].transform.position.x - startP.transform.position.x) / 3f;
            startP.rightP.transform.position = new Vector3(x1, startP.transform.position.y, 0);

            var x2 = fixed_points[last].transform.position.x + (endP.transform.position.x - fixed_points[last].transform.position.x)/3f * 2f;
            endP.leftP.transform.position = new Vector3(x2, endP.transform.position.y, 0);
            
            var x3 = startP.transform.position.x + (fixed_points[0].transform.position.x - startP.transform.position.x) / 3f * 2f;
            fixed_points[0].leftP.transform.position  = new Vector3(x3, fixed_points[0].leftP.transform.position.y, 0);

            var x4 = fixed_points[last].transform.position.x + (endP.transform.position.x - fixed_points[last].transform.position.x) / 3f;
            fixed_points[last].rightP.transform.position = new Vector3(x4, fixed_points[last].rightP.transform.position.y, 0);
            for (int i = 0; i < fixed_points.Count - 1; ++i)
            {
                var p1 = fixed_points[i].rightP.transform.position;
                var p2 = fixed_points[i + 1].leftP.transform.position;
                fixed_points[i].rightP.transform.position = new Vector3(fixed_points[i].transform.position.x + (fixed_points[i + 1].transform.position.x - fixed_points[i].transform.position.x) / 3f, p1.y, 0);
                fixed_points[i + 1].leftP.transform.position = new Vector3(fixed_points[i].transform.position.x + (fixed_points[i + 1].transform.position.x - fixed_points[i].transform.position.x) / 3f * 2f , p2.y, 0);
            }
        }
    }


    public void Update()
    {
        SortControlPoint();
        DrawCurve();
    }

    private void CubicBezier(Vector2 start,Vector2 mid1,Vector2 mid2,Vector2 end)
    {
        Vector2 pos1 = start;
        Vector2 pos2;
        int d = 100 / division;
        for (int t = 0; ; t += d)
        {
            if (t >= 100)
            {
                Debug.DrawLine(pos1, end, curveColor);
                return;
            }
            pos2 = start * Mathf.Pow(1 - t / 100f, 3) + 3 * mid1 * t / 100f * Mathf.Pow(1 - t / 100f, 2) + 3 * mid2 * t / 100f * t / 100f * (1 - t / 100f) + end * Mathf.Pow(t / 100f, 3);
            Debug.DrawLine(pos1, pos2, curveColor);
            pos1 = pos2;
        }
    }

    public void AddPoint(RoadPoint p)
    {
        fixed_points.Add(p);

        fixed_points.Sort((p1, p2) =>
        {
            return p1.transform.position.x < p2.transform.position.x ? -1 : 1;
        });
    }

#if  UNITY_EDITOR
    public void Save()
    {
        if (roadData == null)
        {
            var t = ScriptableObject.CreateInstance<RoadData>();
            AssetDatabase.CreateAsset(t, @"Assets\Resources_moved\road_data\" + "new data" + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            roadData = t;
        }
        roadData.points.Clear();
        roadData.road_sprite = null;
        roadData.road_sprite_position = Vector2.zero;

        roadData.points.Add(new PointData(){
            position = startP.transform.localPosition,
            left_position = startP.transform.localPosition + startP.leftP.transform.localPosition,      //从定点坐标系转到Points坐标系
            right_position  = startP.transform.localPosition + startP.rightP.transform.localPosition,
        });

        foreach(var p in fixed_points)
        {
            roadData.points.Add(new PointData()
            {
                position = p.transform.localPosition,
                left_position = p.transform.localPosition + p.leftP.transform.localPosition,
                right_position  =  p.transform.localPosition + p.rightP.transform.localPosition,
            });
        }


        roadData.points.Add(new PointData()
        {
            position = endP.transform.localPosition,
            left_position = endP.transform.localPosition + endP.leftP.transform.localPosition,
            right_position = endP.transform.localPosition + endP.rightP.transform.localPosition,
        });
        if (road_sprite != null)
        {
            roadData.road_sprite = road_sprite.sprite;
            roadData.road_sprite_position = road_sprite.transform.localPosition;
        }

        EditorUtility.SetDirty(roadData);
        AssetDatabase.SaveAssetIfDirty(roadData);
    }
#endif
}
