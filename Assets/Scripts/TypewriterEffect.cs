using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private string[] phrases;
    public float delayBetweenCharacters = 0.05f;
    public float delayBetweenPhrases = 1f;
    private Coroutine typewriterCoroutine;

    void Awake()
    {
        CacheTextComponent();
    }

    void Start()
    {
        CacheTextComponent();
    }

    void CacheTextComponent()
    {
        if (textComponent != null)
            return;

        textComponent = GetComponentInChildren<TextMeshProUGUI>(true);
        if (textComponent == null)
            Debug.LogError("TextMeshProUGUI component not found in TypewriterEffect or its children!");
    }

    public void SetPhrases(params string[] newPhrases)
    {
        phrases = newPhrases;
    }

    public void StartTypewriter()
    {
        CacheTextComponent();

        // Stop any existing typewriter effect
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        if (phrases == null || phrases.Length == 0)
        {
            Debug.LogWarning("No phrases set for TypewriterEffect!");
            return;
        }

        typewriterCoroutine = StartCoroutine(TypewriterSequence());
    }

    private IEnumerator TypewriterSequence()
    {
        for (int i = 0; i < phrases.Length; i++)
        {
            // Type the phrase
            yield return StartCoroutine(TypePhrase(phrases[i]));

            // If not the last phrase, backspace and continue
            if (i < phrases.Length - 1)
            {
                yield return new WaitForSecondsRealtime(delayBetweenPhrases);
                yield return StartCoroutine(BackspacePhrase());
                yield return new WaitForSecondsRealtime(delayBetweenPhrases);
            }
        }
    }

    private IEnumerator TypePhrase(string phrase)
    {
        if (textComponent == null)
            yield break;

        textComponent.text = phrase;
        textComponent.ForceMeshUpdate();
        textComponent.maxVisibleCharacters = 0;
        int charCount = 0;
        int totalCharacters = textComponent.textInfo.characterCount;

        Debug.Log($"Starting typewriter for phrase: '{phrase}' (length: {totalCharacters})");

        while (charCount < totalCharacters)
        {
            charCount++;
            textComponent.maxVisibleCharacters = charCount;
            Debug.Log($"Typing character {charCount}/{totalCharacters}");
            yield return new WaitForSecondsRealtime(delayBetweenCharacters);
        }

        Debug.Log("Finished typing phrase");
    }

    private IEnumerator BackspacePhrase()
    {
        int charCount = textComponent.text.Length;

        while (charCount > 0)
        {
            charCount--;
            textComponent.maxVisibleCharacters = charCount;
            yield return new WaitForSecondsRealtime(delayBetweenCharacters);
        }
    }
}

