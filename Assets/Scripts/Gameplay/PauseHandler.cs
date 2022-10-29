using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static void UnPause(bool turnOffPause)
    {
        IsPaused = turnOffPause;
        TogglePause();
    }

    public static void UnpauseIfPaused()
    {
        if (IsPaused || TimeScale == 0) UnPause(true);
    }
}
