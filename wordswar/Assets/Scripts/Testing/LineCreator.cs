using UnityEngine;

public class LineCreator : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Vector3 startPoint;
    public Vector3 endPoint;

    void Start()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
    }
}
