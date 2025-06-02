using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class UIController : MonoBehaviour
{
    [Header("Bird Controller")]
    [SerializeField] private BirdController birdController;

    [Header("Spawn UI Settings")]
    public GameObject spawnUIHolder;

    public static UIController Instance;


    private void Awake()
    {
        if (birdController == null)
        {
            birdController = FindObjectOfType<BirdController>();
            if (birdController == null)
            {
                Debug.LogError("BirdController not found in the scene!");
            }
        }

        if (spawnUIHolder == null)
        {
            Debug.LogWarning("Spawn UI Holder is not assigned! Please assign it in the inspector.");
        }
        else
        {
            spawnUIHolder.SetActive(false); // Ensure the UI is initially disabled
            Debug.Log("Spawn UI Holder initialized and set to inactive.");
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    void Update()
    {
    }

    public void EnableSpawnUI(bool enable)
    {
        if (spawnUIHolder != null)
        {
            spawnUIHolder.SetActive(enable);
            Debug.Log($"Spawn UI is now {(enable ? "enabled" : "disabled")}");
        }
        else
        {
            Debug.LogWarning("Spawn UI Holder is not assigned!");
        }
    }
}