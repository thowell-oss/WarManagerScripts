using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{

    public class BezierCurveHandler : MonoBehaviour
    {
        public List<Transform> Locations = new List<Transform>();

        public void OnDrawGizmos()
        {

            if (Locations != null && Locations.Count >= 2)
            {

                Vector3 lastLocation = Vector3.zero;

                Gizmos.color = Color.cyan;

                for (float i = 0; i <= 10; i++)
                {
                    if (lastLocation != Vector3.zero)
                        Gizmos.DrawLine(Bezier.CalculateBezierLocation(i / 10, Locations), lastLocation);

                    lastLocation = Bezier.CalculateBezierLocation(i / 10, Locations);
                }

                foreach (var x in Bezier.GetBezierPoints(Locations, 10))
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
