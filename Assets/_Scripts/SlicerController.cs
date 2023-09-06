using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicerController: MonoBehaviour
{
    public Transform slicerTip;

    private Vector3 sliceStartingPoint, sliceTipPoint, sliceExitPoint;

    private void OnTriggerEnter(Collider other)
    {
        sliceStartingPoint = other.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
    }

    private void OnTriggerExit(Collider other)
    {
        sliceExitPoint = other.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        sliceTipPoint = other.GetComponent<Collider>().ClosestPointOnBounds(slicerTip.position);

        Plane plane = new Plane(Vector3.Cross(sliceStartingPoint, sliceExitPoint).normalized, sliceTipPoint);

        /// Very dumb
        if (plane.GetDistanceToPoint(GameObject.Find("HighPoint").transform.position) < 0)
            plane.Flip();

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
