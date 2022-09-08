using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D
{

    [RequireComponent(typeof(LineRenderer))]
    public class Spline : MonoBehaviour
    {

        public List<Transform> Locations = new List<Transform>();

        LineRenderer Renderer;

        public List<Vector3> lineLocations = new List<Vector3>();

        int accuracy = 10;


        public void Update()
        {
            if (Renderer == null)
                Renderer = GetComponent<LineRenderer>();


            lineLocations.Clear();

            for (int i = 0; i < accuracy; i++)
            {
                Vector3 location = BezierCurveCalculation.CalculateBezierLocation(i / accuracy, Locations);
                lineLocations.Add(location);
            }

            Renderer.SetPositions(lineLocations.ToArray());

        }

        public void OnDrawGizmos()
        {

            if (Locations != null && Locations.Count >= 2)
            {

                Vector3 lastLocation = Vector3.zero;

                Gizmos.color = Color.cyan;

                for (float i = 0; i <= 10; i++)
                {
                    if (lastLocation != Vector3.zero)
                        Gizmos.DrawLine(BezierCurveCalculation.CalculateBezierLocation(i / 10, Locations), lastLocation);

                    lastLocation = BezierCurveCalculation.CalculateBezierLocation(i / 10, Locations);
                }

                foreach (var x in BezierCurveCalculation.GetBezierPoints(Locations, 10))
                {
                    Gizmos.DrawWireSphere(x, .125f);
                }

                Gizmos.color = Color.green;


                for (int i = 1; i < Locations.Count; i++)
                {
                    Gizmos.DrawLine(Locations[i - 1].position, Locations[i].position);
                    Gizmos.DrawCube(Locations[i - 1].position, new Vector3(1, 1, 1));
                }

                Gizmos.DrawCube(Locations[Locations.Count - 1].position, new Vector3(1, 1, 1));
            }

        }
    }
}
