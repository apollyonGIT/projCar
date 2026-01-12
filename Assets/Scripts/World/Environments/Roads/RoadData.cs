using System.Collections.Generic;
using UnityEngine;

namespace World.Environments.Roads
{
    public class RoadData : ScriptableObject
    {
        public List<PointData> points = new List<PointData>();

        public Sprite road_sprite;

        public Vector2 road_sprite_position;

        public List<PointData> mul(float v)
        {
            List<PointData> new_points = new();
            foreach (var point in points)
            {
                new_points.Add(new PointData()
                {
                    position = point.position * v,
                    left_position = point.left_position * v,
                    right_position = point.right_position * v,
                });
            }

            return new_points;
        }
    }

    [System.Serializable]
    public class PointData
    {
        public Vector2 position, left_position, right_position;
    }
}
