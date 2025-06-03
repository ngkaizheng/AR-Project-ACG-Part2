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

    [Header("UI Settings")]
    public GameObject spawnUIHolder;
    public GameObject detectPlaneUIHolder;

    [Header("Dialogue Settings")]
    [SerializeField] private List<GameObject> dialogueUIHolders = new List<GameObject>();
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


        spawnUIHolder.SetActive(false); // Ensure the UI is initially disabled
        detectPlaneUIHolder.SetActive(true); // Ensure the UI is initially disabled

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
        spawnUIHolder.SetActive(enable);
        if (enable)
        {
            detectPlaneUIHolder.SetActive(false);
        }
    }

    public void SetDialogueUIHolder(GameObject newDialogueUIHolder)
    {
        if (!dialogueUIHolders.Contains(newDialogueUIHolder))
        {
            dialogueUIHolders.Add(newDialogueUIHolder);
            Debug.Log($"Added dialogue UI holder. Total holders: {dialogueUIHolders.Count}");
        }
    }

    public void SpawnDialogue(string textContent, Color borderColor, float duration, int birdIndex = 0)
    {
        if (dialoguePrefab != null && birdIndex < dialogueUIHolders.Count)
        {
            GameObject dialogueInstance = Instantiate(dialoguePrefab, dialogueUIHolders[birdIndex].transform);
            DialoguePrefabController dialogueController = dialogueInstance.GetComponent<DialoguePrefabController>();

            if (dialogueController != null)
            {
                dialogueController.InitializeDialogue(textContent, borderColor, duration);
                dialogueList.Add(dialogueInstance);
            }
        }
        else
        {
            Debug.LogWarning($"Dialogue prefab or dialogueUIHolder for bird index {birdIndex} is not assigned!");
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
        objectives.Add(new Objective(ObjectiveType.FindWaterSource, "Find the water resources."));
        SpawnObjectiveUI(objectives.Find(o => o.type == ObjectiveType.FindWaterSource));
    }

    private void AskForHelpObjectives()
    {
        objectives.Add(new Objective(ObjectiveType.AskForHelp, "Ask for help from the crow."));
        SpawnObjectiveUI(objectives.Find(o => o.type == ObjectiveType.AskForHelp));
    }

    private void AddPebbleObjectives()
    {
        objectives.Add(new Objective(ObjectiveType.CollectPebbles, "Collect 5 pebbles to raise the water level.", false, 0, 5, true));
        objectives.Add(new Objective(ObjectiveType.PutPebblesInPitcher, "Put 5 pebbles in the pitcher to drink the water.", false, 0, 5, true));
        SpawnObjectiveUI(objectives.Find(o => o.type == ObjectiveType.CollectPebbles));
        SpawnObjectiveUI(objectives.Find(o => o.type == ObjectiveType.PutPebblesInPitcher));
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

    public void UpdateObjectiveProgress(ObjectiveType type, int newProgress)
    {
        Objective objective = objectives.Find(o => o.type == type);
        if (objective != null && objective.isProgressBased)
        {
            objective.currentProgress = Mathf.Clamp(newProgress, 0, objective.targetProgress);
            ObjectivePrefabController controller = objectiveInstances[objectives.IndexOf(objective)].GetComponent<ObjectivePrefabController>();
            if (controller != null)
            {
                controller.UpdateObjectiveText(objective.GetFormattedDescription());
                Debug.Log($"Objective '{objective.description}' progress updated: {objective.currentProgress}/{objective.targetProgress}");
            }

            if (objective.currentProgress >= objective.targetProgress && !objective.isCompleted)
            {
                CompleteObjective(type);
            }
        }
        else
        {
            Debug.LogWarning($"Objective of type '{type}' is not progress-based or not found!");
        }
    }

    public void CompleteObjective(ObjectiveType type)
    {
        Objective objective = objectives.Find(o => o.type == type);
        if (objective != null)
        {
            objective.isCompleted = true;
            ObjectivePrefabController controller = objectiveInstances[objectives.IndexOf(objective)].GetComponent<ObjectivePrefabController>();
            if (controller != null)
            {
                controller.UpdateObjectiveStatus(true);
                Debug.Log($"Objective '{objective.description}' completed.");
            }

            // Handle specific objectives
            if (type == ObjectiveType.FindWaterSource)
            {
                AskForHelpObjectives();
            }
            else if (type == ObjectiveType.AskForHelp)
            {
                AddPebbleObjectives();
            }
            else if (type == ObjectiveType.CollectPebbles || type == ObjectiveType.PutPebblesInPitcher)
            {
                bool allPebbleObjectivesCompleted = objectives.Find(o => o.type == ObjectiveType.CollectPebbles).isCompleted &&
                                                    objectives.Find(o => o.type == ObjectiveType.PutPebblesInPitcher).isCompleted;

                if (allPebbleObjectivesCompleted)
                {
                    // SpawnDialogue("All objectives completed! The crow can now drink the water!", Color.green, 5f);
                }
            }
        }
        else
        {
            Debug.LogWarning($"Objective of type '{type}' not found!");
        }
    }

    public List<Objective> GetObjectives()
    {
        return objectives;
    }
    public Objective GetObjectiveByType(ObjectiveType type)
    {
        return objectives.Find(o => o.type == type);
    }

    public bool IsObjectiveCompleted(ObjectiveType type)
    {
        Objective objective = GetObjectiveByType(type);
        return objective != null && objective.isCompleted;
    }
    #endregion
}