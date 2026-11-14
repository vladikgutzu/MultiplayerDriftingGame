using UnityEngine;
using UnityEngine.EventSystems;

public class DroneModeController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    // Speed of camera movement
    public float movementSpeed = 5f;

    // Sensitivity of touch input
    public float touchSensitivity = 0.1f;

    // Zoom speed
    public float zoomSpeed = 1f;

    // Smoothness of zooming
    public float zoomSmoothness = 5f;

    // Maximum and minimum FOV
    public float maxFOV = 60f;
    public float minFOV = 20f;

    // Variables to store touch input positions
    private Vector2 touchStartPos;
    private Vector2 touchDelta;

    // Store if the camera is being dragged for rotation
    private bool isRotating = false;

    // Store if the camera should move forward
    private bool isMovingForward = false;

    // Store if the camera should move backward
    private bool isMovingBackward = false;

    // Store if the camera should zoom in
    private bool isZoomingIn = false;

    // Store if the camera should zoom out
    private bool isZoomingOut = false;

    // Store if the camera should move up
    private bool isMovingUp = false;

    // Store if the camera should move down
    private bool isMovingDown = false;

    private float targetFOV;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        targetFOV = cam.fieldOfView;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isRotating)
        {
            // Calculate touch delta for rotation
            touchDelta = eventData.delta;

            // Update camera rotation based on touch delta
            float yaw = touchDelta.x * touchSensitivity;
            float pitch = -touchDelta.y * touchSensitivity;

            transform.Rotate(Vector3.up, yaw);
            transform.Rotate(Vector3.right, pitch);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        switch (eventData.pointerPress.tag)
        {
            case "RotateButton":
                StartRotating();
                break;
            case "MoveForwardButton":
                StartMovingForward();
                break;
            case "MoveBackwardButton":
                StartMovingBackward();
                break;
            case "ZoomInButton":
                StartZoomingIn();
                break;
            case "ZoomOutButton":
                StartZoomingOut();
                break;
            case "MoveUpButton":
                StartMovingUp();
                break;
            case "MoveDownButton":
                StartMovingDown();
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        switch (eventData.pointerPress.tag)
        {
            case "RotateButton":
                StopRotating();
                break;
            case "MoveForwardButton":
                StopMovingForward();
                break;
            case "MoveBackwardButton":
                StopMovingBackward();
                break;
            case "ZoomInButton":
                StopZoomingIn();
                break;
            case "ZoomOutButton":
                StopZoomingOut();
                break;
            case "MoveUpButton":
                StopMovingUp();
                break;
            case "MoveDownButton":
                StopMovingDown();
                break;
        }
    }

    void Update()
    {
        // Handle camera movement
        Vector3 movement = Vector3.zero;
        if (isMovingForward)
        {
            movement += transform.forward;
        }
        else if (isMovingBackward)
        {
            movement -= transform.forward;
        }
        if (isMovingUp)
        {
            movement += Vector3.up;
        }
        else if (isMovingDown)
        {
            movement -= Vector3.up;
        }
        movement.Normalize();
        transform.Translate(movement * movementSpeed * Time.deltaTime, Space.World);

        // Smoothly adjust FOV for zooming in and out
        if (isZoomingIn && targetFOV > minFOV)
        {
            targetFOV -= zoomSpeed * Time.deltaTime * zoomSmoothness;
        }
        else if (isZoomingOut && targetFOV < maxFOV)
        {
            targetFOV += zoomSpeed * Time.deltaTime * zoomSmoothness;
        }

        // Clamp target FOV within min and max FOV
        targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);

        // Apply the smoothed FOV to the camera
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSmoothness);
    }

    // Methods for starting actions
    void StartRotating()
    {
        isRotating = true;
        touchStartPos = Input.mousePosition;
    }

    void StartMovingForward()
    {
        isMovingForward = true;
    }

    void StartMovingBackward()
    {
        isMovingBackward = true;
    }

    void StartZoomingIn()
    {
        isZoomingIn = true;
    }

    void StartZoomingOut()
    {
        isZoomingOut = true;
    }

    void StartMovingUp()
    {
        isMovingUp = true;
    }

    void StartMovingDown()
    {
        isMovingDown = true;
    }

    // Methods for stopping actions
    void StopRotating()
    {
        isRotating = false;
    }

    void StopMovingForward()
    {
        isMovingForward = false;
    }

    void StopMovingBackward()
    {
        isMovingBackward = false;
    }

    void StopZoomingIn()
    {
        isZoomingIn = false;
    }

    void StopZoomingOut()
    {
        isZoomingOut = false;
    }

    void StopMovingUp()
    {
        isMovingUp = false;
    }

    void StopMovingDown()
    {
        isMovingDown = false;
    }
}
