using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

namespace WarManager.Unity3D
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public string headerText;
        public string contentText;

        public bool useBoundsSystem = false;

        public Vector2 offset;
        public Vector2 size;
        public Camera cam;
        public Vector3 pos;

        private Vector3 lastPos = Vector3.zero;

        bool isOn = false;

        private void Awake() => StartCoroutine(TooltipCheck());

        void Update()
        {

            // if (cam == null)
            //     cam = WarManagerCameraController.MainCamera;

            // pos = cam.ScreenToWorldPoint(Input.mousePosition);

            // var distance = Vector3.Distance(lastPos, pos);

            // if (distance > .1f)
            // {
            //     TooltipSystem.main.Hide();
            //     Debug.Log(distance);
            // }

            // lastPos = pos;
        }

        IEnumerator TooltipCheck()
        {
            yield return new WaitForSecondsRealtime(.25f);
            CheckToolTip();

            if (useBoundsSystem)
                StartCoroutine(TooltipCheck());
        }

        void CheckToolTip()
        {
            if (!useBoundsSystem)
                return;

            //Debug.Log("checking " + size + " " + offset);

            if (cam == null)
                cam = WarManagerCameraController.MainCamera;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = cam.nearClipPlane;
            pos = cam.ScreenToWorldPoint(mousePos);

            if (pos.x > transform.position.x + offset.x - size.x / 2)
            {
                if (pos.x < transform.position.x + offset.x + size.x / 2)
                {
                    //Debug.Log("x: yes");

                    if (pos.y > transform.position.y + offset.y - size.y / 2)
                    {
                        if (pos.y < transform.position.y + offset.y + size.y / 2)
                        {
                            if (!isOn)// && InputSystem.Main.InputMode == InputMode.MouseAndKeyboard)
                            {
                                TooltipSystem.main.Show(headerText, contentText);
                                isOn = true;
                            }

                            //Debug.Log("y: yes");
                            return;
                        }
                    }
                }
            }

            if (isOn)
            {
                isOn = false;
                TooltipSystem.main.Hide();
            }


        }

        // void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.green;

        //     Gizmos.DrawLine(transform.position + new Vector3(offset.x, offset.y, transform.position.z) + new Vector3(size.x, size.y, transform.position.z), transform.position + new Vector3(offset.x, offset.y, transform.position.z) - new Vector3(size.x, size.y, transform.position.z));

        //     Gizmos.color = Color.blue;
        //     Gizmos.DrawLine(transform.position, pos);
        // }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (InputSystem.Main.InputMode == InputMode.MouseAndKeyboard)
                TooltipSystem.main.Show(headerText, contentText);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipSystem.main.Hide();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (InputSystem.Main.InputMode == InputMode.Touch)
            {
                TooltipSystem.main.Show(headerText, contentText);
            }
        }

        // public void OnMouseEnter()
        // {
        //     TooltipSystem.main.Show(headerText, contentText);
        // }

        // public void OnMouseExit()
        // {
        //     TooltipSystem.main.Hide();
        // }
    }
}
