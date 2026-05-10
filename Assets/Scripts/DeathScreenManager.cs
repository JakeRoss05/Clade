using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenManager : MonoBehaviour
{
    public GameObject deathScreenPanel;
    private TypewriterEffect typewriterEffect;

    void Awake()
    {
        InitializeDeathScreen();
        // Find the TypewriterEffect in the death screen panel
        if (deathScreenPanel != null)
            typewriterEffect = deathScreenPanel.GetComponentInChildren<TypewriterEffect>();
    }

    void InitializeDeathScreen()
    {
        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(false);
    }

    public void ShowDeathScreen()
    {
        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(true);

        // Set up the phrases for the typewriter effect
        if (typewriterEffect != null)
        {
            typewriterEffect.SetPhrases("GENETIC LINE TERMINATED", "ORGANISM LOST");
            typewriterEffect.StartTypewriter();
        }

        // Pause the game
        Time.timeScale = 0f;
    }

    public void ReturnToMainMenu()
    {
        // Resume time before loading scene
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Assuming MainMenu is scene 0
    }

    public void QuitGame()
    {
        // Resume time before quitting
        Time.timeScale = 1f;
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
