using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AbilityHandler
{
    public static List<Ability> CurrentAbilities = new List<Ability>();

    public delegate void SetAbilityDel(List<Ability> abs);
    public static event SetAbilityDel OnSetAbility;

    public delegate void AbilityMenu();
    public static event AbilityMenu OnAbilityMenuEnter;

    public static void UpdatePlayerAbilities(List<Ability> abs)
    {
        CurrentAbilities = abs;
        OnSetAbility?.Invoke(abs);
    }

    public static void EnterAbilityMenu()
    {
        OnAbilityMenuEnter?.Invoke();
    }

    public static void ClearCurrentAbilities()
    {
        CurrentAbilities.Clear();
    }
}
