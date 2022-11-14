using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the values for the slider component with values respective to the assigned player object's health.
/// </summary>
public class UIHealthBar : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private Sprite _playerIcon100;
    [SerializeField] private Sprite _playerIcon75;
    [SerializeField] private Sprite _playerIcon50;
    [SerializeField] private Sprite _playerIcon25;
    private Image _playerIcon;
    private Slider _slider;
    
    void Start()
    {
        // Assign the slider component
        _playerIcon = gameObject.transform.GetChild(0).transform.GetChild(2).GetComponent<Image>();
        _slider = gameObject.transform.GetChild(0).transform.GetChild(1).GetComponent<Slider>();
        // Set the min and max values and the current value of the slider
        _slider.minValue = 0;
        _slider.maxValue = _player.GetComponent<Health>().GetMaxHealth();
        _slider.value = _player.GetComponent<Health>().GetHealth();
    }

    void Update()
    {
        // Update slider value
        int currentHealth = _player.GetComponent<Health>().GetHealth();
        _slider.value = currentHealth;
        // Update the sprite to match with the current health percentage
        int maxHealth = _player.GetComponent<Health>().GetMaxHealth();
        int maxHealth75Percent = (int)(maxHealth * 0.75f);
        int maxHealth50Percent = (int)(maxHealth * 0.5f);
        int maxHealth25Percent = (int)(maxHealth * 0.25f);
        if (maxHealth75Percent < currentHealth) { _playerIcon.sprite = _playerIcon100; }
        else if (maxHealth50Percent < currentHealth && currentHealth <= maxHealth75Percent) { _playerIcon.sprite = _playerIcon75; }
        else if (maxHealth25Percent < currentHealth && currentHealth <= maxHealth50Percent) { _playerIcon.sprite = _playerIcon50; }
        else { _playerIcon.sprite = _playerIcon25; }
    }
}
