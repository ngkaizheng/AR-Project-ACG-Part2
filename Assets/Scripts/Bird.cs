using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers; // Make sure your project references XR Interaction Toolkit if needed

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

    private ARRaycastManager arRaycastManager;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Reference to the lb.Bird script
    [SerializeField] private BirdAnim birdAnim;
    Vector2 m_TapStartPosition;


    void Start()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();

        if (birdAnim == null)
            birdAnim = GetComponent<BirdAnim>();
    }

    void Update()
    {
        HandleTapInput();
    }

    void HandleTapInput()
    {

        var prevTapStartPosition = m_TapStartPosition;
        var tapPerformedThisFrame = m_TapStartPositionInput.TryReadValue(out m_TapStartPosition) && prevTapStartPosition != m_TapStartPosition;

        if (tapPerformedThisFrame)
        {
            if (arRaycastManager.Raycast(m_TapStartPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = hits[0].pose;

                // Teleport this bird to the hit position
                // transform.position = hitPose.position;

                // Trigger the bird animation
                if (birdAnim != null)
                {
                    birdAnim.FlyToTarget(hitPose.position);
                    Debug.Log($"Bird flying to position: {hitPose.position}");
                }
                else
                {
                    Debug.LogWarning("BirdAnim script is not assigned or found on the GameObject.");
                }

            }
            else
            {
                Debug.LogWarning("No AR Plane detected at the tap position.");
            }
        }
        else
        {
            Debug.Log("No tap input detected this frame.");
        }

    }
}
