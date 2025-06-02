using UnityEngine;

public class GameController : MonoBehaviour
{
    public Bird bird;
    public PlaneDetectionController planeDetectionController;

    [Header("Game Settings")]
    public GameObject rockPrefab;
    public int numOfRocksToSpawn = 5;
    public static GameController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
    }

    private void OnEnable()
    {
        BirdController.OnSizeThresholdMet += HandleSizeThresholdMet;
        BirdController.OnBirdSpawned += HandleBirdSpawned;
    }

    private void OnDisable()
    {
        BirdController.OnSizeThresholdMet -= HandleSizeThresholdMet;
        BirdController.OnBirdSpawned -= HandleBirdSpawned;
    }

    private void HandleSizeThresholdMet(bool birdSpawned)
    {
        UIController.Instance.EnableSpawnUI(!birdSpawned);
    }

    private void HandleBirdSpawned(Bird bird)
    {
        this.bird = bird;
        UIController.Instance.SetDialogueUIHolder(bird.dialogueUIHolder);
        DialogueController.Instance.PlayDialogueSequence(DialogueSequence.StartingDialogue);
        SpawnRocks();
    }

    //Spwan some rocks on the AR plane
    public void SpawnRocks()
    {
        planeDetectionController.SpawnRocks(numOfRocksToSpawn, rockPrefab);
    }
}