using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlUI : MonoBehaviour
{
    [SerializeField] private GameObject _canvas;
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private bool _saveControlsBetweenSessions;

    private void OnEnable()
    {
        PauseHandler.OnControlScreenEnable += OnEnableControlUI;

        //From "RebindSaveLoad.cs"
        if (_saveControlsBetweenSessions)
        {
            var rebinds = PlayerPrefs.GetString("rebinds");
            if (!string.IsNullOrEmpty(rebinds))
                _inputActions.LoadBindingOverridesFromJson(rebinds);
        }

    }

    private void OnDisable()
    {
        PauseHandler.OnControlScreenEnable -= OnEnableControlUI;
        //From "RebindSaveLoad.cs"
        if (_saveControlsBetweenSessions)
        {
            var rebinds = _inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
        }
    }

    public void ResetBindings()
    {
        foreach (InputActionMap map in _inputActions.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
        PlayerPrefs.DeleteKey("rebinds");
    }

    public void OnEnableControlUI()
    {
        _canvas.gameObject.SetActive(true);
    }

    public void OnDisableControlUI()
    {
        _canvas.gameObject.SetActive(false);
        PauseHandler.CloseControlsMenu();
    }
}
