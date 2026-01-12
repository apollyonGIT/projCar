

using UnityEngine;

[ExecuteInEditMode]
public class ControlPoint : Point
{
    private Vector3 current_position;

    public RoadPoint ownerP;

    private void DrawControlLine()
    {
        Debug.DrawLine(transform.position,ownerP.transform.position,Color.black);
    }
    private void Update()
    {

        DrawControlLine();

        if (current_position == transform.position)
        {
            return;
        }
        else
        {
            ControlPoint otherP = ownerP.leftP == this ? ownerP.rightP : ownerP.leftP;

            var p1 = transform.position;
            var p2 = ownerP.transform.position;

            var otherY = p1.y + (p2.y - p1.y) / (p2.x - p1.x) * (otherP.transform.position.x - p1.x);

            var p = otherP.transform.position;
            otherP.transform.position = new Vector3(p.x, otherY, 0f);

            current_position = transform.position;
        }
    }
}