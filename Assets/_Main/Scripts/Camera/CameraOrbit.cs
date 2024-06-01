using System;
using TMPro;
using UnityEngine;

namespace TDLN.CameraControllers
{
    public class CameraOrbit : MonoBehaviour
    {
        private bool _canControl = false;
        new public Camera camera;
        public GameObject target;
        public float distance = 10.0f;
        public float movementSmoothTime = 0.05f;

        public float xSpeed = 250.0f;
        public float ySpeed = 120.0f;

        public float yMinLimit = -20;
        public float yMaxLimit = 80;
        public bool isTest = true;
        float x = 0.0f;
        float y = 0.0f;

        private Vector3 moveVelocity;
        private Vector3 targetPosition;
        void Start()
        {
            target = new GameObject(" ");
            target.transform.position += Vector3.up * 3f;
            var angles = transform.eulerAngles;
            x = angles.y;
            y = angles.x;
            this.AddEvent(EventKey.REQUEST_UPDATE_CAMERA_TARGET, MoveTargetToBoundsCenter);
            this.AddEvent(EventKey.REQUEST_ENABLE_ANIMATION_CAMERA, EnableCamera);
            this.AddEvent(EventKey.REQUEST_DISABLE_ANIMATION_CAMERA, DisableCamera);
            DisableCamera();
        }
        private void OnDestroy()
        {
            this.DropEvent(EventKey.REQUEST_UPDATE_CAMERA_TARGET, MoveTargetToBoundsCenter);
            this.DropEvent(EventKey.REQUEST_ENABLE_ANIMATION_CAMERA, EnableCamera);
            this.DropEvent(EventKey.REQUEST_DISABLE_ANIMATION_CAMERA, DisableCamera);
        }

        float prevDistance;
        void EnableCamera()
        {
            camera.enabled = true;
            _canControl = true;
        }
        void DisableCamera()
        {
            if (isTest)
            {
                camera.enabled = true;
                _canControl = true;
                return;
            }
            camera.enabled = false;
            _canControl = false;
        }
        void LateUpdate()
        {
            if (!_canControl)
            {
                return;
            }
            if (distance < 2) distance = 2;
            float zoomFactor = Input.GetAxis("Mouse ScrollWheel") * 2;
            distance -= zoomFactor;
            float currentSmoothTimeThisFrame = zoomFactor != 0 ? 0 : movementSmoothTime;
            if (target)
            {
                var pos = Input.mousePosition;
                var dpiScale = 1f;
                if (Screen.dpi < 200) dpiScale = 1;
                else dpiScale = Screen.dpi / 200f;

                if (pos.x < 380 * dpiScale && Screen.height - pos.y < 250 * dpiScale) return;
                if (Input.GetMouseButton(1))
                {
                    // comment out these two lines if you don't want to hide mouse curser or you have a UI button 
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;

                    x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                    y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                }
                else
                {
                    // comment out these two lines if you don't want to hide mouse curser or you have a UI button 
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                y = ClampAngle(y, yMinLimit, yMaxLimit);
                var rotation = Quaternion.Euler(y, x, 0);
                targetPosition = Vector3.SmoothDamp(targetPosition, target.transform.position, ref moveVelocity, currentSmoothTimeThisFrame);
                var position = rotation * new Vector3(0.0f, 0.0f, -distance) + targetPosition;
                transform.rotation = rotation;
                transform.position = position;

            }
            else
            {

            }

            if (Math.Abs(prevDistance - distance) > 0.001f)
            {
                prevDistance = distance;
                var rot = Quaternion.Euler(y, x, 0);
                var po = rot * new Vector3(0.0f, 0.0f, -distance) + target.transform.position;
                transform.rotation = rot;
                transform.position = po;
            }
        }
        private void MoveTargetToBoundsCenter(object o)
        {
            var bounds = (Bounds)o;
            target.transform.position = bounds.center;
        }
        static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
    }
}