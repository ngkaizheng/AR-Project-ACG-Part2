using UnityEngine;

public class GameController : MonoBehaviour
{
    private void OnEnable()
    {
        BirdController.OnSizeThresholdMet += HandleSizeThresholdMet;
    }

    private void OnDisable()
    {
        BirdController.OnSizeThresholdMet -= HandleSizeThresholdMet;
    }

    private void HandleSizeThresholdMet(bool birdSpawned)
    {
        Debug.Log("Size threshold met! Notify UIController.");
        // Pass the event to UIController or perform other actions
        UIController.Instance.EnableSpawnUI(!birdSpawned);
    }
}