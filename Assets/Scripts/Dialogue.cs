using System.Collections.Generic;
using UnityEngine;

public enum DialogueSequence
{
    StartingDialogue,
    FoundPitcher,
    FoundPebbles1,
    DropPebble1,
    DropPebble2,
    ReachWater,
}

public static class Dialogue
{
    private static readonly Dictionary<DialogueSequence, string[]> dialogueLines = new()
    {
        {
            DialogueSequence.StartingDialogue, new[]
            {
                "Ahh... I'm so thirsty...",
                "I need to find some water.",
                "This place is so dry.",
            }
        },
        {
            DialogueSequence.FoundPitcher, new[]
            {
                "A pitcher! Maybe it has water!",
                "Water! But... I can't reach it.",
                "The water is too low\nfor my beak to touch.",
                "Think, think...\nHow can I make the water rise?",
                "Pebbles! If I add pebbles,\nthe water will come up!"
            }
        },
        {
            DialogueSequence.FoundPebbles1, new[]
            {
                "Pebbles! These will help!",
                "One by one, I'll drop them in.\nThe water must rise!"
            }
        },
        {
            DialogueSequence.DropPebble1, new[]
            {
                "Yes! The water is coming up!",
                "More pebbles...\njust a little higher..."
            }
        },
        {
            DialogueSequence.DropPebble2, new[]
            {
                "Almost there!\nOne more should do it!",
                "I can see the water\nrising with each stone!"
            }
        },
        {
            DialogueSequence.ReachWater, new[]
            {
                "Success! The water reaches me now!",
                "At last... cool, fresh water!\nI'm saved!",
                "Small steps can solve big problems.\nPatience and cleverness win!",
                "When faced with a challenge,\nthink like the crow with the pitcher!"
            }
        },
    };

    public static string[] GetLines(DialogueSequence sequence)
    {
        return dialogueLines.TryGetValue(sequence, out var lines)
            ? lines
            : new string[0];
    }

    public static string GetLine(DialogueSequence sequence, int index)
    {
        if (dialogueLines.TryGetValue(sequence, out var lines) && index >= 0 && index < lines.Length)
            return lines[index];
        else
            return string.Empty;
    }
}
