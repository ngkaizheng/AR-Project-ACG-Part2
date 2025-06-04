using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneDetectionController : MonoBehaviour
{
    private ARPlaneManager planeManager;
    private ARAnchorManager anchorManager;
    public BirdController birdController;
    [SerializeField]
    private float accumulatedPlaneSize = 0.0f;
    private ARRaycastManager raycastManager; // Reference to ARRaycastManager
    private Dictionary<TrackableId, Vector2> planeSizeLookup = new Dictionary<TrackableId, Vector2>();
    private List<Vector3> spawnedItemsPosition = new List<Vector3>();
    private float distanceSpawnedItems = 0.25f;

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
        anchorManager = GetComponent<ARAnchorManager>();
        if (anchorManager == null)
        {
            Debug.LogError("ARAnchorManager component not found!");
        }
        else
        {
            Debug.Log("ARAnchorManager found and initialized.");
        }
        raycastManager = FindObjectOfType<ARRaycastManager>();
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

    #region OnPlanesChanged Event Handler
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
    #endregion

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

    #region Spawn Rocks on Detected Planes
    public void SpawnRocks(int count, GameObject rockPrefab)
    {
        if (rockPrefab == null)
        {
            Debug.LogError("Rock prefab is not assigned in PlaneDetectionController!");
            return;
        }

        if (raycastManager == null)
        {
            Debug.LogError("ARRaycastManager is not assigned in PlaneDetectionController!");
            return;
        }

        if (planeManager == null || planeManager.trackables.count == 0)
        {
            Debug.LogWarning("No planes detected to spawn rocks on.");
            return;
        }

        List<ARPlane> activePlanes = new List<ARPlane>();
        foreach (var plane in planeManager.trackables)
        {
            if (plane.boundary != null && plane.boundary.Count() > 2)
            {
                activePlanes.Add(plane);
            }
            else
            {
                Debug.LogWarning($"Skipping plane {plane.trackableId} due to invalid boundary.");
            }
        }

        if (activePlanes.Count == 0)
        {
            Debug.LogWarning("No valid planes with boundaries to spawn rocks on.");
            return;
        }

        int maxAttempts = count * 10; // Limit attempts to prevent infinite loops
        int rocksSpawned = 0;

        while (rocksSpawned < count && maxAttempts > 0)
        {
            ARPlane selectedPlane = activePlanes[Random.Range(0, activePlanes.Count)];
            Vector3 spawnPosition = GetRandomPointInPlaneBoundary(selectedPlane);

            // Verify the point is on the plane using raycasting
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(spawnPosition);
            if (raycastManager.Raycast(screenPoint, hits, TrackableType.Planes))
            {
                foreach (var hit in hits)
                {
                    if (hit.trackableId == selectedPlane.trackableId)
                    {
                        // Check if the position is too close to existing rocks
                        if (!IsPositionTooClose(hit.pose.position, spawnedItemsPosition, distanceSpawnedItems))
                        {
                            GameObject rock = Instantiate(rockPrefab, hit.pose.position, Quaternion.identity);
                            spawnedItemsPosition.Add(hit.pose.position);
                            rocksSpawned++;
                            Debug.Log($"Rock {rocksSpawned} spawned at {hit.pose.position} on plane {selectedPlane.trackableId}");
                            break;
                        }
                    }
                }
            }

            maxAttempts--;
        }

        if (rocksSpawned < count)
        {
            Debug.LogWarning($"Only spawned {rocksSpawned} out of {count} rocks due to space constraints or invalid positions.");
        }
        else
        {
            Debug.Log($"Successfully spawned {rocksSpawned} rocks on detected planes.");
        }
    }

    private Vector3 GetRandomPointInPlaneBoundary(ARPlane plane)
    {
        NativeArray<Vector2> boundary = plane.boundary;
        if (boundary.Length < 3)
        {
            Debug.LogWarning($"Plane {plane.trackableId} has insufficient boundary points.");
            return plane.center; // Fallback to plane center
        }

        // Find bounding box of the boundary
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        foreach (Vector2 point in boundary)
        {
            minX = Mathf.Min(minX, point.x);
            maxX = Mathf.Max(maxX, point.x);
            minZ = Mathf.Min(minZ, point.y);
            maxZ = Mathf.Max(maxZ, point.y);
        }

        // Generate random points within bounding box and check if inside polygon
        int maxAttempts = 50;
        while (maxAttempts > 0)
        {
            Vector2 randomPoint = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minZ, maxZ)
            );

            if (IsPointInPolygon(randomPoint, boundary))
            {
                Vector3 localPosition = new Vector3(randomPoint.x, 0, randomPoint.y);
                return plane.transform.TransformPoint(localPosition);
            }

            maxAttempts--;
        }

        Debug.LogWarning($"Failed to find valid point in plane {plane.trackableId}. Using plane center as fallback.");
        return plane.center;
    }

    private bool IsPointInPolygon(Vector2 point, NativeArray<Vector2> polygon)
    {
        int crossings = 0;
        for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
        {
            Vector2 a = polygon[j];
            Vector2 b = polygon[i];

            if ((a.y > point.y) != (b.y > point.y) &&
                point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y + 0.0001f) + a.x)
            {
                crossings++;
            }
        }
        return (crossings % 2) == 1; // Odd number of crossings means point is inside
    }

    private bool IsPositionTooClose(Vector3 position, List<Vector3> existingPositions, float minDistance)
    {
        foreach (var existingPosition in existingPositions)
        {
            if (Vector3.Distance(position, existingPosition) < minDistance)
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Spawn Pitcher on Detected Plane
    public GameObject SpawnPitcher(GameObject pitcherPrefab)
    {
        if (pitcherPrefab == null)
        {
            Debug.LogError("Pitcher prefab is not assigned!");
            return null;
        }

        List<ARPlane> activePlanes = new List<ARPlane>();
        foreach (var plane in planeManager.trackables)
        {
            if (plane.boundary != null && plane.boundary.Count() > 2)
            {
                activePlanes.Add(plane);
            }
            else
            {
                Debug.LogWarning($"Skipping plane {plane.trackableId} due to invalid boundary.");
            }
        }

        if (activePlanes.Count == 0)
        {
            Debug.LogWarning("No valid planes with boundaries to spawn pitcher on.");
            return null;
        }

        int maxAttempts = 10; // Maximum number of attempts to spawn the pitcher
        while (maxAttempts > 0)
        {
            ARPlane selectedPlane = activePlanes[Random.Range(0, activePlanes.Count)];
            Vector3 spawnPosition = GetRandomPointInPlaneBoundary(selectedPlane);

            // Verify the point is on the plane using raycasting
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(spawnPosition);
            if (raycastManager.Raycast(screenPoint, hits, TrackableType.Planes))
            {
                foreach (var hit in hits)
                {
                    if (hit.trackableId == selectedPlane.trackableId)
                    {
                        if (IsPositionTooClose(hit.pose.position, spawnedItemsPosition, distanceSpawnedItems))
                        {
                            Debug.LogWarning($"Position {hit.pose.position} is too close to existing items. Retrying...");
                            continue; // Skip this position and try again
                        }
                        GameObject pitcher = Instantiate(pitcherPrefab, hit.pose.position, Quaternion.identity);
                        spawnedItemsPosition.Add(hit.pose.position);
                        return pitcher;
                    }
                }
            }

            maxAttempts--;
            Debug.LogWarning($"Attempt to spawn pitcher failed. Remaining attempts: {maxAttempts}");
        }

        Debug.LogWarning("Failed to spawn pitcher after multiple attempts.");
        return null;
    }
    #endregion


    #region Spawn Bird NPC
    public GameObject SpawnBirdNPC(GameObject birdPrefab)
    {
        if (birdPrefab == null)
        {
            Debug.LogError("Bird prefab is not assigned!");
            return null;
        }

        List<ARPlane> activePlanes = new List<ARPlane>();
        foreach (var plane in planeManager.trackables)
        {
            if (plane.boundary != null && plane.boundary.Count() > 2)
            {
                activePlanes.Add(plane);
            }
            else
            {
                Debug.LogWarning($"Skipping plane {plane.trackableId} due to invalid boundary.");
            }
        }

        if (activePlanes.Count == 0)
        {
            Debug.LogWarning("No valid planes with boundaries to spawn bird on.");
            return null;
        }

        int maxAttempts = 10; // Maximum number of attempts to spawn the bird
        while (maxAttempts > 0)
        {
            ARPlane selectedPlane = activePlanes[Random.Range(0, activePlanes.Count)];
            Vector3 spawnPosition = GetRandomPointInPlaneBoundary(selectedPlane);

            // Verify the point is on the plane using raycasting
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(spawnPosition);
            if (raycastManager.Raycast(screenPoint, hits, TrackableType.Planes))
            {
                foreach (var hit in hits)
                {
                    if (hit.trackableId == selectedPlane.trackableId)
                    {
                        if (IsPositionTooClose(hit.pose.position, spawnedItemsPosition, distanceSpawnedItems))
                        {
                            Debug.LogWarning($"Position {hit.pose.position} is too close to existing items. Retrying...");
                            continue;
                        }

                        GameObject bird = Instantiate(birdPrefab, hit.pose.position, Quaternion.identity);
                        spawnedItemsPosition.Add(hit.pose.position);
                        return bird;
                    }
                }
            }

            maxAttempts--;
            Debug.LogWarning($"Attempt to spawn bird failed. Remaining attempts: {maxAttempts}");
        }

        Debug.LogWarning("Failed to spawn bird after multiple attempts.");
        return null;
    }
    #endregion
}