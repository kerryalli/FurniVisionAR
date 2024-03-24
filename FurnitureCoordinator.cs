using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FurnitureCoordinator : MonoBehaviour
{
    // Reference to the prefab of deployable furniture
    public GameObject DeployableFurniture;

    // Reference to the XR origin
    public XROrigin xrOrigin;

    // Reference to the ARRaycastManager for raycasting
    public ARRaycastManager raycastManager;

    // Reference to the ARPlaneManager for managing planes
    public ARPlaneManager planeManager;

    // List to hold ARRaycastHits
    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

    // Variables for pinch zooming
    private Vector2 touchStartPos;
    private float initialPinchDistance;
    private bool isZooming;

    // Update is called once per frame
    private void Update()
    {
        // Handle pinch zooming
        HandlePinchZoom();

        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                // Perform raycasting to detect planes
                if (raycastManager.Raycast(Input.GetTouch(0).position, raycastHits, TrackableType.PlaneWithinPolygon))
                {
                    // Check if a UI button is pressed
                    if (!IsButtonPressed())
                    {
                        // Instantiate deployable furniture at the hit position and rotation
                        GameObject gameObject = Instantiate(DeployableFurniture);
                        gameObject.transform.position = raycastHits[0].pose.position;
                        gameObject.transform.rotation = raycastHits[0].pose.rotation;
                    }

                    // Disable all detected planes
                    foreach (var plane in planeManager.trackables)
                    {
                        plane.gameObject.SetActive(false);
                    }

                    // Disable plane detection
                    planeManager.enabled = false;
                }
            }
        }
    }

    // Handle pinch zooming of the selected object
    private void HandlePinchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                // Calculate initial pinch distance
                initialPinchDistance = Vector2.Distance(touchZero.position, touchOne.position);
                isZooming = true;
            }
            else if (touchZero.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Ended)
            {
                // End zooming when fingers are lifted
                isZooming = false;
            }

            if (isZooming)
            {
                // Calculate pinch difference and adjust object scale
                float currentPinchDistance = Vector2.Distance(touchZero.position, touchOne.position);
                float pinchDifference = currentPinchDistance - initialPinchDistance;
                DeployableFurniture.transform.localScale += Vector3.one * pinchDifference * Time.deltaTime;
                initialPinchDistance = currentPinchDistance;
            }
        }
    }

    // Check if a UI button is currently pressed
    private bool IsButtonPressed()
    {
        GameObject currentSelectedObject = EventSystem.current.currentSelectedGameObject;

        // Return true if a UI button is pressed, otherwise false
        return currentSelectedObject != null && currentSelectedObject.GetComponent<UnityEngine.UI.Button>() != null;
    }

    // Switch the deployable furniture prefab
    public void SwitchFurniture(GameObject furniture)
    {
        DeployableFurniture = furniture;
    }
}

