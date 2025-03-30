using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerCam1 : MonoBehaviour
{

    public float sensx = 400f;
    public float sensy = 400f;

    public float diff;

    public Transform orientation;

    public Transform Model;

    float xRotation;
    float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //geting input

        float mousex = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensx;
        float mousey = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensy;

        yRotation += mousex;
        xRotation -= mousey;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //rotate cam and orintation

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0); 
        Model.rotation = Quaternion.Euler(0, yRotation - diff, 0); 

    }
}
