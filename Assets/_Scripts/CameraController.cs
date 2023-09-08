using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] MoverController target;
    [Range(0f, 1f)] public float cameraHeight;

    void Update()
    {
            transform.position = new Vector3(target.transform.position.x, target.transform.position.y + cameraHeight, target.transform.position.z);
    }
}