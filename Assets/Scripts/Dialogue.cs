using System.Collections.Generic;
using UnityEngine;

public enum DialogueSequence
{
    StartingDialogue,
    FoundPitcher,
    FoundRock1,
    SolutionRealization,
    SuccessEnding
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
                "This desert is so dry..."
            }
        },
        {
            DialogueSequence.FoundPitcher, new[]
            {
                "Oh look, a pitcher!",
                "Maybe it has water?",
                "This pitcher has water, but it’s too low!"
            }
        },
        {
            DialogueSequence.FoundRock1, new[]
            {
                "A rock! Maybe I can use it to reach the water.",
                "Maybe I can drop it in the pitcher?"
            }
        },
        {
            DialogueSequence.SolutionRealization, new[]
            {
                "If only I could reach it...",
                "Maybe I can knock it over?",
                "No good... It won’t budge.",
                "Wait... What if I drop pebbles in it?",
                "The water’s rising!",
                "It’s working! Just a bit more..."
            }
        },
        {
            DialogueSequence.SuccessEnding, new[]
            {
                "Finally! I can drink!",
                "Where there’s a will, there’s a way."
            }
        }
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
