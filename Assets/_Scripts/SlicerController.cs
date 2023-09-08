using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicerController: MonoBehaviour
{
    public Transform slicerTip;

    public bool isCutting;

    private Vector3 sliceStartingPoint, sliceTipPoint, sliceExitPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!isCutting)
            return;

        sliceStartingPoint = transform.position;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isCutting)
            return;

        sliceExitPoint = transform.position; 
        sliceTipPoint = other.GetComponent<Collider>().ClosestPointOnBounds(slicerTip.position);

        Vector3 vect1 = sliceExitPoint - sliceStartingPoint;
        Vector3 vect2 = sliceTipPoint - sliceStartingPoint;

        Vector3 normal = Vector3.Cross(vect1, vect2).normalized;

        /// Wouldn't figure this one out in 10 years. shoutout to tvtig
        Vector3 transformedNormal = ((Vector3)(other.gameObject.transform.localToWorldMatrix.transpose * normal)).normalized;

        /// Because sliceStartingPoint is a local point attached to slicer, not a point in world coordinates
        Vector3 transformedStartingPoint = other.gameObject.transform.InverseTransformPoint(sliceStartingPoint);

        Plane plane = new Plane(transformedNormal, transformedStartingPoint);

        Slicer slicer = new Slicer(other.GetComponent<Sliceable>(), plane);
        slicer.Slice();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(sliceStartingPoint, 0.05f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(sliceTipPoint, 0.05f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(sliceExitPoint, 0.05f);
    }
}
