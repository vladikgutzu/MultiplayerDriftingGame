using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_SWITCH
using UnityEngine.Switch;
#endif


public class LoadingScreenBoost : MonoBehaviour
{
    private void OnEnable()
    {
#if UNITY_SWITCH && !UNITY_EDITOR
        Performance.SetCpuBoostMode(Performance.CpuBoostMode.FastLoad);
#endif
    }
    
    private void OnDisable()
    {
#if UNITY_SWITCH && !UNITY_EDITOR
        Performance.SetCpuBoostMode(Performance.CpuBoostMode.Normal);
#endif
    }

    private void OnDestroy()
    {
#if UNITY_SWITCH && !UNITY_EDITOR
        Performance.SetCpuBoostMode(Performance.CpuBoostMode.Normal);
#endif
    }
}
