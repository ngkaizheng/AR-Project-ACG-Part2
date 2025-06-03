using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    [SerializeField] private Color dialogueColor = Color.green;
    [SerializeField] private float defaultDuration = 3f;

    [Header("Dialogue Tracking")]
    [SerializeField] private List<bool> dialoguePlayedStatus = new List<bool>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        InitializeDialogueTracking();
    }

    public void PlayDialogueSequence(DialogueSequence sequence, float delayBetweenLines = 1.5f, int birdIndex = 0)
    {
        // Check if the sequence has already been played
        if (dialoguePlayedStatus[(int)sequence])
        {
            Debug.LogWarning($"Dialogue sequence {sequence} has already been played.");
            return;
        }
        // Mark the sequence as played
        dialoguePlayedStatus[(int)sequence] = true;

        string[] lines = Dialogue.GetLines(sequence);
        StartCoroutine(PlaySequenceCoroutine(lines, delayBetweenLines, birdIndex));
    }

    private IEnumerator PlaySequenceCoroutine(string[] lines, float delay, int birdIndex)
    {
        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                UIController.Instance.SpawnDialogue(line, dialogueColor, defaultDuration, birdIndex);

                // Play audio
                GameController.Instance.bird.birdAnim.DisplayBehavior(birdBehaviors.sing);

                yield return new WaitForSeconds(delay);
            }
        }
    }

    public void InitializeDialogueTracking()
    {
        dialoguePlayedStatus.Clear();
        int sequenceLength = System.Enum.GetValues(typeof(DialogueSequence)).Length;
        for (int i = 0; i < sequenceLength; i++)
        {
            dialoguePlayedStatus.Add(false);
        }
    }

    public List<bool> GetDialoguePlayedStatus()
    {
        return dialoguePlayedStatus;
    }
}