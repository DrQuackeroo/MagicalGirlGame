using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityButton : MonoBehaviour
{
    private AbilityUI _abilityUI;
    private string _abilityName;
    private Toggle _toggle;

    // Start is called before the first frame update
    void Start()
    {
        _toggle = GetComponent<Toggle>();
    }

    /// <summary>
    /// Set up variables for this AbilityButton. Sets the button images associated with the Ability.
    /// </summary>
    /// <param name="abilityUI">The AbilityUI script in this scene.</param>
    /// <param name="ability">The ability associated with this button. The player clicks this button to select/deselect this ability.</param>
    public void Initialize(AbilityUI abilityUI, Ability ability)
    {
        _abilityUI = abilityUI;
        _abilityName = ability.GetName();
        transform.Find("Background").GetComponent<Image>().sprite = ability.GetIcon();
        transform.Find("Background").Find("Checkmark").GetComponent<Image>().sprite = ability.GetIcon();
    }

    /// <summary>
    /// Called when corresponding ability button is clicked.
    /// </summary>
    /// <param name="isOn">The new state of the ability button.</param>
    public void ToggleAbilityButton(bool isOn)
    {
        bool result = _abilityUI.TrySelectAbility(_abilityName, isOn);
        if (!result)
            _toggle.isOn = false;
    }
}
