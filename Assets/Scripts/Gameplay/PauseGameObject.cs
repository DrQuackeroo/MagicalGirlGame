using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * If the buttons do not work properly, make sure there is a (1) PauseUI prefab in the scene and (2) an EventSystem GameObject in the scene.
 * To make an EventSystem: Right Click in hierarchy -> UI -> EventSystem (default settings it has are fine).
 */

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
        Debug.Log("Restart button pressed");
        PauseHandler.UnPause(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResumeButton()
    {
        Debug.Log("Resume button pressed");
        PauseHandler.UnPause(true);
    }

    public void AbilityButton()
    {
        Debug.Log("Ability button pressed");
        AbilityHandler.EnterAbilityMenu();
    }

    public void ControlsButton()
    {
        Debug.Log("Control button pressed");
        PauseHandler.OpenControlsMenu();
    }
}


