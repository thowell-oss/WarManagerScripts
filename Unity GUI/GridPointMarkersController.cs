using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarManager.Unity3D;

using WarManager.Backend;

namespace WarManager
{
    /// <summary>
    /// Handles moving grid point markers
    /// </summary>
    [Notes.Author("Handles moving grid point markers")]
    public class GridPointMarkersController : MonoBehaviour
    {
        [SerializeField] DropPointMarker GridPointMarkerPrefab;
        [SerializeField] DropPointMarker AddCardsMarkerPrefab;

        private List<DropPointMarker> _markers = new List<DropPointMarker>();
        private List<DropPointMarker> _AddCardsMarkers = new List<DropPointMarker>();

        void Start()
        {
            PoolManager.Main.RegisterPrefab(GridPointMarkerPrefab.gameObject, 10, this.transform);
        }

        /// <summary>
        /// Set new marker locations
        /// </summary>
        /// <param name="points">the list of points</param>
        public void SetDropPointMarkers(List<Point> points)
        {
            ClearDropPointMarkers();

            if (SheetsManager.TryGetCurrentSheet(out var sheet))
            {
                var grid = SheetsManager.GetWarGrid(sheet.ID);

                for (int i = 0; i < points.Count; i++)
                {
                    var marker = PoolManager.Main.CheckOutGameObject<DropPointMarker>(GridPointMarkerPrefab.gameObject, true, this.transform);

                    marker.Grid = grid;
                    marker.SetLocation(points[i]);
                    //Debug.Log("setting current points " + points[i]);

                    _markers.Add(marker);
                }
            }
        }

        /// <summary>
        /// Set add card markers
        /// </summary>
        /// <param name="points">the list of points </param>
        public void SetAddCardMarkers(List<Point> points)
        {
            ClearAddCardMarkers();

            if (SheetsManager.TryGetCurrentSheet(out var sheet))
            {
                var grid = SheetsManager.GetWarGrid(sheet.ID);

                for (int i = 0; i < points.Count; i++)
                {
                    var marker = PoolManager.Main.CheckOutGameObject<DropPointMarker>(AddCardsMarkerPrefab.gameObject, true, this.transform);

                    marker.Grid = grid;
                    marker.SetLocation(points[i]);
                    //Debug.Log("setting current points " + points[i]);

                    _AddCardsMarkers.Add(marker);
                }
            }
        }

        /// <summary>
        /// Clear the markers
        /// </summary>
        public void ClearDropPointMarkers()
        {
            foreach (var x in _markers)
            {
                x.gameObject.SetActive(false);
            }
        }

        public void ClearAddCardMarkers()
        {
            foreach (var x in _AddCardsMarkers)
            {
                x.gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            SheetDropPointManager.OnCustomDropPointsChange += SetDropPointMarkers;
        }

        void OnDisable()
        {
            SheetDropPointManager.OnCustomDropPointsChange -= SetDropPointMarkers;
        }
    }
}
