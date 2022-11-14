using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Holds functions that are called when buttons on the Main Menu are clicked. Navigates between Main Menu screens.
/// </summary>
public class UIMainMenuController : MonoBehaviour
{
    [Tooltip("The name of scene to load when the 'Start Game' button is clicked.")]
    [SerializeField] private string _startingSceneName = "DebugScene";

    [Header("Screens")]
    [Tooltip("Assumed to be the starting screen.")]
    [SerializeField] private CanvasGroup _titleScreen;
    [SerializeField] private CanvasGroup _loadingScreen;

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
        // TODO: Maybe fade here?
        _loadingScreen.alpha = 1;

        SceneManager.LoadSceneAsync(_startingSceneName);
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
