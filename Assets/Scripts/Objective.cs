public class Objective
{
    public string description;
    public bool isCompleted;
    public int currentProgress; // Current progress (e.g., pebbles collected)
    public int targetProgress; // Target progress (e.g., total pebbles needed)
    public bool isProgressBased; // Indicates if the objective tracks progress

    public Objective(string desc, bool completed = false, int current = 0, int target = 0, bool progressBased = false)
    {
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