using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Holds functions that are called when buttons on the Main Menu are clicked. Navigates between Main Menu screens.
/// </summary>
public class UIMainMenuController : MonoBehaviour
{
    // How long it takes for a screen to fade in or out.
    private const float _fadeTime = 0.15f;

    [Tooltip("The name of scene to load when the 'Start Game' button is clicked.")]
    [SerializeField] private string _startingSceneName = "DebugScene";

    [Header("Screens")]
    [Tooltip("Assumed to be the starting screen.")]
    [SerializeField] private CanvasGroup _titleScreen;
    [SerializeField] private CanvasGroup _loadingScreen;
    [SerializeField] private CanvasGroup _coverScreen;

    private CanvasGroup _currentScreen;

    // Start is called before the first frame update
    void Start()
    {
        _currentScreen = _titleScreen;
    }

    /// <summary>
    /// Loads the starting scene and begins the game.
    /// </summary>
    public void OnStartGamePressed()
    {
        _currentScreen.interactable = false;
        _currentScreen = _loadingScreen;
        StartCoroutine(SwapToLoadingScreen());
    }

    /// <summary>
    /// Swap from the currently shown screen to a different screen.
    /// </summary>
    /// <param name="newScreen">CanvasGroup of the new screen to show.</param>
    public void SwapToScreen(CanvasGroup newScreen)
    {
        StartCoroutine(FadeToNewScreen(newScreen));
    }

    /// <summary>
    /// Used to make a smoother transition to the Loading Screen.
    /// </summary>
    IEnumerator SwapToLoadingScreen()
    {
        StartCoroutine(FadeScreen(_coverScreen, true));
        yield return new WaitForSeconds(_fadeTime);
        _loadingScreen.alpha = 1.0f;

        SceneManager.LoadSceneAsync(_startingSceneName);
    }

    /// <summary>
    /// Fades out _currentScreen, fades in newScreen.
    /// </summary>
    IEnumerator FadeToNewScreen(CanvasGroup newScreen)
    {
        _currentScreen.interactable = false;
        _currentScreen.blocksRaycasts = false;
        StartCoroutine(FadeScreen(_currentScreen, false));
        yield return new WaitForSeconds(_fadeTime);

        _currentScreen = newScreen;
        _currentScreen.interactable = true;
        _currentScreen.blocksRaycasts = true;
        StartCoroutine(FadeScreen(_currentScreen, true));
        yield break;
    }

    /// <summary>
    /// Fade in/out a screen.
    /// Could also be done with an animation, but that's too much hassle.
    /// </summary>
    IEnumerator FadeScreen(CanvasGroup screen, bool isFadingIn)
    {
        float currentFadeTime = 0.0f;
        float start = 0.0f;
        float end = 1.0f;

        if (!isFadingIn)
        {
            start = 1.0f;
            end = 0.0f;
        }

        while (currentFadeTime <= _fadeTime)
        {
            currentFadeTime += Time.deltaTime;
            screen.alpha = Mathf.Lerp(start, end, currentFadeTime / _fadeTime);
            yield return null;
        }

        yield break;
    }

    /// <summary>
    /// Closes the game. Also stops playing in editor if not running the built game.
    /// </summary>
    public void OnQuitGamePressed()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }
}
