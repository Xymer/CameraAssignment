using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBoom : MonoBehaviour
{
    

    [Header("Camera boom settings"), SerializeField]
    float cameraBoomLength = 10.0f;
    [SerializeField] float maxCameraBoomLength = 10.0f;
    [SerializeField] float minCameraBoomLength = 2.0f;
    [SerializeField] float boomSmoothening = 0.75f;

    [SerializeField] float lerpMultiplier = 50.0f;
    [SerializeField] float cameraReturnSmoothening = 0.05f;

    [SerializeField] float mouseAxisYMax = 45.0f;
    [SerializeField] float mouseAxisYMin = -10.0f;

    float targetBoomLength = 10f;
    float cameraSphereRadius = 1f;
    float distanceToLockCamera = 1.0f;
    bool isReturning = false;
    bool isOverridingLength = false;

    [SerializeField] Vector3 cameraBoomOffset = Vector3.zero;
    [SerializeField] Vector3 cameraOffset = Vector3.zero;

    [Header("Scroll"), SerializeField]
    float scrollWheelMultiplier = 10.0f;

    [SerializeField] float mouseMultiplier = 10.0f;
    private float scrollWheelInput = 0.0f;

    float mouseAxisX = 0;
    float mouseAxisY = 0;



    Vector3 endPosition;
    Camera camera = null;
    [SerializeField]
    GameObject cameraTarget;
    void Start()
    {
        camera = GetComponentInChildren<Camera>();
        camera.transform.position = cameraBoomOffset;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        targetBoomLength = cameraBoomLength;
    }

    // Update is called once per frame
    void Update()
    {
        if (LinecastToTarget())
        {
            OverrideCameraBoomLength(Vector3.Distance(RaycastFromTargetToCamera(), endPosition));
        }
        else if (!LinecastToTarget())
        {
            ReturnToTargetBoomLength();
        }

        if (GetScrollInput() != 0)
        {
            SetCameraBoomLength();
        }
        if (!GetRightMouseInput())
        {
            UpdateEndPosition(cameraTarget);
            SetCameraPosition(cameraTarget);
            RotateCameraBoom(cameraTarget);
            LerpMouseAxisBackToZero();
        }

        if (GetRightMouseInput())
        {
            UpdateEndPosition(cameraTarget);
            SetFreeCameraPosition(cameraTarget);
        }

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(endPosition, 1.0f);
        SetupCameraSphere(camera.transform.position, cameraSphereRadius);
        if (LinecastToTarget())
        {
            Gizmos.DrawCube(RaycastFromTargetToCamera(), Vector3.one);
        }
    }

    void SetupCameraSphere(Vector3 cameraPosition, float radius)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(cameraPosition, radius);
    }
    void UpdateEndPosition(GameObject target)
    {
        endPosition = target.transform.position + cameraBoomOffset;
    }
    void SetCameraPosition(GameObject target)
    {
        transform.position = target.transform.position;
        camera.transform.position = Vector3.Lerp(camera.transform.position, endPosition - transform.forward * cameraBoomLength + cameraOffset, Time.fixedDeltaTime * lerpMultiplier);
    }
    void RotateCameraBoom(GameObject target)
    {
        if (isReturning && Vector3.Distance(transform.rotation.eulerAngles, target.transform.rotation.eulerAngles) < distanceToLockCamera) // Decides if it should lock the camera behind the camera target
        {
            isReturning = false;
        }
        if (!isReturning)
        {
            transform.rotation = target.transform.rotation;
        }
        else if (isReturning)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, target.transform.rotation, Time.deltaTime * lerpMultiplier * cameraReturnSmoothening);
        }
        camera.transform.LookAt(endPosition);
    }
    void SetFreeCameraPosition(GameObject target)
    {
        SetCameraPosition(target);
        Vector3 mouseMovement = new Vector3(mouseAxisX, Mathf.Clamp(mouseAxisY, mouseAxisYMin, mouseAxisYMax), 0);
        mouseAxisX += Input.GetAxis("Mouse X") * mouseMultiplier;
        mouseAxisY -= Input.GetAxis("Mouse Y") * mouseMultiplier;

        mouseAxisY = Mathf.Clamp(mouseAxisY, mouseAxisYMin, mouseAxisYMax);
        Quaternion rotation = Quaternion.Euler(mouseMovement.y + target.transform.rotation.eulerAngles.x, mouseMovement.x + target.transform.rotation.eulerAngles.y, 0.0f);

        camera.transform.LookAt(endPosition);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, lerpMultiplier * Time.deltaTime);
        isReturning = true;
    }
    void SetCameraBoomLength()
    {
        float currentBoomLength = cameraBoomLength;
        currentBoomLength += -GetScrollInput() * scrollWheelMultiplier;
        cameraBoomLength = Mathf.Lerp(cameraBoomLength, Mathf.Clamp(currentBoomLength, minCameraBoomLength, maxCameraBoomLength), lerpMultiplier * Time.deltaTime);
        targetBoomLength = cameraBoomLength;
    }
    void OverrideCameraBoomLength(float length)
    {
        if (length < targetBoomLength)
        {
            cameraBoomLength = Mathf.Lerp(cameraBoomLength, length, lerpMultiplier * Time.deltaTime * boomSmoothening);
            isOverridingLength = true;
        }
    }
    void ReturnToTargetBoomLength()
    {
        cameraBoomLength = Mathf.Lerp(cameraBoomLength, targetBoomLength, lerpMultiplier * Time.deltaTime * boomSmoothening);
        if (cameraBoomLength == targetBoomLength)
        {
            isOverridingLength = false;
        }
    }
    float GetScrollInput()
    {
        return Input.GetAxis("Mouse ScrollWheel");
    }
    void LerpMouseAxisBackToZero()
    {
        mouseAxisX = Mathf.Lerp(mouseAxisX, 0, Time.deltaTime * lerpMultiplier * cameraReturnSmoothening);
        mouseAxisY = Mathf.Lerp(mouseAxisY, 0, Time.deltaTime * lerpMultiplier * cameraReturnSmoothening);
    }
    bool GetRightMouseInput()
    {
        return Input.GetMouseButton(1);
    }
    bool LinecastToTarget()
    {
        int playerLayer = 1 << 8;
        playerLayer = ~playerLayer;
        Debug.DrawLine(camera.transform.position - camera.transform.forward * 5f, endPosition, Color.red);

        return Physics.Linecast(camera.transform.position - camera.transform.forward * 5f, endPosition, playerLayer);
    }
    Vector3 RaycastFromTargetToCamera()
    {

        RaycastHit hitInfo;
        Physics.Linecast(endPosition, camera.transform.position - camera.transform.forward * 5f, out hitInfo);
        Debug.DrawLine(endPosition, camera.transform.position - camera.transform.forward);
        return hitInfo.point;
    }
}
