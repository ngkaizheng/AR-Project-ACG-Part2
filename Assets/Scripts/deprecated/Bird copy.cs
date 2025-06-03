// using UnityEngine;
// using UnityEngine.XR.ARFoundation;
// using UnityEngine.XR.ARSubsystems;
// using System.Collections.Generic;
// using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers; // Make sure your project references XR Interaction Toolkit if needed

// public class Bird : MonoBehaviour
// {
//     [Header("Input Settings")]
//     [SerializeField]
//     XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");

//     public XRInputValueReader<Vector2> tapStartPositionInput
//     {
//         get => m_TapStartPositionInput;
//         set => XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
//     }

//     [Header("Bird Animation")]
//     [SerializeField] public BirdAnim birdAnim;

//     [Header("Dialogue UI")]
//     public GameObject dialogueUIHolder;

//     [Header("Collider")]
//     [SerializeField] private Collider birdCollider;

//     [Header("Pebble Collection")]
//     [SerializeField] private float collectDistance = 1f; // Max distance to collect a pebble
//     private int pebblesCollected = 0; // Track collected pebbles
//     public bool isCarryingPebble = false; // Track if the bird is carrying a pebble

//     private ARRaycastManager arRaycastManager;
//     static List<ARRaycastHit> hits = new List<ARRaycastHit>();
//     Vector2 m_TapStartPosition;


//     void Start()
//     {
//         arRaycastManager = FindObjectOfType<ARRaycastManager>();

//         if (birdAnim == null)
//             birdAnim = GetComponent<BirdAnim>();
//     }

//     void Update()
//     {
//         HandleTapInput();
//     }

//     public void HandleTapInput()
//     {
//         var prevTapStartPosition = m_TapStartPosition;
//         var tapPerformedThisFrame = m_TapStartPositionInput.TryReadValue(out m_TapStartPosition) && prevTapStartPosition != m_TapStartPosition;

//         if (tapPerformedThisFrame)
//         {
//             // Convert screen tap to a ray
//             Ray ray = Camera.main.ScreenPointToRay(m_TapStartPosition);

//             // Use RaycastAll to detect all objects along the ray
//             RaycastHit[] hitsPhysics = Physics.RaycastAll(ray, Mathf.Infinity);
//             Debug.Log($"RaycastAll detected {hitsPhysics.Length} hits.");

//             // Loop through all hits to find a rock
//             foreach (RaycastHit hit in hitsPhysics)
//             {
//                 GameObject hitObject = hit.collider.gameObject;
//                 Debug.Log($"Hit object: {hitObject.name} at position: {hit.point}, Tag: {hitObject.tag}");

//                 // Skip the bird's own GameObject
//                 if (hitObject == gameObject)
//                 {
//                     Debug.Log($"Skipping bird GameObject: {hitObject.name}");
//                     continue;
//                 }

//                 if (hitObject.CompareTag("Rock"))
//                 {
//                     float distanceToRock = Vector3.Distance(transform.position, hitObject.transform.position);
//                     if (distanceToRock <= collectDistance)
//                     {
//                         CollectPebble(hitObject);
//                         Debug.Log($"Rock hit and collected at position: {hitObject.transform.position}");
//                     }
//                     else
//                     {
//                         if (birdAnim != null)
//                         {
//                             birdAnim.FlyToTarget(hitObject.transform.position);
//                             Debug.Log($"Bird flying to rock at position: {hitObject.transform.position}");
//                         }
//                         else
//                         {
//                             Debug.LogWarning("BirdAnim script is not assigned or found on the GameObject.");
//                         }
//                     }
//                     return; // Exit after handling rock
//                 }
//             }

//             // If no rock was hit, try ARRaycastManager for planes
//             if (arRaycastManager.Raycast(m_TapStartPosition, hits, TrackableType.PlaneWithinPolygon | TrackableType.Planes))
//             {
//                 var hitPose = hits[0].pose;
//                 if (birdAnim != null)
//                 {
//                     birdAnim.FlyToTarget(hitPose.position);
//                     Debug.Log($"Bird flying to plane position: {hitPose.position}");
//                 }
//                 else
//                 {
//                     Debug.LogWarning("BirdAnim script is not assigned or found on the GameObject.");
//                 }
//             }
//             else
//             {
//                 Debug.LogWarning("No AR Plane or Rock detected at the tap position.");
//             }
//         }
//         else
//         {
//             Debug.Log("No tap input detected this frame.");
//         }
//     }
//     private void CollectPebble(GameObject pebble)
//     {
//         pebblesCollected++;
//         UIController.Instance.UpdateObjectiveProgress(1, pebblesCollected); // Update objective progress (index 1 for pebble collection)
//         UIController.Instance.SpawnDialogue($"Pebble collected! Progress: {pebblesCollected}/5", Color.blue, 3f);
//         Destroy(pebble); // Remove the pebble from the scene
//         Debug.Log($"Pebble collected! Total: {pebblesCollected}/5");

//         // Play dialogue sequence for rock collection (consistent with OnTriggerEnter)
//         // DialogueController.Instance.PlayDialogueSequence(DialogueSequence.FoundRock1);
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Rock"))
//         {
//             DialogueController.Instance.PlayDialogueSequence(DialogueSequence.FoundRock1);
//         }
//     }

//     private void OnTriggerExit(Collider other)
//     {
//         if (other.CompareTag("Rock"))
//         {

//         }
//     }
// }
