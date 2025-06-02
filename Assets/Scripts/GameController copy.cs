// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;
// using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

// public class GameController : MonoBehaviour
// {
//     [HideInInspector]
//     public Bird bird;
//     public PlaneDetectionController planeDetectionController;
//     private Pitcher pitcher; // Reference to the Pitcher script
//     [SerializeField] private ARRaycastManager arRaycastManager; // For AR plane detection

//     [Header("Input Settings")]
//     [SerializeField]
//     private XRInputValueReader<Vector2> tapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");
//     static List<ARRaycastHit> hits = new List<ARRaycastHit>();


//     [Header("Game Settings")]
//     public GameObject rockPrefab;
//     public int numOfRocksToSpawn = 5;
//     public GameObject pitcherPrefab;

//     private Vector2 prevTapStartPosition;
//     public static GameController Instance { get; private set; }

//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//         }
//         else
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject); // Keep this instance across scenes
//         }
//     }

//     private void Start()
//     {
//         if (arRaycastManager == null)
//         {
//             arRaycastManager = FindObjectOfType<ARRaycastManager>();
//             if (arRaycastManager == null)
//             {
//                 Debug.LogError("ARRaycastManager not found in scene!");
//             }
//         }
//     }

//     private void OnEnable()
//     {
//         BirdController.OnSizeThresholdMet += HandleSizeThresholdMet;
//         BirdController.OnBirdSpawned += HandleBirdSpawned;
//     }

//     private void OnDisable()
//     {
//         BirdController.OnSizeThresholdMet -= HandleSizeThresholdMet;
//         BirdController.OnBirdSpawned -= HandleBirdSpawned;
//     }

//     private void HandleSizeThresholdMet(bool birdSpawned)
//     {
//         UIController.Instance.EnableSpawnUI(!birdSpawned);
//     }

//     private void HandleBirdSpawned(Bird bird)
//     {
//         this.bird = bird;
//         UIController.Instance.SetDialogueUIHolder(bird.dialogueUIHolder);
//         UIController.Instance.EnableObjectiveUI(true);
//         DialogueController.Instance.PlayDialogueSequence(DialogueSequence.StartingDialogue);
//         SpawnRocks();
//         SpawnPitcher();
//     }

//     private void Update()
//     {
//         HandleTapInput();
//     }

//     private void HandleTapInput()
//     {
//         Vector2 tapStartPosition;
//         bool tapPerformedThisFrame = tapStartPositionInput.TryReadValue(out tapStartPosition) && tapStartPosition != prevTapStartPosition;

//         if (tapPerformedThisFrame)
//         {
//             // Convert screen tap to a ray
//             Ray ray = Camera.main.ScreenPointToRay(tapStartPosition);
//             RaycastHit[] hitsPhysics = Physics.RaycastAll(ray, Mathf.Infinity);
//             Debug.Log($"RaycastAll detected {hitsPhysics.Length} hits.");

//             // Loop through all physics hits
//             foreach (RaycastHit hit in hitsPhysics)
//             {
//                 GameObject hitObject = hit.collider.gameObject;
//                 Debug.Log($"Hit object: {hitObject.name} at position: {hit.point}, Tag: {hitObject.tag}");

//                 // Skip the bird's own GameObject
//                 if (hitObject == bird?.gameObject)
//                 {
//                     Debug.Log($"Skipping bird GameObject: {hitObject.name}");
//                     continue;
//                 }

//                 // Handle tap on pitcher
//                 if (hitObject.GetComponent<Pitcher>() != null && pitcher != null)
//                 {
//                     if (pitcher.birdInRange && bird != null && bird.IsCarryingPebble())
//                     {
//                         pitcher.AddPebbleToBottle();
//                         bird.SetCarryingPebble(false);
//                         Debug.Log($"Pebble added to pitcher via GameController.");
//                     }
//                     else
//                     {
//                         bird.FlyToTarget(hitObject.transform.position);
//                         Debug.Log("Bird is not in range or not carrying a pebble.");
//                     }
//                     prevTapStartPosition = tapStartPosition;
//                     return; // Exit after handling pitcher
//                 }

//                 // Handle tap on rock
//                 if (hitObject.CompareTag("Rock"))
//                 {
//                     if (bird != null)
//                     {
//                         float distanceToRock = Vector3.Distance(bird.transform.position, hitObject.transform.position);
//                         if (distanceToRock <= bird.GetCollectDistance())
//                         {
//                             if (bird.IsCarryingPebble())
//                             {
//                                 UIController.Instance.SpawnDialogue("I can only carry one pebble at a time!", Color.red, 3f);
//                             }
//                             else
//                             {
//                                 bird.CollectPebble(hitObject);
//                                 Debug.Log($"Rock hit and collected at position: {hitObject.transform.position}");
//                             }
//                         }
//                         else
//                         {
//                             bird.FlyToTarget(hitObject.transform.position);
//                             Debug.Log($"Bird flying to rock at position: {hitObject.transform.position}");
//                         }
//                     }
//                     prevTapStartPosition = tapStartPosition;
//                     return; // Exit after handling rock
//                 }
//             }

//             // If no physics hit, try AR plane detection
//             if (arRaycastManager != null && arRaycastManager.Raycast(tapStartPosition, hits, TrackableType.PlaneWithinPolygon | TrackableType.Planes))
//             {
//                 var hitPose = hits[0].pose;
//                 if (bird != null)
//                 {
//                     bird.FlyToTarget(hitPose.position);
//                     Debug.Log($"Bird flying to plane position: {hitPose.position}");
//                 }
//                 prevTapStartPosition = tapStartPosition;
//             }
//             else
//             {
//                 Debug.LogWarning("No AR Plane, Rock, or Pitcher detected at the tap position.");
//             }
//         }
//         prevTapStartPosition = tapStartPosition;
//     }

//     //Spwan some rocks on the AR plane
//     public void SpawnRocks()
//     {
//         planeDetectionController.SpawnRocks(numOfRocksToSpawn, rockPrefab);
//     }

//     public void SpawnPitcher()
//     {
//         GameObject pitcherInstance = planeDetectionController.SpawnPitcher(pitcherPrefab);
//         pitcher = pitcherInstance.GetComponent<Pitcher>(); // Assign the reference to the spawned pitcher
//     }
// }