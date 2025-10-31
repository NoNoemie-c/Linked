using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways] [RequireComponent(typeof(CanvasRenderer))]
public sealed class lineGraphic : MaskableGraphic
{
    public List<Vector2> points {
        get => Points;

        set {
            Points = value;
            SetVerticesDirty();
        }
    }
    public List<Vector2> Points = new List<Vector2>();
    public bool loop;
    public float thickness;
    public AnimationCurve thickCurve = AnimationCurve.Constant(0, 1, 1);
    private float totalLength, length;
    public bool useWorldSpace;

    protected override void OnPopulateMesh(VertexHelper vh) {
        vh.Clear();

        if (points.Count < 2)
            return;

        for (int i = 0; i < points.Count - 1; i ++)
            totalLength += (points[i + 1] - points[i]).magnitude;
        if (loop)
            totalLength += (points[points.Count - 1] - points[0]).magnitude;

        for (int i = 0; i < points.Count; i ++)
            drawVertices(vh, points[i], points[(i + 1) % points.Count]);

        for (int i = 0; i < points.Count - 1; i++) {
            int index = i * 4;
            vh.AddTriangle(index + 0, index + 1, index + 3);
            vh.AddTriangle(index + 3, index + 2, index + 1);
        }

        if (loop) {
            drawVertices(vh, points[points.Count - 1], points[0]);

            int index = points.Count * 4;
            vh.AddTriangle(index + 0, index + 1, index + 3);
            vh.AddTriangle(index + 3, index + 2, index + 1);
        }
    }

    private void drawVertices(VertexHelper vh, Vector2 prev, Vector2 next) {
        UIVertex vertex = UIVertex.simpleVert;

        vertex.color = color;

        if (useWorldSpace) {
            prev = transform.InverseTransformPoint(prev);
            next = transform.InverseTransformPoint(next);
        }

        Vector2 dir = (next - prev).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x);

        vertex.position = prev + (-dir + normal) * thickness * thickCurve.Evaluate(length / totalLength);
        vh.AddVert(vertex);
        vertex.position = prev + (-dir - normal) * thickness * thickCurve.Evaluate(length / totalLength);
        vh.AddVert(vertex);

        length += (prev - next).magnitude;

        vertex.position = next + (dir - normal) * thickness * thickCurve.Evaluate(length / totalLength);
        vh.AddVert(vertex);
        vertex.position = next + (dir + normal) * thickness * thickCurve.Evaluate(length / totalLength);
        vh.AddVert(vertex);
    }
}
