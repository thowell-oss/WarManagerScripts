using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D.Windows
{
    /// <summary>
    /// The active element's interface
    /// </summary>
    public interface ISlideWindow_Element
    {
        /// <summary>
        /// The info to use
        /// </summary>
        public SlideWindow_Element_ContentInfo info { get; set; }

        /// <summary>
        /// The content to search for
        /// </summary>
        public string SearchContent { get; }

        /// <summary>
        /// The parent gameobject of the element
        /// </summary>
        public GameObject targetGameObject { get; }

        /// <summary>
        /// Update the content when necessary
        /// </summary>
        public void UpdateElement();

    }
}
