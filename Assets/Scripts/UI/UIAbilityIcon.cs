using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityIcon : MonoBehaviour
{
    [SerializeField] private Slider _slider;

    [SerializeField] private Image _backgroundImage;

    [SerializeField] private Image _fillImage;

    /*
    
    
    Pseudocode until the hierarchy code for abilities have been implemented

    // UIAbilityIconsManager will set up a UIAbilityIcon for each ability the player has selected. On every Update Call in UIAbilityIconsManager, it will pass the necessary information
    to the ability's corresponding UTAbilityIcon
    public void SetUpAbilityIconUI(Image image){
    {
        _backgroundImage = image;
        _fillImage = image;
    }
    
    public void UpdateAbilityIconUI(float currentTimer, float cooldown)
    {
        _slider.value = currentTimer / cooldown;
    }
    */
}