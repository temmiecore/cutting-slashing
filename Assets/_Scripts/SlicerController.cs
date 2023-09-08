using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicerController: MonoBehaviour
{
    public Transform slicerTip;

    private Vector3 sliceStartingPoint, sliceTipPoint, sliceExitPoint;

    private void OnTriggerEnter(Collider other)
    {
        sliceStartingPoint = transform.position;
    }

    private void OnTriggerExit(Collider other)
    {
        sliceExitPoint = transform.position; 
        sliceTipPoint = other.GetComponent<Collider>().ClosestPointOnBounds(slicerTip.position);

        Vector3 vect1 = sliceExitPoint - sliceStartingPoint;
        Vector3 vect2 = sliceTipPoint - sliceStartingPoint;

        Vector3 normal = Vector3.Cross(vect1, vect2).normalized;

        /// Because sliceStartingPoint is a local point attached to slicer, not a point in world coordinates
        Vector3 transformedStartingPoint = other.gameObject.transform.InverseTransformPoint(sliceStartingPoint);

        Plane plane = new Plane(normal, transformedStartingPoint);

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
