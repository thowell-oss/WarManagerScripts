using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Forms
{
    public interface IFormsInput
    {
        public IFormsInput Previous { get; set; }
        public IFormsInput Next { get; set; }
        public FormsInputState CurrentState { get; set; }

        public string GroupID { get; set; }
        public int GroupOrder { get; set; }

        public bool Critical { get; set; }
        public bool Verified { get; set; }

        public void SetInputFocused(bool focused);

        public FormsInputState SetState();
    }

    public enum FormsInputState
    {
        Active,
        Completed,
        Incompleted,
        Error,
        Disabled,
        Hidden
    }
}
