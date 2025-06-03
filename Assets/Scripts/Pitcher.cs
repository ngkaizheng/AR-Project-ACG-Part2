using UnityEngine;

public class Pitcher : MonoBehaviour
{
    [Header("Pitcher Settings")]
    [SerializeField] private int pebblesInBottle = 0; // Number of pebbles in the pitcher
    [SerializeField] private int maxPebbles = 5; // Maximum pebbles the pitcher can hold

    [Header("Shader Settings")]
    [SerializeField] private Renderer pitcherRenderer; // Renderer for the pitcher
    [SerializeField] private string fillPropertyName = "_Fill"; // Shader property name for the fill value
    [SerializeField] private float initialFillValue = 0.1f; // Initial fill value for the shader

    public bool birdInRange = false; // Track if the bird is in range
    private Bird bird; // Reference to the Bird script

    private void Start()
    {
        UpdateShaderFill(); // Initialize shader fill value
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bird"))
        {
            bird = other.GetComponent<Bird>();
            if (bird != null)
            {
                birdInRange = true;
                Debug.Log("Bird entered pitcher range.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bird"))
        {
            birdInRange = false;
            bird = null;
            Debug.Log("Bird exited pitcher range.");
        }
    }

    private void Update()
    {
        HandleTapInput();
    }

    private void HandleTapInput()
    {
        if (birdInRange && bird != null && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(touch.position);

            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject == gameObject) // Check if the tap is on the pitcher
                {
                    if (bird.isCarryingPebble)
                    {
                        AddPebbleToBottle();
                        bird.isCarryingPebble = false; // Reset bird's pebble-carrying state
                        Debug.Log($"Pebble added to pitcher. Total: {pebblesInBottle}/{maxPebbles}");
                    }
                    else
                    {
                        Debug.Log("Bird is not carrying a pebble.");
                    }
                    break; // Exit loop once the pitcher is found
                }
            }
        }
    }

    public void AddPebbleToBottle()
    {
        if (pebblesInBottle < maxPebbles)
        {
            pebblesInBottle++;
            UpdateShaderFill();
            UIController.Instance.UpdateObjectiveProgress(ObjectiveType.PutPebblesInPitcher, pebblesInBottle); // Index 2 for pitcher pebble progress

            Objective pitcherObjective = UIController.Instance.GetObjectiveByType(ObjectiveType.PutPebblesInPitcher);

            if (pitcherObjective != null)
            {
                // Check if the progress of the objective is 1/5
                if (pitcherObjective.currentProgress == 1 && !pitcherObjective.isCompleted)
                {
                    DialogueController.Instance.PlayDialogueSequence(DialogueSequence.DropPebble1);
                }

                // Check if the progress of the objective is 4/5
                if (pitcherObjective.currentProgress == 4 && !pitcherObjective.isCompleted)
                {
                    DialogueController.Instance.PlayDialogueSequence(DialogueSequence.DropPebble2);
                }

                // Check if the objective is completed (5/5)
                if (pitcherObjective.isCompleted)
                {
                    DialogueController.Instance.PlayDialogueSequence(DialogueSequence.ReachWater);
                }
            }
            else
            {
                Debug.LogWarning("Objective 'PutPebblesInPitcher' not found!");
            }
        }
        else
        {
            Debug.Log("Pitcher is already full!");
        }
    }

    private void UpdateShaderFill()
    {
        if (pitcherRenderer != null)
        {
            float fillValue = Mathf.Max(initialFillValue, (float)pebblesInBottle / maxPebbles); // Ensure fill value is at least initialFillValue
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            pitcherRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(fillPropertyName, fillValue);
            pitcherRenderer.SetPropertyBlock(propertyBlock);

            Debug.Log($"Shader fill updated to: {fillValue}");
        }
        else
        {
            Debug.LogError("Pitcher renderer is not assigned!");
        }
    }
}