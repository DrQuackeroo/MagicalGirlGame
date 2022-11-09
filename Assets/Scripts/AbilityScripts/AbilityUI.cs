using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AbilityUI : MonoBehaviour
{
    // At most how many abilities the Player can select.
    private const int MaxAbilities = 3;
    
    [SerializeField] private GameObject _allAbilitiesHolder;
    [Tooltip("The grid parent GameObject of the Ability Buttons.")]
    [SerializeField] private GameObject _abilityButtonGrid;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private List<TMP_Dropdown> _dropdownList;
    [SerializeField] private TMP_Text _errorText;

    [Tooltip("Reference to the Ability Button prefab asset. Will be spawned at runtime for each Ability")]
    [SerializeField] private GameObject _abilityButtonPrefab;

    private Dictionary<string, Ability> allAbilitiesDict = new Dictionary<string, Ability>();
    private List<string> _selectedAbilityNames = new List<string>();

    //First get all abilities from abilities holder gameobject (get all scripts that inherit Abilty)
    //Next, add them to a dictionary where key = ability name (serialized field on script), value = ability script
    //Then, add all the abilities and a null value to the drop downs' options
    private void Awake()
    {
        Ability[] abilities = _allAbilitiesHolder.GetComponents<Ability>();
        foreach (Ability ability in abilities)
        {
            allAbilitiesDict.Add(ability.GetName(), ability);

            GameObject newButton = Instantiate(_abilityButtonPrefab, _abilityButtonGrid.transform);
            newButton.GetComponent<UIAbilityButton>().Initialize(this, ability.GetName());
        }
        foreach (TMP_Dropdown dd in _dropdownList)
        {
            dd.ClearOptions();
            dd.options.Add(new TMP_Dropdown.OptionData() { text = "" });
            foreach (string s in allAbilitiesDict.Keys)
            {
                dd.options.Add(new TMP_Dropdown.OptionData() { text = s });
            }
        }
        //Default abilities - add first three abilities from abilityholder as default abilities
        AbilityHandler.ClearCurrentAbilities();
        for (int i = 0; i < 3; i++)
        {
            AbilityHandler.CurrentAbilities.Add(abilities[i]);
        }

        for (int i = 0; i < MaxAbilities; i++)
        {
            _selectedAbilityNames.Add("");
        }
    }

    private void Start()
    {
        
    }

    private void OnEnable()
    {
        AbilityHandler.OnAbilityMenuEnter += EnterMenu;
    }

    private void OnDisable()
    {
        AbilityHandler.OnAbilityMenuEnter -= EnterMenu;
    }

    /// <summary>
    /// Tries to add abilityName to the list of selected Abilities if the button is now on. Otherwise, remove the associated Ability from
    /// the current selected Abilities.
    /// </summary>
    /// <param name="abilityName">The ability button that was clicked.</param>
    /// <param name="isOn">True if the ability is now selected.</param>
    /// <returns>True if the Ability was successfully added to the list of selected Abilities. Otherwise, return false.</returns>
    public bool TrySelectAbility(string abilityName, bool isOn)
    {
        // Try to add this Ability to _selectedAbilityNames
        if (isOn)
        {
            int i = _selectedAbilityNames.FindIndex(name => name == "");

            if (i == -1)
            {
                // We are unable to add the Ability.
                // TODO: Change error message
                Debug.LogError("Unable To add ability");
                foreach (string s in _selectedAbilityNames)
                    print(s);
                return false;
            }

            _selectedAbilityNames[i] = abilityName;
        }
        // Remove this Ability from _selectedAbilityNames
        else
        {
            int i = _selectedAbilityNames.FindIndex(name => name == abilityName);
            if (i > -1)
            {
                _selectedAbilityNames[i] = "";
            }
        }

        foreach (string s in _selectedAbilityNames)
            print(s);

        return true;
    }

    //Custom exceptions for confirm button
    public class NullAbilityChosenError : Exception { }
    public class RepeatAbilityError : Exception { }

    //For each drop down, add the ability to a list, throw any errors (repeated/null ability)
    //If successful in previous step, then turn the name list into an Ability list using the dictionary
    //Finally, send new ability list to player and turn off menu
    public void ConfirmButton()
    {
        try
        {
            List<string> ability_names = new List<string>();
            foreach (TMP_Dropdown dd in _dropdownList)
            {
                string temp = dd.options[dd.value].text;
                if (temp.Equals("")) throw new NullAbilityChosenError();
                if (ability_names.Contains(temp)) throw new RepeatAbilityError();
                ability_names.Add(temp);
            }
            _errorText.text = "";
            List<Ability> ability_real = new List<Ability>();
            foreach (string name in ability_names)
            {
                ability_real.Add(allAbilitiesDict[name]);
            }
            AbilityHandler.UpdatePlayerAbilities(ability_real);
            ExitMenu();
        }
        catch (NullAbilityChosenError)
        {
            _errorText.text = "Error: Must choose an ability for each dropdown.";
        }
        catch (RepeatAbilityError)
        {
            _errorText.text = "Error: Each chosen ability must be unique.";
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("Error in choosing ability: {}", e.Message));
        }
    }

    //Here in case I need to add more to cancel button
    public void CancelButton()
    {
        if (AbilityHandler.CurrentAbilities.Count == 0)
        {
            _errorText.text = "Error: You must set your abilities intially.";
            return;
        }
        ExitMenu();
    }

    private void ExitMenu()
    {
        _canvas.gameObject.SetActive(false);
        PauseHandler.UnpauseIfPaused();
    }

    private void EnterMenu()
    {
        _canvas.gameObject.SetActive(true);
    }

}

