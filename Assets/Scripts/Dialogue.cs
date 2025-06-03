using System.Collections.Generic;
using UnityEngine;

public enum DialogueSequence
{
    StartingDialogue,
    FoundPitcher,
    NPCGiveHint,
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
                "Maybe I can go ask for help?",
                // "Think, think...\nHow can I make the water rise?",
                // "Pebbles! If I add pebbles,\nthe water will come up!"
            }
        },
        {
            DialogueSequence.NPCGiveHint, new[]
            {
                "Hi there, little crow!\nI see you're thirsty.",
                "I can help you with that.",
                "If you find some pebbles,\nyou can drop them in the pitcher.",
                "Each pebble will make the water rise.\nJust keep adding them until you can drink!",
                "Good luck, little crow!\nYou can do it!"
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
                "Where there is a will, there is a way!"
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
