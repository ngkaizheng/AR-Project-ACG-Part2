using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectivePrefabController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText; // Text component for objective description
    [SerializeField] private Image completionIcon; // Icon to show completion status (e.g., checkmark)
    [SerializeField] private Sprite completedSprite; // Sprite for completed state
    [SerializeField] private Sprite incompleteSprite; // Sprite for incomplete state

    public void InitializeObjective(string description, bool isCompleted)
    {
        if (objectiveText != null)
        {
            objectiveText.text = description;
        }
        UpdateObjectiveStatus(isCompleted);
    }

    public void UpdateObjectiveText(string newDescription)
    {
        if (objectiveText != null)
        {
            objectiveText.text = newDescription;
        }
    }

    public void UpdateObjectiveStatus(bool isCompleted)
    {
        if (completionIcon != null)
        {
            completionIcon.sprite = isCompleted ? completedSprite : incompleteSprite;
            completionIcon.color = isCompleted ? Color.green : Color.white; // Optional: Change color for visibility
        }
    }

    private void Awake()
    {
        if (objectiveText == null)
        {
            Debug.LogWarning("Objective TextMeshProUGUI is not assigned in ObjectivePrefabController!");
        }
        if (completionIcon == null)
        {
            Debug.LogWarning("Completion Icon Image is not assigned in ObjectivePrefabController!");
        }
    }
}