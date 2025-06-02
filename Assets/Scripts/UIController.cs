using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public class UIController : MonoBehaviour
{
    [Header("Bird Controller")]
    [SerializeField] private BirdController birdController;

    [Header("Spawn UI Settings")]
    public GameObject spawnUIHolder;

    [Header("Dialogue Settings")]
    [SerializeField] private GameObject dialogueUIHolder; // Parent for dialogue prefabs
    [SerializeField] private GameObject dialoguePrefab; // Prefab for dialogue
    [SerializeField] private List<GameObject> dialogueList = new List<GameObject>();

    [Header("Objective UI Settings")]
    [SerializeField] private GameObject objectiveUIHolder; // Parent for objective prefabs
    [SerializeField] private GameObject objectivePrefab; // Prefab for objective UI
    [SerializeField] private List<Objective> objectives = new List<Objective>(); // List of objectives
    [SerializeField] private List<GameObject> objectiveInstances = new List<GameObject>(); // Active objective UI instances


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

        if (objectiveUIHolder == null)
        {
            Debug.LogWarning("Objective UI Holder is not assigned! Please assign it in the inspector.");
        }
        else
        {
            objectiveUIHolder.SetActive(false); // Ensure the UI is initially disabled
            Debug.Log("Objective UI Holder initialized and set to inactive.");
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
        InitializeObjectives();
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

    public void SetDialogueUIHolder(GameObject newDialogueUIHolder)
    {
        dialogueUIHolder = newDialogueUIHolder;
        Debug.Log("Dialogue UI Holder has been set.");
    }

    public void SpawnDialogue(string textContent, Color borderColor, float duration)
    {
        if (dialoguePrefab != null && dialogueUIHolder != null)
        {
            GameObject dialogueInstance = Instantiate(dialoguePrefab, dialogueUIHolder.transform);
            DialoguePrefabController dialogueController = dialogueInstance.GetComponent<DialoguePrefabController>();

            if (dialogueController != null)
            {
                dialogueController.InitializeDialogue(textContent, borderColor, duration);
                dialogueList.Add(dialogueInstance); // Add to the list for tracking
            }
        }
        else
        {
            Debug.LogWarning("Dialogue prefab or dialogueUIHolder is not assigned!");
        }
    }

    #region Objective Management
    public void EnableObjectiveUI(bool enable)
    {
        if (objectiveUIHolder != null)
        {
            objectiveUIHolder.SetActive(enable);
            Debug.Log($"Objective UI is now {(enable ? "enabled" : "disabled")}");
        }
        else
        {
            Debug.LogWarning("Objective UI Holder is not assigned!");
        }
    }

    private void InitializeObjectives()
    {
        // Initialize only the first objective
        objectives.Add(new Objective("Find the water resources."));
        SpawnObjectiveUI(objectives[0]);
    }

    private void AddPebbleObjectives()
    {
        // Add the two pebble-related objectives
        objectives.Add(new Objective("Collect 5 pebbles to raise the water level. (0/5)", false, 0, 5, true));
        objectives.Add(new Objective("Put 5 pebbles in the pitcher to drink the water. (0/5)", false, 0, 5, true));
        // Spawn UI for the new objectives
        SpawnObjectiveUI(objectives[1]);
        SpawnObjectiveUI(objectives[2]);
    }

    private void SpawnObjectiveUI(Objective objective)
    {
        if (objectivePrefab != null && objectiveUIHolder != null)
        {
            GameObject objectiveInstance = Instantiate(objectivePrefab, objectiveUIHolder.transform);
            ObjectivePrefabController objectiveController = objectiveInstance.GetComponent<ObjectivePrefabController>();

            if (objectiveController != null)
            {
                objectiveController.InitializeObjective(objective.description, objective.isCompleted);
                objectiveInstances.Add(objectiveInstance);
            }
        }
        else
        {
            Debug.LogWarning("Objective prefab or objectiveUIHolder is not assigned!");
        }
    }

    public void UpdateObjectiveProgress(int index, int newProgress)
    {
        if (index >= 0 && index < objectives.Count && objectives[index].isProgressBased)
        {
            Objective obj = objectives[index];
            obj.currentProgress = Mathf.Clamp(newProgress, 0, obj.targetProgress);
            ObjectivePrefabController controller = objectiveInstances[index].GetComponent<ObjectivePrefabController>();
            if (controller != null)
            {
                controller.UpdateObjectiveText(obj.GetFormattedDescription());
                Debug.Log($"Objective {index + 1} progress updated: {obj.currentProgress}/{obj.targetProgress}");
            }

            if (obj.currentProgress >= obj.targetProgress && !obj.isCompleted)
            {
                CompleteObjective(index);
            }
        }
        else
        {
            Debug.LogWarning("Invalid objective index or objective is not progress-based!");
        }
    }

    public void CompleteObjective(int index)
    {
        if (index >= 0 && index < objectives.Count)
        {
            objectives[index].isCompleted = true;
            ObjectivePrefabController controller = objectiveInstances[index].GetComponent<ObjectivePrefabController>();
            if (controller != null)
            {
                controller.UpdateObjectiveStatus(true);
                Debug.Log($"Objective {index + 1}: '{objectives[index].description}' completed.");
            }

            // If the first objective is completed, add the pebble objectives
            if (index == 0)
            {
                AddPebbleObjectives();
                // SpawnDialogue("Water resources found! New objectives: Collect and place pebbles.", Color.green, 5f);
            }
            else
            {
                // Check if both pebble objectives (indices 1 and 2) are completed
                bool allPebbleObjectivesCompleted = objectives[1].isCompleted && objectives[2].isCompleted;

                if (allPebbleObjectivesCompleted)
                {
                    // SpawnDialogue("All objectives completed! The crow can now drink the water!", Color.green, 5f);
                }
            }
        }
        else
        {
            Debug.LogWarning("Invalid objective index!");
        }
    }

    public List<Objective> GetObjectives()
    {
        return objectives;
    }
    #endregion
}