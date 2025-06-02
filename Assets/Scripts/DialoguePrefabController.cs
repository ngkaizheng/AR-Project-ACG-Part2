using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialoguePrefabController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image borderImage; // Border of the dialogue box
    [SerializeField] private TMP_Text dialogueText; // Text content of the dialogue

    [Header("Customization Settings")]
    [SerializeField] private Color borderColor = Color.white; // Default border color
    [SerializeField] private string textContent = "Default Dialogue"; // Default text content
    [SerializeField] private float fadeDuration = 5.0f; // Duration before fading away

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void InitializeDialogue(string newTextContent, Color newBorderColor, float duration)
    {
        // Update border color
        if (borderImage != null)
        {
            borderImage.color = newBorderColor;
        }

        // Update text content
        if (dialogueText != null)
        {
            dialogueText.text = newTextContent;
        }

        // Set fade duration
        fadeDuration = duration;

        // Start fading after the duration
        Invoke(nameof(FadeOutDialogue), fadeDuration);
    }

    private void FadeOutDialogue()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    private System.Collections.IEnumerator FadeOutCoroutine()
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < 1.0f)
        {
            elapsedTime += Time.deltaTime / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(1.0f, 0.0f, elapsedTime);
            yield return null;
        }

        Destroy(gameObject); // Remove the dialogue prefab after fading
    }
}