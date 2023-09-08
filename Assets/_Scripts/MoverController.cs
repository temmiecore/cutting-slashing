using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverController : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] CameraController cameraScript;
    public Animator sword;
    [Range(0.1f, 9f)] [SerializeField] float sensitivity = 2f;
    Vector2 rotation = Vector2.zero;
    const string xAxis = "Mouse X"; 
    const string yAxis = "Mouse Y";
    float yRotationLimit = 88f;


    [HideInInspector] public Vector3 movement;
    [HideInInspector] public Vector3 originForward;
    public float speed = 1f;


    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        originForward = transform.forward;
    }

    private void FixedUpdate()
    {
        transform.Translate(movement * speed * Time.deltaTime);
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        movement = new Vector3(x, 0, z);

        rotation.x += Input.GetAxis(xAxis) * sensitivity;
        rotation.y += Input.GetAxis(yAxis) * sensitivity;
        rotation.y = Mathf.Clamp(rotation.y, -yRotationLimit, yRotationLimit);

        var xQuat = Quaternion.AngleAxis(rotation.x, Vector3.up);
        var yQuat = Quaternion.AngleAxis(rotation.y, Vector3.left);
        cam.transform.localRotation = xQuat * yQuat;
        transform.eulerAngles = new Vector3(0, Camera.main.transform.eulerAngles.y, 0);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            sword.SetTrigger("Attack1");
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            sword.SetTrigger("Attack2");
        }
    }
}
