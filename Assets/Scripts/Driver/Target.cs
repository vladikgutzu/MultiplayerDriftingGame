
//using Photon.Pun;

using System;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Transform PointTarget;
    public Transform ParentTarget;
    public Transform steeringWheel;

    public bool MovieToPoint;
    public float DistanceToTarget;

    [SerializeField] private float animSpeed;

    public bool inCar;

    void Update()
    {
        if (inCar)
        {
            if (steeringWheel.localEulerAngles.y < 5 && steeringWheel.localEulerAngles.y > -5 || steeringWheel.localEulerAngles.y > 355)
            {
                MovieToPoint = true;
            }
            
            float distanceToParent = Vector3.Distance(ParentTarget.position, transform.position);
            if (distanceToParent >= DistanceToTarget)
            {
                MovieToPoint = true;
            }

            if (MovieToPoint)
            {
                if (distanceToParent >= 0)
                {
                    PointTarget.position =
                        Vector3.MoveTowards(transform.position, ParentTarget.position, animSpeed);
                    PointTarget.rotation = ParentTarget.rotation;
                }

                if (distanceToParent <= 0)
                {
                    MovieToPoint = false;
                }
            }

            transform.rotation = PointTarget.rotation;
            transform.position = PointTarget.position;
        }
    }
}