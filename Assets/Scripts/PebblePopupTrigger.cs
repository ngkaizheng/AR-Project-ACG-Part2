using UnityEngine;

public class PebblePopupTrigger : MonoBehaviour
{
    [Header("Assign the pop-up UI GameObject (Panel, Canvas, etc.)")]
    public GameObject popupMessage;

    private bool hasShown = false;

    // Singleton pattern for easy access
    public static PebblePopupTrigger Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        if (popupMessage != null)
            popupMessage.SetActive(false); // Hide pop-up at start
    }

    /// <summary>
    /// Call this when 5 pebbles have been collected.
    /// </summary>
    public void ShowPopup()
    {
        if (!hasShown && popupMessage != null)
        {
            popupMessage.SetActive(true);
            hasShown = true;
            Debug.Log("Pop-up message shown after collecting 5 pebbles!");
        }
    }

    /// <summary>
    /// Hide the popup, call from UI Button if needed.
    /// </summary>
    public void HidePopup()
    {
        if (popupMessage != null)
            popupMessage.SetActive(false);
    }
}