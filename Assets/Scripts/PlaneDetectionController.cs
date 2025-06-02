using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneDetectionController : MonoBehaviour
{
    private ARPlaneManager planeManager;
    public BirdController birdController;
    [SerializeField]
    private float accumulatedPlaneSize = 0.0f;
    private Dictionary<TrackableId, Vector2> planeSizeLookup = new Dictionary<TrackableId, Vector2>();


    private void Awake()
    {
        planeManager = GetComponent<ARPlaneManager>();
        if (planeManager == null)
        {
            Debug.LogError("ARPlaneManager component not found!");
        }
        else
        {
            Debug.Log("ARPlaneManager found and initialized.");
        }
    }

    private void OnEnable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
            Debug.Log("Subscribed to planesChanged event.");
        }
    }

    private void OnDisable()
    {
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
            Debug.Log("Unsubscribed from planesChanged event.");
        }
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Log added planes
        if (args.added != null && args.added.Count > 0)
        {
            foreach (var plane in args.added)
            {
                var size = plane.size;
                float area = size.x * size.y;

                planeSizeLookup[plane.trackableId] = size;
                accumulatedPlaneSize += area;

                Debug.Log($"Plane added: ID={plane.trackableId}, Position={plane.transform.position}, Classification={plane.classification}, Size={plane.size}");
            }
        }

        // Log updated planes
        if (args.updated != null && args.updated.Count > 0)
        {
            foreach (var plane in args.updated)
            {
                var id = plane.trackableId;
                var newSize = plane.size;
                float newArea = newSize.x * newSize.y;

                if (planeSizeLookup.TryGetValue(id, out var oldSize))
                {
                    float oldArea = oldSize.x * oldSize.y;
                    accumulatedPlaneSize -= oldArea;
                }

                accumulatedPlaneSize += newArea;
                planeSizeLookup[id] = newSize;

                Debug.Log($"Plane updated: ID={plane.trackableId}, Position={plane.transform.position}, Classification={plane.classification}, Size={plane.size}");
            }
        }

        // Log removed planes
        if (args.removed != null && args.removed.Count > 0)
        {
            foreach (var plane in args.removed)
            {
                var id = plane.trackableId;

                if (planeSizeLookup.TryGetValue(id, out var oldSize))
                {
                    float oldArea = oldSize.x * oldSize.y;
                    accumulatedPlaneSize -= oldArea;
                    planeSizeLookup.Remove(id);
                }

                Debug.Log($"Plane removed: ID={plane.trackableId}");
            }
        }

        if (!birdController.sizeThresholdMet)
        {
            birdController.CheckAccumulatedPlaneSize(accumulatedPlaneSize);
        }

        // Log total number of active planes
        int totalPlanes = planeManager.trackables.count;
        Debug.Log($"Total active planes: {totalPlanes}");
    }

    // Optional: Method to manually check current planes (can be called from another script or UI)
    public void LogCurrentPlanes()
    {
        if (planeManager == null)
        {
            Debug.LogWarning("Cannot log planes: ARPlaneManager is null.");
            return;
        }

        int totalPlanes = planeManager.trackables.count;
        if (totalPlanes == 0)
        {
            Debug.Log("No planes currently detected.");
            return;
        }

        foreach (var plane in planeManager.trackables)
        {
            Debug.Log($"Current plane: ID={plane.trackableId}, Position={plane.transform.position}, Classification={plane.classification}, Size={plane.size}");
        }
        Debug.Log($"Total current planes: {totalPlanes}");
    }
}