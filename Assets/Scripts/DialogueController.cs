using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    [SerializeField] private Color dialogueColor = Color.green;
    [SerializeField] private float defaultDuration = 3f;

    // Map each sequence to its starting index and line count
    private readonly Dictionary<DialogueSequence, (int startIndex, int count)> sequenceMap = new()
    {
        { DialogueSequence.StartingDialogue, (0, 3) },        // Lines 0-2
        { DialogueSequence.FoundPitcher, (3, 3) },            // Lines 3-5
        { DialogueSequence.SolutionRealization, (6, 6) },     // Lines 6-11
        { DialogueSequence.SuccessEnding, (12, 2) }           // Lines 12-13
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void PlayDialogueSequence(DialogueSequence sequence, float delayBetweenLines = 1.5f)
    {
        string[] lines = Dialogue.GetLines(sequence);
        StartCoroutine(PlaySequenceCoroutine(lines, delayBetweenLines));
    }

    private IEnumerator PlaySequenceCoroutine(string[] lines, float delay)
    {
        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(line))
            {
                UIController.Instance.SpawnDialogue(line, dialogueColor, defaultDuration);

                // Play audio
                GameController.Instance.bird.birdAnim.DisplayBehavior(birdBehaviors.sing);

                yield return new WaitForSeconds(delay);
            }
        }
    }
}