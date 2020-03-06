using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBoom : MonoBehaviour
{
    [Header("Camera boom settings"), SerializeField]
    public float cameraBoomLength = 10.0f;
    public float maxCameraBoomLength = 10.0f;
    public float minCameraBoomLength = 2.0f;

    public float lerpTime = 1.0f;
    public float cameraReturnSmoothening = 0.05f;

    public float mouseAxisYMax = 45.0f;
    public float mouseAxisYMin = -10.0f;

    private bool isReturning = false;
    private float distanceToReturn = 1.0f;
  
    public Vector3 cameraBoomOffset = Vector3.zero;
    public Vector3 cameraOffset = Vector3.zero;
    [System.NonSerialized]
    float currentCameraBoomLength;

    [Header("Scroll"), SerializeField]
    float scrollWheelMultiplier = 10.0f;
    [SerializeField]
    float mouseMultiplier = 10.0f;
    float scrollWheelInput = 0.0f;

    float mouseAxisX = 0;
    float mouseAxisY = 0;



    Vector3 endPosition;
    Camera camera;
    [SerializeField]
    GameObject cameraTarget;
    void Start()
    {
        camera = GetComponentInChildren<Camera>();
        camera.transform.position = cameraBoomOffset;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Physics.Raycast(camera.transform.position, endPosition);
        Debug.DrawLine(camera.transform.position, endPosition, Color.red);
        if (GetScrollInput() != 0)
        {
            SetCameraBoomLength();
        }
        if (!GetRightMouseInput())
        {
            UpdateEndPosition(cameraTarget);
            SetCameraPosition(cameraTarget);
            RotateCameraBoom(cameraTarget);
            LerpMouseAxis();
        }

        if (GetRightMouseInput())
        {

            UpdateEndPosition(cameraTarget);
            SetFreeCameraPosition(cameraTarget);
        }
        currentCameraBoomLength = Vector3.Distance(camera.transform.position, cameraTarget.transform.position);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(endPosition, 1.0f);
        SetupCameraSphere(camera.transform.position, 1.0f);
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
        camera.transform.position = Vector3.Lerp(camera.transform.position, endPosition - transform.forward * cameraBoomLength + cameraOffset, Time.fixedDeltaTime * lerpTime);
    }
    void RotateCameraBoom(GameObject target)
    {   
        if (isReturning && Vector3.Distance(transform.rotation.eulerAngles, target.transform.rotation.eulerAngles) < distanceToReturn)
        {
            isReturning = false;
        }
        if (!isReturning)
        {
            transform.rotation = target.transform.rotation;
        }
        else if (isReturning)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, target.transform.rotation, Time.deltaTime * lerpTime * cameraReturnSmoothening);

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

        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, lerpTime * Time.deltaTime);
        camera.transform.LookAt(endPosition);
        isReturning = true;
    }
    void SetCameraBoomLength()
    {
        float currentBoomLength = cameraBoomLength;
        currentBoomLength += -GetScrollInput() * scrollWheelMultiplier;
        cameraBoomLength = Mathf.Clamp(currentBoomLength, minCameraBoomLength, maxCameraBoomLength);
    }
    float GetScrollInput()
    {
        return Input.GetAxis("Mouse ScrollWheel");
    }
    void LerpMouseAxis()
    {
        mouseAxisX = Mathf.Lerp(mouseAxisX, 0, Time.deltaTime * lerpTime * cameraReturnSmoothening);
        mouseAxisY = Mathf.Lerp(mouseAxisY, 0, Time.deltaTime * lerpTime * cameraReturnSmoothening);
    }
    bool GetRightMouseInput()
    {
        return Input.GetMouseButton(1);
    }
}
