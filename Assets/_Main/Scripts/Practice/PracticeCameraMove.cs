using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeCameraMove : CustomMonoBehaviour
{
    [SerializeField]
    private float speedCamera = 3;
    [SerializeField]
    private float mouseSensivity = 100f;
    [SerializeField]
    private GameObject camera;
    private float xRotation = 0;

    // Update is called once per frame
    void Update()
    {
        Movement();
        if (Input.GetMouseButton(1))
        {
            //Cursor.lockState = CursorLockMode.Locked;
            MouseLook();
        }

    }

    private void Movement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = camera.transform.right * horizontal + camera.transform.forward * vertical;
        transform.parent.position += direction.normalized * speedCamera * Time.deltaTime;
    }
    private void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
        camera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.parent.Rotate(Vector3.up * mouseX);
    }



    //chạy khi nhấn nút reset trên editor không liên quan đến game chỉ hỗ trợ kéo thả
    #region editor
    protected override void LoadComponent()
    {
        base.LoadComponent();
        camera = transform.parent.transform.Find("Main Camera").gameObject;
    }
    protected override void ResetValue()
    {
        base.ResetValue();
        speedCamera = 3;

    }
    #endregion
}
