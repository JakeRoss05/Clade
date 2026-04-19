using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Unified.UniversalBlur.Runtime;

public class MainMenuManager : MonoBehaviour
{
    public GameObject menuRoot;
    public GameObject optionsRoot;
    public GameObject blurOverlay;
    public float blurIntensity = 4f;

    void Awake()
    {
        InitializeMenuState();
        ApplyBlurIntensity();
    }

    void Start()
    {
        InitializeMenuState();
        ApplyBlurIntensity();
    }

    void InitializeMenuState()
    {
        if (menuRoot != null)
            menuRoot.SetActive(true);

        if (optionsRoot != null)
            optionsRoot.SetActive(false);

        if (blurOverlay != null)
            blurOverlay.SetActive(false);
    }

    void ApplyBlurIntensity()
    {
        foreach (ScriptableRendererFeature feature in Resources.FindObjectsOfTypeAll<ScriptableRendererFeature>())
        {
            if (feature is UniversalBlurFeature blurFeature)
                blurFeature.Intensity = blurIntensity;
        }
    }

    public void StartGame()
    {
        Debug.Log("PLAY BUTTON CLICKED");
        SceneManager.LoadScene(1);
    }

    public void OpenOptions()
    {
        if (menuRoot != null)
            menuRoot.SetActive(false);

        if (optionsRoot != null)
            optionsRoot.SetActive(true);

        if (blurOverlay != null)
            blurOverlay.SetActive(true);

        OptionsMenuManager optionsMenuManager = GetComponent<OptionsMenuManager>();
        if (optionsMenuManager != null)
            optionsMenuManager.ShowDefaultTab();
    }

    public void CloseOptions()
    {
        if (optionsRoot != null)
            optionsRoot.SetActive(false);

        if (blurOverlay != null)
            blurOverlay.SetActive(false);

        if (menuRoot != null)
            menuRoot.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT BUTTON CLICKED");
        Application.Quit();
    }
}