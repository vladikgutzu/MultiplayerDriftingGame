using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HandTargetPoint : MonoBehaviour
{
    Vector3 StartPos;
    Quaternion StartRotation;

    public Transform ParentTarget;

    public Transform StartPoint;

    public Target Target;
    public SteeringWheel TempSteeringWheel;

    private PhotonView pV;
    private bool isMine;

    void Awake()
    {
        // StartCoroutine(SetReferences());
    }

    IEnumerator SetReferences()
    {
        yield return new WaitForSeconds(0.25f);
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
        }
        else
        {
            if (StartPoint)
            {
                StartPos = StartPoint.position;
            }

            if (ParentTarget)
            {
                Target.ParentTarget = ParentTarget;
            }

            StartRotation = transform.rotation;
        }
    }

    void Update()
    {
        // if (!isGarage)
        // {
        //     transform.position = StartPos;
        //     transform.rotation = StartRotation;
        // }
    }
}
