using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGameObject : MonoBehaviour
{
    [SerializeField] private GameObject _UICanvas;

    private void OnEnable()
    {
        PauseHandler.OnPauseEnable += EnablePause;
        PauseHandler.OnPauseDisable += DisablePause;
    }

    private void OnDisable()
    {
        PauseHandler.OnPauseEnable -= EnablePause;
        PauseHandler.OnPauseDisable -= DisablePause;
    }

    private void Start()
    {
        PauseHandler.TimeScale = Time.timeScale;
    }

    private void EnablePause()
    {
        _UICanvas.SetActive(true);
    }

    private void DisablePause()
    {
        _UICanvas?.SetActive(false);
    }

    //Untoggles pause then restarts scene
    public void RestartButton()
    {
        PauseHandler.TogglePause();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResumeButton()
    {
        PauseHandler.TogglePause();
    }

    public void AbilityButton()
    {
        //to be added
    }
}


