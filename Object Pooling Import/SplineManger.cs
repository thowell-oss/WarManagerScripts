using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineManger : MonoBehaviour
{

    public List<Transform> splines = new List<Transform>();

    public float GizmoSize = 2;
    public Color GizmoColor = Color.green;

    // public LineRenderer lineRenderer;

    // Update is called once per frame
    void Update()
    {
        ShowLines();
    }

    public void ShowLines()
    {
        // if (splines.Count < 2)
        //     return;

        // List<Vector3> locations = new List<Vector3>();

        // for (int i = 0; i < splines.Count; i++)
        // {
        //     locations.Add(splines[i].position);
        // }

        // lineRenderer.positionCount = locations.Count;
        // lineRenderer.SetPositions(locations.ToArray());
    }

    // void OnDrawGizmos()
    // {
    //     if (splines == null || splines.Count < 1)
    //         return;

    //     Gizmos.color = GizmoColor;

    //     if (splines.Count > 0)
    //         Gizmos.DrawWireSphere(splines[0].position, GizmoSize);

    //     if (splines.Count < 2)
    //         return;


    //     for (int i = 0; i < splines.Count - 1; i++)
    //     {
    //         Gizmos.DrawLine(splines[i].position, splines[i + 1].position);
    //         Gizmos.DrawWireSphere(splines[i + 1].position, GizmoSize);
    //     }

    // }
}
