using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class AbilityUI : MonoBehaviour
{
    
    [SerializeField] private GameObject _allAbilitiesHolder;

    [SerializeField] private Canvas _canvas;
    [SerializeField] private List<TMP_Dropdown> _dropdownList;
    [SerializeField] private TMP_Text _errorText;

    private Dictionary<string, Ability> allAbilitiesDict = new Dictionary<string, Ability>();

    //First get all abilities from abilities holder gameobject (get all scripts that inherit Abilty)
    //Next, add them to a dictionary where key = ability name (serialized field on script), value = ability script
    //Then, add all the abilities and a null value to the drop downs' options
    private void Awake()
    {
        Ability[] abilities = _allAbilitiesHolder.GetComponents<Ability>();
        foreach (Ability ability in abilities)
        {
            allAbilitiesDict.Add(ability.GetName(), ability);
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
    }

    private void OnEnable()
    {
        AbilityHandler.OnAbilityMenuEnter += EnterMenu;
    }

    private void OnDisable()
    {
        AbilityHandler.OnAbilityMenuEnter -= EnterMenu;
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

