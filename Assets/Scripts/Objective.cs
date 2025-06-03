public class Objective
{
    public ObjectiveType type; // Type of the objective
    public string description;
    public bool isCompleted;
    public int currentProgress; // Current progress (e.g., pebbles collected)
    public int targetProgress; // Target progress (e.g., total pebbles needed)
    public bool isProgressBased; // Indicates if the objective tracks progress

    public Objective(ObjectiveType type, string desc, bool completed = false, int current = 0, int target = 0, bool progressBased = false)
    {
        this.type = type;
        description = desc;
        isCompleted = completed;
        currentProgress = current;
        targetProgress = target;
        isProgressBased = progressBased;
    }

    // Get formatted description with progress
    public string GetFormattedDescription()
    {
        if (isProgressBased)
        {
            return $"{description.Replace("(0/5)", $"({currentProgress}/{targetProgress})")}";
        }
        return description;
    }
}

public enum ObjectiveType
{
    FindWaterSource,
    AskForHelp,
    CollectPebbles,
    PutPebblesInPitcher,
}