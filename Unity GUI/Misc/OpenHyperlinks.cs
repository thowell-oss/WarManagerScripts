using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace WarManager
{

    [RequireComponent(typeof(TextMeshProUGUI))]
    public class OpenHyperlinks : MonoBehaviour, IPointerClickHandler
    {

        private TMP_Text pTextMeshPro;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (pTextMeshPro == null)
                pTextMeshPro = GetComponent<TMP_Text>();

            var pCamera = Camera.main;

            // Debug.Log(pCamera + " " + eventData.pointerPressRaycast.worldPosition);
            // Debug.DrawLine(eventData.pointerPressRaycast.worldPosition, eventData.pointerPressRaycast.worldPosition + new Vector3(0,5,0), Color.red, 10);

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, Input.mousePosition, pCamera);
            if (linkIndex != -1)
            { // was a link clicked?
                TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];

                // open the link id as a url, which is the metadata we added in the text field
                Application.OpenURL(linkInfo.GetLinkID());
            }
        }

        List<Color32[]> SetLinkToColor(int linkIndex, Color32 color)
        {
            if (pTextMeshPro == null)
                pTextMeshPro = GetComponent<TMP_Text>();

            TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];

            var oldVertColors = new List<Color32[]>(); // store the old character colors

            for (int i = 0; i < linkInfo.linkTextLength; i++)
            { // for each character in the link string
                int characterIndex = linkInfo.linkTextfirstCharacterIndex + i; // the character index into the entire text
                var charInfo = pTextMeshPro.textInfo.characterInfo[characterIndex];
                int meshIndex = charInfo.materialReferenceIndex; // Get the index of the material / sub text object used by this character.
                int vertexIndex = charInfo.vertexIndex; // Get the index of the first vertex of this character.

                Color32[] vertexColors = pTextMeshPro.textInfo.meshInfo[meshIndex].colors32; // the colors for this character
                oldVertColors.Add(vertexColors);

                if (charInfo.isVisible)
                {
                    vertexColors[vertexIndex + 0] = color;
                    vertexColors[vertexIndex + 1] = color;
                    vertexColors[vertexIndex + 2] = color;
                    vertexColors[vertexIndex + 3] = color;
                }
            }

            // Update Geometry
            pTextMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

            return oldVertColors;
        }
    }
}
