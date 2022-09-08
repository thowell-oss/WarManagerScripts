/* CustomAnimationEvents.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Apply custom event to animations
    /// </summary>
    public class CustomAnimationEvents : MonoBehaviour
    {
        public UnityEvent OnAnimationComplete;

        /// <summary>
        /// The animation has been completed
        /// </summary>
        public void AnimationComplete()
        {
            if (OnAnimationComplete != null)
                OnAnimationComplete.Invoke();
        }
    }
}