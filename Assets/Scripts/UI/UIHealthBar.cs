using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the values for the slider component and HP text with values respective to the assigned player object's health.
/// </summary>
public class UIHealthBar : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    private TextMeshProUGUI _currentHealthText;
    private Slider _slider;
    
    void Start()
    {
        // Get the object's necessary components
        _currentHealthText = gameObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        _slider = gameObject.GetComponent<Slider>();
        // Set the min and max values and the current value of the slider
        int maxHealth = _player.GetComponent<Health>().GetMaxHealth();
        int currentHealth = _player.GetComponent<Health>().GetHealth();
        _slider.minValue = 0;
        _slider.maxValue = maxHealth;
        _slider.value = currentHealth;
        _currentHealthText.text = currentHealth.ToString();
    }

    void Update()
    {
        if (_player == null)  // If null, the player has died. Values should be set to zero and then disable this class
        {
            _slider.value = 0;
            _currentHealthText.text = "0";
            enabled = false;
        }
        else
        {
            int currentHealth = _player.GetComponent<Health>().GetHealth();
            _slider.value = currentHealth;
            _currentHealthText.text = currentHealth.ToString();
        }
    }
}
