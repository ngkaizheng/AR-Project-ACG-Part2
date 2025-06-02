using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class BirdController : MonoBehaviour
{
    [Header("Bird Settings")]
    [SerializeField] private GameObject birdPrefab;
    [SerializeField] private float sizeThreshold = 1.0f;

    [Header("AR Components")]
    [SerializeField] private ARRaycastManager raycastManager;

    [Header("Input Settings")]
    [SerializeField]
    XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");

    public XRInputValueReader<Vector2> tapStartPositionInput
    {
        get => m_TapStartPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
    }

    public bool sizeThresholdMet = false;
    public bool birdSpawned = false;
    private Vector2 previousTapPosition;
    private static List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();
    public static event Action<bool> OnSizeThresholdMet;

    private void OnEnable()
    {
        tapStartPositionInput.EnableDirectActionIfModeUsed();
    }

    private void OnDisable()
    {
        tapStartPositionInput.DisableDirectActionIfModeUsed();
    }
    void Update()
    {
        if (!sizeThresholdMet || birdSpawned)
            return;

        // Detect if tap position changed (i.e., a new tap happened)
        if (tapStartPositionInput.TryReadValue(out var tapPosition) && tapPosition != previousTapPosition)
        {
            previousTapPosition = tapPosition;
            TrySpawnBirdAt(tapPosition);
        }
    }

    public void CheckAccumulatedPlaneSize(float accumulatedSize)
    {
        if (accumulatedSize >= sizeThreshold)
        {
            sizeThresholdMet = true;
            OnSizeThresholdMet?.Invoke(birdSpawned);
        }
    }

    private void TrySpawnBirdAt(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, raycastHits, TrackableType.Planes))
        {
            Pose hitPose = raycastHits[0].pose;
            Instantiate(birdPrefab, hitPose.position, Quaternion.identity);
            birdSpawned = true;

            OnSizeThresholdMet?.Invoke(birdSpawned);
            Debug.Log("Bird spawned at: " + hitPose.position);
        }
        else
        {
            Debug.LogWarning("No AR plane hit at tapped position.");
        }
    }
}