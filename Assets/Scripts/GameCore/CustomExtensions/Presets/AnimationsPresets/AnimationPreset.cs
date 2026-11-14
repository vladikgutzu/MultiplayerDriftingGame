using System;
using UnityEngine;

namespace GameCore.CustomExtensions.Presets.AnimationsPresets
{
    public abstract class AnimationPreset : MonoBehaviour
    {
        public abstract void Animate(Action completeCallback = null, Action completeMainPartCallback = null, bool reset = true);
    }
}