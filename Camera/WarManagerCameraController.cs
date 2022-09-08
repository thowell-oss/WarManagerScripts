/* CameraController.cs
 * Author: @kylewbanks 
 * Modified By: Taylor Howell 
 * Note: information copied from 
 */

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

using WarManager.Backend;
using WarManager.Unity3D;

namespace WarManager
{
    [RequireComponent(typeof(Camera))]
    /// <summary>
    /// Controls the Camera
    /// </summary>

    [Notes.Modified("Taylor Howell", "Fixed several bugs and added functionality to plugin to war manager. Also added zooming to location of the mouse/pinch, tweaked other bounds settings, etc.", 2)]
    public class WarManagerCameraController : MonoBehaviour
    {

        public bool isPaused { get; private set; } = false;
        public bool isFocused { get; private set; } = true;

        public Vector2 StartSheetCameraOffset = new Vector2(10, 10);

        private static readonly float PanSpeed = 20f / 6;
        private static readonly float PanSpeedY = 2f;
        private static readonly float ZoomSpeedTouch = 0.1f;
        private static readonly float ZoomSpeedMouse = 20;

        public EventSystem sceneEventSystem;

        public float[] BoundsX { get; private set; } = new float[] { -20, 20 };
        public float[] BoundsY { get; private set; } = new float[] { -20, 20 };

        public bool ignoreBounds = false;
        public Vector2 _boundsFudge = new Vector2(5, 5);

        public bool isMoving;

        // /// <summary>
        // /// How much pinching will it take to zoom in/out?
        // /// </summary>
        // public float PinchToZoomThresholdAmt = 3;

        // public Vector2 pinchToZoomMidPointOffset = new Vector2(5, 5);


        public float zoomOutMin = 1;
        public float zoomOutMax = 20;
        public float ZoomMouseMultiplier = 10;
        Vector3 touchStart;

        bool started = false;

        bool startedZoomPanning = false;

        public bool OverWorkSpace;

        [SerializeField] private Color DefaultColor;

        /// <summary>
        /// Is the pointer over the card editor work space?
        /// </summary>
        /// <value></value>
        public bool OverCardEditorWorkSpace { get; private set; } = false;

        private static readonly float[] ZoomBounds = new float[] { 5f, 22f };

        private List<CamLocation> _activeSheetCameraStateList = new List<CamLocation>();
        private string CurrentCamSheetSettingsId;

        private static Camera _cam;

        public static WarManagerCameraController MainController;

        /// <summary>
        /// Get the camera from the war manager controller
        /// </summary>
        /// <value></value>
        public Camera GetCamera
        {
            get
            {
                return _cam;
            }
        }

        /// <summary>
        /// Get the active camera from the war manager controller
        /// </summary>
        /// <value></value>
        public static Camera MainCamera
        {
            get
            {
                return _cam;
            }
        }

        // private Vector3 lastPanPosition;
        // private int panFingerId; // Touch mode only

        // private bool wasZoomingLastFrame; // Touch mode only
        // private Vector2[] lastZoomPositions; // Touch mode only

        /// <summary>
        /// Is the user currently navigating through a menu?
        /// </summary>
        public bool InMenu { get; set; }

        /// <summary>
        /// get the zoom percentage as a normalized float
        /// </summary>
        /// <value></value>
        public float ZoomAmtNormalized
        {
            get
            {
                float amt = GetCamera.orthographicSize - zoomOutMin;
                float max = zoomOutMax - zoomOutMin;

                return amt / max;
            }
        }

        public delegate void CameraChange();
        public static event CameraChange OnCameraChange;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographicSize = 10;

            if (MainController == null)
            {
                MainController = this;
            }
            else
            {
                Debug.LogError("There should be only one main camera controller in the scene");
                Destroy(MainController);
                return;
            }

            if (SheetsManager.Camera == null)
            {
                SheetsManager.Camera = this;
            }
            else
            {
                NotificationHandler.Print("There is already another camera in the scene.");
            }
        }

        public Pointf GetPointfLocation()
        {
            Vector2 position = transform.position;
            return new Pointf(position.x, position.y);
        }

        void LateUpdate()
        {
            if (ToolsManager.Mode == WarMode.Menu || ToolsManager.SelectedTool == ToolTypes.None)
                return;

            if (Input.GetMouseButtonDown(2) || (Input.GetMouseButtonDown(0) && ToolsManager.SelectedTool == ToolTypes.Pan))
            {
                touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                started = false;

                if (LeanTween.isTweening(this.gameObject))
                {
                    LeanTween.cancel(this.gameObject);
                }
            }

            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;

                Vector2 touchMidPosition = new Vector2((touchZero.position.x + touchOne.position.x) / 2, (touchZero.position.y + touchOne.position.y) / 2);


                if (!started)
                {
                    touchStart = Camera.main.ScreenToWorldPoint(touchMidPosition);
                    started = true;

                    if (LeanTween.isTweening(this.gameObject))
                    {
                        LeanTween.cancel(this.gameObject);
                    }
                }
                else
                {
                    PanCamera();
                }

                ZoomCamera(difference * 0.01f * GeneralSettings.zoomPower);
            }

            if (Input.GetMouseButton(2) || (Input.GetMouseButton(0) && ToolsManager.SelectedTool == ToolTypes.Pan))
            {
                PanCamera();
                started = false;
            }

            float x = Input.GetAxis("Mouse ScrollWheel") * GeneralSettings.zoomPower;

            if (Input.touchCount < 1 && x != 0)
            {
                if (!startedZoomPanning)
                {
                    touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    startedZoomPanning = true;
                }

                ZoomCamera(x, true);

                started = false;
            }

            if (Input.touchCount < 1)
            {
                started = false;
            }

            if (Mathf.Abs(Input.GetAxis("Mouse X")) < .25f || Mathf.Abs(Input.GetAxis("Mouse Y")) < .25f)
            {
                startedZoomPanning = false;
            }
        }

        void PanCamera()
        {
            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.position += direction;
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -20);

            if (OnCameraChange != null)
            {
                OnCameraChange();
            }
        }

        void ZoomCamera(float increment, bool panToLocation = false)
        {

            if (DrawRectOnCanvas.MouseContextStatus != MouseStatus.editTool &&
                DrawRectOnCanvas.MouseContextStatus != MouseStatus.selectTool &&
                DrawRectOnCanvas.MouseContextStatus != MouseStatus.calculateTool &&
                DrawRectOnCanvas.MouseContextStatus != MouseStatus.annotateTool)
                return;

            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, zoomOutMin, zoomOutMax);

            if (panToLocation)
            {
                PanCamera();
            }
            else
            {
                if (OnCameraChange != null)
                {
                    OnCameraChange();
                }
            }

        }


        /// <summary>
        /// Move the camera to a new float point position (world coordinates)
        /// </summary>
        /// <param name="newPosition"></param>
        public void MoveCamera(Pointf newPosition)
        {
            float moveTime = .25f;

            LeanTween.move(this.gameObject, new Vector2(newPosition.x, newPosition.y), moveTime).setEaseOutCubic();

            LeanTween.delayedCall(moveTime, () =>
            {
                if (OnCameraChange != null)
                {
                    OnCameraChange();
                }
            });
        }


        /// <summary>
        /// Move the camera to a specific grid point
        /// </summary>
        /// <param name="gridPoint"></param>
        public void MoveCamera(Point gridPoint)
        {
            if (SheetsManager.SheetCount > 0)
            {
                var grid = SheetsManager.GetWarGrid(SheetsManager.CurrentSheetID);

                //Pointf gridScale = new Pointf((float)GeneralSettings.DefaultGridScale[0], (float)GeneralSettings.DefaultGridScale[1]);
                Pointf p = Pointf.GridToWorld(gridPoint, grid);

                MoveCamera(p);
            }
        }

        /// <summary>
        /// Move the camera if the distance is greater than the distance fudge
        /// </summary>
        /// <param name="gridPoint">the point to move</param>
        /// <param name="distanceFudge">the fudging distance between the two values</param>
        public void MoveCamera(Point gridPoint, float distanceFudge)
        {
            if (SheetsManager.SheetCount > 0)
            {
                var grid = SheetsManager.GetWarGrid(SheetsManager.CurrentSheetID);

                //Pointf gridScale = new Pointf((float)GeneralSettings.DefaultGridScale[0], (float)GeneralSettings.DefaultGridScale[1]);
                Pointf p = Pointf.GridToWorld(gridPoint, grid);

                Vector2 loc = new Vector2(p.x, p.y);
                if (Vector2.Distance(loc, transform.position) > distanceFudge)
                    MoveCamera(p);
            }
        }

        /// <summary>
        /// Set the background color of the sheet
        /// </summary>
        /// <param name="color"></param>
        public void SetBackgroundColor(string color)
        {
            if (ColorUtility.TryParseHtmlString(color, out var col))
            {
                SetBackgroundColor(col);
            }
        }

        /// <summary>
        /// Set the background color of the sheet
        /// </summary>
        public void SetBackgroundColor(Color color)
        {
            GetCamera.backgroundColor = color;
        }

        /// <summary>
        /// Set the background to the default color
        /// </summary>
        public void SetDefaultColor()
        {
            SetBackgroundColor(DefaultColor);
        }

        public void AnimateZoom(float nextAmt)
        {
            _cam.orthographicSize = nextAmt;

            if (OnCameraChange != null)
                OnCameraChange();
        }

        /// <summary>
        /// Get the bounds from the card driver and apply them to the camera
        /// </summary>
        private void ApplyBounds()
        {
            ((float x, float y) upperLeft, (float x, float y) lowerRight) rect = CardUtility.GetGlobalBounds(WarManagerDriver.Main.GetGlobalOffsetTuple(),
                WarManagerDriver.Main.GetCardMultiplierTuple());

            SetBounds(rect.upperLeft.x, rect.lowerRight.x, rect.lowerRight.y, rect.upperLeft.y);

            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
            pos.y = Mathf.Clamp(transform.position.y, BoundsY[0], BoundsY[1]);
            transform.position = pos;
        }

        /// <summary>
        /// Set the camera bounds
        /// </summary>
        /// <param name="minX">minumum x value bound</param>
        /// <param name="maxX">maximum x value bound</param>
        /// <param name="minY">minimum y value bound</param>
        /// <param name="maxY">maximum y value bound</param>
        private void SetBounds(float minX, float maxX, float minY, float maxY)
        {
            BoundsX[0] = minX + _boundsFudge.x * _cam.orthographicSize;
            BoundsX[1] = maxX + _boundsFudge.y * _cam.orthographicSize;

            // Debug.DrawLine(new Vector2(minX, minY), new Vector2(minX, maxY), Color.green);
            // Debug.DrawLine(new Vector2(minX, minY), new Vector2(maxX, minY), Color.green);
            // Debug.DrawLine(new Vector2(minX, maxY), new Vector2(maxX, maxY), Color.green);
            // Debug.DrawLine(new Vector2(maxX, minY), new Vector2(maxX, maxY), Color.green);

            BoundsY[0] = minY + _boundsFudge.x / 2 * _cam.orthographicSize;
            BoundsY[1] = maxY + _boundsFudge.y / 2 * _cam.orthographicSize;
        }

        /// <summary>
        /// Is the location in world space within the orthographic camera bounds?
        /// </summary>
        /// <param name="location">the point in world space</param>
        /// <returns>returns true if the point is within the bounds of the orthoghraphic camera, false if not</returns>
        public bool IsInOrthographicCameraBounds(Vector2 location, float cameraFudgeBounds)
        {
            var locations = GetCameraOrthographicBounds(cameraFudgeBounds);

            var topLeft = locations.topLeft;
            var bottomRight = locations.bottomRight;


            //Debug.DrawLine(topLeft, bottomRight);

            if (location.x > topLeft.x && location.x < bottomRight.x)
            {
                if (location.y < topLeft.y && location.y > bottomRight.y)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns two points of the camera orthographic bounds
        /// </summary>
        /// <param name="cameraFudgeBounds">the camera bounds fudging</param>
        /// <returns>Returns two points of the camera orthographic bounds (top left, bottom right) as a vector2 tuple</returns>
        public (Vector2 topLeft, Vector2 bottomRight) GetCameraOrthographicBounds(float cameraFudgeBounds = 0)
        {
            float xBounds = _cam.aspect * _cam.orthographicSize + cameraFudgeBounds;
            float yBounds = _cam.orthographicSize + cameraFudgeBounds;

            Vector3 camPos = _cam.transform.position;

            Vector2 topleft = new Vector2(camPos.x - xBounds, camPos.y + yBounds);
            Vector2 bottomRight = new Vector2(camPos.x + xBounds, camPos.y - yBounds);

            //Debug.DrawLine(topleft, bottomRight);

            return (topleft, bottomRight);

        }


        /// <summary>
        /// Move the camera to a specific point on the sheet
        /// </summary>
        /// <param name="location">the grid point location on the sheet</param>
        /// <param name="grid">the grid settings of the sheet</param>
        /// <param name="zoomAmt">the zoom amout of the camera (between 5 and 22)</param>
        /// <param name="timeToMove">the tween time it takes to move to the location</param>
        public void MoveCamera(Point location, WarGrid grid, float zoomAmt = 20, float timeToMove = .25f / 3)
        {
            var worldPoint = Point.GridToWorld(location, grid);
            Vector2 vLocation = WarManagerExtensions.ConvertToVector2(worldPoint);
            SetCamera(vLocation, zoomAmt, true, timeToMove);
        }

        /// <summary>
        /// Set the camera to a specific position
        /// </summary>
        /// <param name="location"></param>
        /// <param name="zoomAmt"></param>
        public void SetCamera(Vector2 location, float zoomAmt, bool smooth = false, float timeToMove = .25f / 3)
        {

            if (!smooth)
            {
                transform.position = new Vector3(location.x, location.y, -20);
                GetCamera.orthographicSize = zoomAmt;
                // CacheCameraLocation();
            }
            else
            {
                LeanTween.cancel(this.gameObject);
                float time = timeToMove;
                LeanTween.value(this.gameObject, SmoothSetCameraLocation, (Vector2)transform.position, location, time * 2).setEaseOutCubic();
                LeanTween.delayedCall(time, () =>
                {
                    LeanTween.value(this.gameObject, SmoothSetCameraOrth, GetCamera.orthographicSize, zoomAmt, time * 2).setEaseOutCubic();
                    // CacheCameraLocation();
                });
            }
        }


        /// <summary>
        /// Used for the lean twean library
        /// </summary>
        /// <param name="location"></param>
        private void SmoothSetCameraLocation(Vector2 location)
        {
            transform.position = new Vector3(location.x, location.y, -20);
        }

        private void SmoothSetCameraOrth(float zoomAmt)
        {
            GetCamera.orthographicSize = zoomAmt;
        }

        private void CacheCameraLocation()
        {
            var camLoc = _activeSheetCameraStateList.Find(x => x.ID == CurrentCamSheetSettingsId);

            if (camLoc != null)
            {
                camLoc.Location = transform.position;
                camLoc.Orthoghraphic = GetCamera.orthographicSize;
                // Debug.Log("Caching location");
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            isPaused = pauseStatus;
        }

        void OnApplicationFocus(bool focused)
        {
            isFocused = focused;

            if (!focused)
            {
                // Application.targetFrameRate = 15;
                // QualitySettings.vSyncCount = 0;
            }
            else
            {
                // Application.targetFrameRate = 90;
                // QualitySettings.vSyncCount = 0;
            }
        }

        /// <summary>
        /// Get the location of the camera on a specific sheet
        /// </summary>
        /// <param name="sheetId">the sheet id</param>
        /// <returns></returns>
        public CamLocation GetCameraLocation(string sheetId)
        {
            return _activeSheetCameraStateList.Find(x => x.ID == sheetId);
        }

        /// <summary>
        /// call this when the sheets change (close, open, new, change, etc.)
        /// </summary>
        /// <param name="id">the id of the sheet</param>
        void OnChangeSheets(string id)
        {
            if (CurrentCamSheetSettingsId != null)
            {
                var settings = _activeSheetCameraStateList.Find((x) => x.ID == CurrentCamSheetSettingsId);
                settings.Location = GetCamera.transform.position;
                settings.Orthoghraphic = GetCamera.orthographicSize;
            }

            var camSettings = _activeSheetCameraStateList.Find((x) => x.ID == id);

            if (camSettings == null)
            {

                if (WarSystem.GetSheetMetaData(id, out var sheet))
                {
                    camSettings = new CamLocation(id, sheet.LastCameraLocation.ConvertToUnityVector2(), 20);
                    _activeSheetCameraStateList.Add(camSettings);
                    //  Debug.Log("Getting sheet meta data " + camSettings.Location.ToString());
                }
                else
                {
                    camSettings = new CamLocation(id, StartSheetCameraOffset, 20);
                    // Debug.Log("Could not find sheet meta data");
                    _activeSheetCameraStateList.Add(camSettings);
                }
            }

            SetCamera(camSettings.Location, camSettings.Orthoghraphic);

            CurrentCamSheetSettingsId = camSettings.ID;
        }

        private void CardEditorCameraInteract(bool interact)
        {
            OverCardEditorWorkSpace = interact;
        }

        void OnEnable()
        {
            SheetsManager.OnSetSheetCurrent += OnChangeSheets;
            SheetMouseDetection.OnMouseOverWorkSpace += CardEditorCameraInteract;
        }

        void OnDisable()
        {
            SheetsManager.OnSetSheetCurrent -= OnChangeSheets;
            SheetMouseDetection.OnMouseOverWorkSpace += CardEditorCameraInteract;
        }
    }


    /// <summary>
    /// Handles the location of cameras for multiple sheets
    /// </summary>
    public class CamLocation
    {
        public string ID;
        public Vector2 Location;
        public float Orthoghraphic;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">the sheet id</param>
        /// <param name="location">the location of the camera</param>
        /// <param name="orthoghraphic"></param>
        public CamLocation(string id, Vector2 location, float orthoghraphic)
        {
            ID = id;
            Location = location;
            Orthoghraphic = orthoghraphic;
        }

        public override string ToString()
        {
            return $"{ID} {Location.ToString()} {Orthoghraphic}";
        }
    }
}
