using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : Ability
{
    [SerializeField] private int _healPerTick = 0;

    [SerializeField] private float _tickDelay = 0;

    private IEnumerator _healingCoroutine = null;

    public override void Activate(GameObject player)
    {
        _healingCoroutine = ActivateHeal(player);
        StartCoroutine(_healingCoroutine);
    }

    public override void Deactivate(GameObject player)
    {
        PlayerControls controls = player.GetComponent<PlayerControls>();

        if (controls != null)
            controls.isInputLocked = false;

        StopCoroutine(_healingCoroutine);
        _healingCoroutine = null;

        ActivateCooldown();
    }

    private IEnumerator ActivateHeal(GameObject player)
    {
        PlayerControls controls = player.GetComponent<PlayerControls>();
        Health health = player.GetComponent<Health>();

        if (controls != null)
            controls.isInputLocked = true;

        float timeSinceHealed = 0;
        
        while(true)
        {
            timeSinceHealed += Time.deltaTime;

            if (timeSinceHealed >= _tickDelay)
            {
                timeSinceHealed = 0;

                health?.HealHealth(_healPerTick);
            }

            yield return null;
        }
    }
}
