using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LightLOD : MonoBehaviour
{

    Transform mainCam;
    Light lightComnponent;

    void Start()
    {
        mainCam = Camera.main.transform;
        lightComnponent = GetComponent<Light>();
    }


    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            if (!lightComnponent.enabled)
                lightComnponent.enabled = true;
            if (lightComnponent.shadows != LightShadows.Soft)
                lightComnponent.shadows = LightShadows.Soft;
            return;
        }

        if (Vector3.Distance(mainCam.position + mainCam.forward * 50f, transform.position) < 100f)
        {
            if (!lightComnponent.enabled)
            {
                lightComnponent.enabled = true;
            }
        }
        else
        {
            if (lightComnponent.enabled)
            {
                lightComnponent.enabled = false;
            }
        }

        if (Vector3.Distance(mainCam.position + mainCam.forward * 5f, transform.position) < 20f)
        {
            if (lightComnponent.shadows == LightShadows.None)
            {
                lightComnponent.shadows = LightShadows.Soft;
            }
        }
        else
        {
            if (lightComnponent.shadows == LightShadows.Soft)
            {
                lightComnponent.shadows = LightShadows.None;
            }
        }
    }
}
