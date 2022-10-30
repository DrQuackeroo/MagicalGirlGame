using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAbilityIconsManager : MonoBehaviour
{
    [SerializeField] private static Dictionary<string, UIAbilityIcon> _abilityIconsDict = new Dictionary<string, UIAbilityIcon>();

    [SerializeField] private GameObject[] _abilityIcons;

    private void OnEnable()
    {
        AbilityHandler.OnSetAbility += SetIcons;
    }

    private void OnDisable()
    {
        AbilityHandler.OnSetAbility -= SetIcons;
    }

    public void SetIcons(List<Ability> abilities)
    {
        _abilityIconsDict.Clear();
        
        for (int i = 0; i < abilities.Count; i++)
        {
            string abilityName = abilities[i].GetName();
            _abilityIconsDict.Add(abilityName, _abilityIcons[i].GetComponent<UIAbilityIcon>());

            _abilityIconsDict[abilityName].SetUpAbilityIconUI(abilities[i].GetIcon());
        }
    }

    public static void ShowCooldown(Ability ability)
    {
        _abilityIconsDict[ability.GetName()].StartCooldown(ability.GetCooldown());
    }
}