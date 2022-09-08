using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Unity3D.Windows
{
    public class SlideWindowsLocationController : MonoBehaviour
    {
        [SerializeField] List<SlideWindowController> _windows = new List<SlideWindowController>();

        [SerializeField] float _openOffsetDifference = 100;
        public float OpenOffset
        {
            get
            {
                return _openOffsetDifference;
            }
        }

        private List<SlideWindowController> GetSortedOpenWindows()
        {

            if (_windows == null || _windows.Count < 1)
            {
                return new List<SlideWindowController>();
            }

            List<SlideWindowController> openWindows = new List<SlideWindowController>();

            foreach (var con in _windows)
            {
                if (con != null && !con.Closed)
                {
                    openWindows.Add(con);
                }
            }

            openWindows.Sort((x, y) =>
            {
                return x.OpenLocation.CompareTo(y.OpenLocation);
            });

            return openWindows;
        }

        public void ShiftWindows()
        {
            var windows = GetSortedOpenWindows();

        }
    }
}
