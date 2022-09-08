using System.Collections.Generic;

using UnityEngine;

using WarManager.Backend;

namespace WarManager
{
    /// <summary>
    /// Handles the world gismos (keeps track of instances and overall pool object life cycle).
    /// </summary>
    [Notes.Author("Handles the world gismos")]
    public class GismosManager : MonoBehaviour
    {
        private Dictionary<Point, WorldGismo> _gismosDict = new Dictionary<Point, WorldGismo>();
        [SerializeField] Transform _parent;
        [SerializeField] GameObject _prefabGismo;

        [SerializeField] WarManagerCameraController CameraController;

        public static GismosManager Main;

        /// <summary>
        /// The current active gismos
        /// </summary>
        public IEnumerable<WorldGismo> Gismos
        {
            get => _gismosDict.Values;
        }

        /// <summary>
        /// The amount of gismos currently active
        /// </summary>
        public int GismosCount
        {
            get => _gismosDict.Count;
        }

        private void Awake()
        {
            if (Main == null)
                Main = this; //singleton pattern
            else
                Destroy(this);
        }

        /// <summary>
        /// Set the world gismo
        /// </summary>
        /// <param name="p">the point p</param>
        /// <returns>returns the world gismo</returns>
        public WorldGismo SetWorldGismo(Point p, bool doNotPlaceIfCardExistsAtPoint)
        {
            if (_gismosDict.ContainsKey(p))
                return null;

            if (!p.IsInGridBounds)
                return null;

            if (SheetsManager.TryGetCurrentSheet(out var sheet))
                if (doNotPlaceIfCardExistsAtPoint)
                {
                    if (CardUtility.ContainsCard(p, sheet.CurrentLayer, sheet.ID))
                    {
                        return null;
                    }
                }

            var grid = SheetsManager.GetWarGrid(sheet.ID);

            WorldGismo g = PoolManager.Main.CheckOutGameObject<WorldGismo>(_prefabGismo, false, _parent);
            g.cameraController = CameraController;
            g.SetWorldGizmoSetWorldGismoOnCurrentSheet(p, grid);
            g.gameObject.SetActive(true);

            _gismosDict.Add(p, g);

            return g;
        }

        /// <summary>
        /// Clear gismos
        /// </summary>
        public void ClearGismos()
        {
            foreach (var x in _gismosDict)
            {
                PoolManager.Main.TryCheckInObject(_prefabGismo, x.Value.gameObject, _parent);
            }

            _gismosDict.Clear();
        }

        /// <summary>
        /// Clear a gismo on the selected point  <paramref name="p"/>
        /// </summary>
        /// <param name="p">point p</param>
        /// <returns></returns>
        public bool ClearGismo(Point p)
        {
            return _gismosDict.Remove(p);
        }

        /// <summary>
        /// does the gismo exist at a selected point?
        /// </summary>
        /// <param name="p">the point</param>
        /// <returns>returns true if the gismo is selected, false if not</returns>
        public bool GismoExistsAtSelectedPoint(Point p)
        {
            return _gismosDict.ContainsKey(p);
        }

        /// <summary>
        /// do something when an new sheet is set to current (responds to event)
        /// </summary>
        /// <param name="id">the id of the sheet</param>
        private void OnSetSheetCurrent(string id)
        {
            ClearGismos();
        }

        void OnEnable()
        {
            SheetsManager.OnSetSheetCurrent += OnSetSheetCurrent;
        }

        void OnDisable()
        {
            SheetsManager.OnSetSheetCurrent -= OnSetSheetCurrent;
        }
    }
}
