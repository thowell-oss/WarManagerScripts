using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using WarManager.Forms;

namespace WarManager.Unity3D
{
    public class FormsHider : MonoBehaviour
    {
        public bool FormsActive { get; private set; } = false;

        [SerializeField] bool InvertFormsActiveBoolean = false;

        public UnityEvent<bool> OnForms;

        void OnEnable()
        {
            FormsController.OnForms += SetFormsActive;
        }

        void SetFormsActive(object sender, bool forms)
        {
            if (sender is FormsController)
            {
                if (InvertFormsActiveBoolean)
                {
                    forms = !forms;
                }

                FormsActive = forms;

                if (OnForms != null)
                {
                    OnForms.Invoke(forms);
                }
            }
        }
    }
}
