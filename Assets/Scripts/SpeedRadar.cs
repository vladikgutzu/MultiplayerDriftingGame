using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SpeedRadar : MonoBehaviour
{
    public float _speedTarget = 0;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !other.TryGetComponent(out RCCP_CarController car))
            return;

        if (car.physicalSpeed >= _speedTarget)
        {
            Destroy(gameObject);
        }
    }
}
