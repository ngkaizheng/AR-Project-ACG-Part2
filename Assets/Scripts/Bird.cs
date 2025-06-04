using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
// using UnityEngine.TestTools.Constraints; // Make sure your project references XR Interaction Toolkit if needed

public class Bird : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField]
    XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");

    public XRInputValueReader<Vector2> tapStartPositionInput
    {
        get => m_TapStartPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
    }

    [Header("Bird Animation")]
    [SerializeField] public BirdAnim birdAnim;

    [Header("Dialogue UI")]
    public GameObject dialogueUIHolder;

    [Header("Collider")]
    [SerializeField] private Collider birdCollider;

    [Header("Pebble Collection")]
    [SerializeField] private float collectDistance = 1f; // Max distance to collect a pebble
    private int pebblesCollected = 0; // Track collected pebbles
    public bool isCarryingPebble = false; // Track if the bird is carrying a pebble

    [Header("Raycast Visualization")]
    [SerializeField] private LineRenderer lineRenderer; // LineRenderer for visualizing the raycast
    [SerializeField] private float rayLength = 5f; // Maximum length of the ray
    [SerializeField] private GameObject eyePosObject; // Reference to the eye position object

    private ARRaycastManager arRaycastManager;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    Vector2 m_TapStartPosition;
    public bool isNPC = false; // Track if this is an NPC bird


    void Start()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();

        if (birdAnim == null)
            birdAnim = GetComponent<BirdAnim>();

        // Initialize the LineRenderer
        if (lineRenderer == null && !isNPC)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.02f; // Set the width of the line
            lineRenderer.endWidth = 0.02f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Assign a default material
            lineRenderer.startColor = Color.green; // Start color of the line
            lineRenderer.endColor = Color.red; // End color of the line
        }

        if (GameController.Instance.isRayVisible)
        {
            lineRenderer.enabled = true;
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (isNPC)
            return;
        VisualizeRaycast();
    }

    private void VisualizeRaycast()
    {
        // Perform a raycast from lineRenderer's position in the forward direction
        Ray ray = new Ray(eyePosObject.transform.position, eyePosObject.transform.forward);
        RaycastHit hit;

        // Set the start point of the LineRenderer
        lineRenderer.SetPosition(0, ray.origin);

        if (Physics.Raycast(ray, out hit, rayLength))
        {
            // Set the end point of the LineRenderer to the hit point
            lineRenderer.SetPosition(1, hit.point);

            // Optional: Log the hit object
            Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");

            HandleRaycastHit(hit);
        }
        else
        {
            // Set the end point of the LineRenderer to the maximum ray length
            lineRenderer.SetPosition(1, ray.origin + ray.direction * rayLength);
        }
    }
    public void CollectPebble(GameObject pebble)
    {
        pebblesCollected++;
        UIController.Instance.UpdateObjectiveProgress(ObjectiveType.CollectPebbles, pebblesCollected); // Update objective progress (index 1 for pebble collection)
        isCarryingPebble = true;
        Destroy(pebble.transform.root.gameObject); // Destroy the pebble GameObject
    }

    public void FlyToTarget(Vector3 targetPosition)
    {
        if (birdAnim != null)
        {
            birdAnim.FlyToTarget(targetPosition);
            Debug.Log($"Bird flying to position: {targetPosition}");
        }
        else
        {
            Debug.LogWarning("BirdAnim script is not assigned or found on the GameObject.");
        }
    }

    public float GetCollectDistance()
    {
        return collectDistance;
    }

    public bool IsCarryingPebble()
    {
        return isCarryingPebble;
    }

    public void SetCarryingPebble(bool carrying)
    {
        isCarryingPebble = carrying;
    }

    public void SetLineRendererVisible(bool visible)
    {
        lineRenderer.enabled = visible;
    }
    public bool GetLineRendererVisible()
    {
        return lineRenderer.enabled;
    }

    private void HandleRaycastHit(RaycastHit hit)
    {
        // Check if the hit object is a pebble
        if (hit.collider.CompareTag("Rock"))
        {
            if (UIController.Instance.IsObjectiveCompleted(ObjectiveType.AskForHelp))
            {
                DialogueController.Instance.PlayDialogueSequence(DialogueSequence.FoundPebbles1);
            }
        }

        if (hit.collider.CompareTag("Pitcher"))
        {
            if (!UIController.Instance.IsObjectiveCompleted(ObjectiveType.FindWaterSource))
            {
                UIController.Instance.CompleteObjective(ObjectiveType.FindWaterSource);
                DialogueController.Instance.PlayDialogueSequence(DialogueSequence.FoundPitcher);
            }
        }

        if (hit.collider.CompareTag("NPC"))
        {
            if (!UIController.Instance.IsObjectiveCompleted(ObjectiveType.AskForHelp))
            {
                UIController.Instance.CompleteObjective(ObjectiveType.AskForHelp);
                DialogueController.Instance.PlayDialogueSequence(DialogueSequence.NPCGiveHint, birdIndex: 1);
            }
        }
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (isNPC)
    //         return; // Skip interaction if this is an NPC bird

    //     if (other.CompareTag("Rock"))
    //     {
    //         // if (UIController.Instance.IsObjectiveCompleted(ObjectiveType.AskForHelp) &&
    //         //    !DialogueController.Instance.GetDialoguePlayedStatus()[(int)DialogueSequence.FoundPebbles1])
    //         // {
    //         //     DialogueController.Instance.PlayDialogueSequence(DialogueSequence.FoundPebbles1);
    //         // }
    //         if (UIController.Instance.IsObjectiveCompleted(ObjectiveType.AskForHelp))
    //         {
    //             DialogueController.Instance.PlayDialogueSequence(DialogueSequence.FoundPebbles1);
    //         }
    //     }

    //     if (other.CompareTag("Pitcher"))
    //     {
    //         if (!UIController.Instance.IsObjectiveCompleted(ObjectiveType.FindWaterSource))
    //         {
    //             UIController.Instance.CompleteObjective(ObjectiveType.FindWaterSource);
    //             DialogueController.Instance.PlayDialogueSequence(DialogueSequence.FoundPitcher);
    //         }
    //     }

    //     if (other.CompareTag("NPC"))
    //     {
    //         if (!UIController.Instance.IsObjectiveCompleted(ObjectiveType.AskForHelp))
    //         {
    //             UIController.Instance.CompleteObjective(ObjectiveType.AskForHelp);
    //             DialogueController.Instance.PlayDialogueSequence(DialogueSequence.NPCGiveHint, birdIndex: 1);
    //         }
    //     }
    // }

    private void OnTriggerExit(Collider other)
    {
        if (isNPC)
            return; // Skip interaction if this is an NPC bird

        if (other.CompareTag("Rock"))
        {

        }
    }
}
