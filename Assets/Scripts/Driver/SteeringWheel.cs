using System.Collections;
using UnityEngine;

public class SteeringWheel : MonoBehaviour
{
    public Target RHand;
    public Target LHand;

    public Transform[] RPoints;
    public Transform[] LPoints;

    void Start()
    {
        // if (!FindAnyObjectByType<Garage_Camera>())
        // {
            StartCoroutine(SetPoints());   
        // }
    }

    IEnumerator SetPoints()
    {
        yield return new WaitForSeconds(.5f);

        if (!RHand || !LHand)
            yield break;

        if (RPoints == null || RPoints.Length == 0 || !RPoints[0] ||
            LPoints == null || LPoints.Length == 0 || !LPoints[0]) {
            Debug.LogWarning($"Steering wheel '{name}' has incomplete hand target points.", this);
            yield break;
        }

        RHand.PointTarget = RPoints[0];
        LHand.PointTarget = LPoints[0];
    }
}
