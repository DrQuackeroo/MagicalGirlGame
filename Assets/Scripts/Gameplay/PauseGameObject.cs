using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void ResumeButton()
    {
        PauseHandler.TogglePause();
    }

    public void AbilityButton()
    {
        //to be added
    }
}

public static class PauseHandler
{
    public static bool IsPaused { get; private set; }
    public static float TimeScale { get; set; }

    public delegate void PauseEnable();
    public static event PauseEnable OnPauseEnable;
    public delegate void PauseDisable();
    public static event PauseDisable OnPauseDisable;

    public static void TogglePause()
    {
        //Turn off pause
        if (IsPaused)
        {
            //resets timescale to timescale value prior to pause
            Time.timeScale = TimeScale;
            IsPaused = false;
            OnPauseDisable?.Invoke();
            Time.timeScale = 1.0f;
            Debug.Log("Pause disabled");
        }
        //Turn on pause
        else
        {
            //saves latest timescale used before pause
            TimeScale = Time.timeScale;
            Time.timeScale = 0;
            IsPaused = true;
            OnPauseEnable?.Invoke();
            Debug.Log("Pause enabled");
        }
    }
}
