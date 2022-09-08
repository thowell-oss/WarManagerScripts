using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D
{
    [RequireComponent(typeof(TMPro.TMP_Text))]
    public class VersionInfoController : MonoBehaviour
    {
        private TMPro.TMP_Text txt;
 
        // Start is called before the first frame update
        void Start()
        {
            txt = GetComponent<TMPro.TMP_Text>();
            txt.text = "v" + WarSystem.ReleaseVersion + " (" + WarSystem.MajorReleaseName + ") " + WarSystem.ReleaseType;
        }
    }
}
