using UnityEngine;
public class Point : MonoBehaviour
{
    protected Vector2 m_position;
}

[ExecuteInEditMode]
public class RoadPoint : Point
{
    public ControlPoint leftP,rightP;
}
