using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBoom : MonoBehaviour
{
    private const string mouseAxisX = "Mouse X";
    private const string mouseAxisY = "Mouse Y";
    private const string scrollWheel = "Mouse ScrollWheel";


    [Header("Camera boom settings"), SerializeField]
    float cameraBoomLength = 10.0f;
    [SerializeField] float maxCameraBoomLength = 10.0f;
    [SerializeField] float minCameraBoomLength = 2.0f;
    [SerializeField, Range(0f, 1f)] float boomSmoothening = 0.75f;

    [SerializeField] float lerpMultiplier = 50.0f;
    [SerializeField, Range(0f, 1f)] float cameraReturnSmoothening = 0.05f;
    [SerializeField, Range(0f, 1f)] float movementLerpSmoothening = 0.5f;

    [SerializeField] float mouseAxisYMax = 45.0f;
    [SerializeField] float mouseAxisYMin = -5.0f;
    [SerializeField, Tooltip("Minimum angle for target while turning")]float minTurnRotation = -10f;
    [SerializeField, Tooltip("Maximum angle for target while turning")]float maxTurnRotation = 10f;
    float mouseAxisYValue = 0f;
    float mouseAxisXValue = 0f;
    float freeCameraAxisXValue = 0;

    float targetBoomLength = 10f;
    float cameraSphereRadius = 1f;
    float distanceToLockCamera = 5f;
    bool isReturning = false;
    bool isOverridingLength = false;

    [SerializeField, Tooltip("Offset from camera boom transform")] Vector3 cameraBoomOffset = Vector3.zero;
    [SerializeField, Tooltip("Offset for camera")] Vector3 cameraOffset = Vector3.zero;
    Vector3 mouseOffset = Vector3.zero;
    Vector3 linetraceHitpoint;
    [Header("Mouse Settings"), SerializeField]
    float scrollWheelMultiplier = 10.0f;
    [SerializeField] float mouseMultiplier = 10.0f;

    Vector3 endPosition;
    Camera camera = default;
    [SerializeField]
    GameObject cameraTarget;
    void Start()
    {
        camera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        targetBoomLength = cameraBoomLength;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (LinecastFromTargetToCamera(ref linetraceHitpoint))
        {
            OverrideCameraBoomLength(Vector3.Distance(endPosition, linetraceHitpoint));
        }
        else
        {
            ReturnToTargetBoomLength();
        }       

        if (GetScrollInput() != 0)
        {
            SetCameraBoomLength();
        }

        UpdateMouseValues();

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
        Gizmos.DrawWireSphere(endPosition, cameraSphereRadius);
        if (camera != null)
        {
        SetupCameraSphere(camera.transform.position, cameraSphereRadius);
        }
    }

    void SetupCameraSphere(Vector3 cameraPosition, float radius)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(cameraPosition, radius);
    }
    void UpdateEndPosition(GameObject target)
    {
        endPosition = Vector3.Lerp(endPosition, target.transform.position + cameraBoomOffset + mouseOffset, Time.deltaTime * lerpMultiplier * movementLerpSmoothening);
    }
    void SetCameraPosition(GameObject target)
    {
        transform.position = target.transform.position;
        camera.transform.position = Vector3.Lerp(camera.transform.position, endPosition - transform.forward * cameraBoomLength + cameraOffset, Time.fixedDeltaTime * lerpMultiplier);
    }
    void RotateCameraBoom(GameObject target)
    {
        float lockValue = target.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y;
        Debug.Log(Mathf.Abs(lockValue));
        if (Mathf.Abs(lockValue) < distanceToLockCamera) // Decides if it should lock the camera behind the camera target
        {
            isReturning = false;
        }
        if (!isReturning)
        {
            mouseAxisYValue -= Input.GetAxis(mouseAxisY);
            mouseAxisXValue += Input.GetAxis(mouseAxisX);
            mouseAxisYValue = Mathf.Clamp(mouseAxisYValue, mouseAxisYMin, mouseAxisYMax);
            Quaternion rotation = Quaternion.Euler(Mathf.Clamp(mouseAxisYValue + target.transform.rotation.eulerAngles.x, mouseAxisYMin, mouseAxisYMax),
                target.transform.rotation.eulerAngles.y + -Mathf.Clamp(mouseAxisXValue, minTurnRotation, maxTurnRotation), 0f);
            transform.rotation = rotation;
        }
        else if (isReturning)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(target.transform.rotation.eulerAngles.x + mouseAxisYValue, target.transform.rotation.eulerAngles.y,
                target.transform.rotation.eulerAngles.z), Time.deltaTime * lerpMultiplier * cameraReturnSmoothening);
        }
        camera.transform.LookAt(endPosition);
    }
    void SetFreeCameraPosition(GameObject target)
    {
        
        SetCameraPosition(target);
        Vector3 mouseMovement = new Vector3(freeCameraAxisXValue, Mathf.Clamp(mouseAxisYValue, mouseAxisYMin, mouseAxisYMax), 0);
        freeCameraAxisXValue += Input.GetAxis(mouseAxisX) * mouseMultiplier;

        Quaternion rotation = Quaternion.Euler(mouseMovement.y + target.transform.rotation.eulerAngles.x,mouseMovement.x + target.transform.rotation.eulerAngles.y, 0.0f);
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
        if (isOverridingLength)
        {
            cameraBoomLength = Mathf.Lerp(cameraBoomLength, targetBoomLength, lerpMultiplier * Time.deltaTime * boomSmoothening);
        }

        if (Mathf.Approximately(cameraBoomLength, targetBoomLength))
        {
            isOverridingLength = false;
        }
    }
    float GetScrollInput()
    {
        return Input.GetAxis(scrollWheel) * Mathf.Abs(Input.GetAxis(scrollWheel));
    }
    void UpdateMouseValues()
    {
        mouseAxisYValue -= Input.GetAxis(mouseAxisY);
        mouseAxisXValue += Input.GetAxis(mouseAxisX);
        mouseAxisYValue = Mathf.Clamp(mouseAxisYValue, mouseAxisYMin, mouseAxisYMax);
    }
    void LerpMouseAxisBackToZero()
    {
        //Regularcam axis
        if (Input.GetAxis(mouseAxisX) == 0 || Input.GetAxis(mouseAxisY) == 0)
        {
            mouseAxisXValue = Mathf.Lerp(mouseAxisXValue, 0, Time.deltaTime * lerpMultiplier * cameraReturnSmoothening);
        }
        //Freecam axis
        freeCameraAxisXValue = Mathf.Lerp(freeCameraAxisXValue, 0, Time.deltaTime * lerpMultiplier * cameraReturnSmoothening);    
    }
    bool GetRightMouseInput()
    {
        return Input.GetMouseButton(1);
    }

    bool LinecastFromTargetToCamera(ref Vector3 hitPoint)
    {
        RaycastHit hitInfo;
        bool hasHitObject = Physics.Linecast(endPosition, camera.transform.position - camera.transform.forward * 2f, out hitInfo);
        Debug.DrawLine(endPosition, camera.transform.position - camera.transform.forward * 2f);

        if (hasHitObject)
        {
            hitPoint = hitInfo.point;
        }
        return hasHitObject;
    }

    /// <summary>
    /// Switch target to follow
    /// </summary>
    /// <param name="target"></param>
    void ChangeCameraTarget(GameObject target)
    {
        cameraTarget = target;
    }
}
