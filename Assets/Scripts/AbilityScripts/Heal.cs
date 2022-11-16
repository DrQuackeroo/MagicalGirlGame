using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : Ability
{
    [SerializeField] private int _healPerTick = 0;

    [SerializeField] private float _tickDelay = 0;

    [SerializeField] private int _numberOfTicks = 1;

    private IEnumerator _healingCoroutine = null;

    public override void Activate(GameObject player)
    {
        _healingCoroutine = ActivateHeal(player);
        StartCoroutine(_healingCoroutine);
    }

    public override void Deactivate(GameObject player)
    {
        CancelHeal(player);
    }

    private IEnumerator ActivateHeal(GameObject player)
    {
        PlayerControls controls = player.GetComponent<PlayerControls>();
        Health health = player.GetComponent<Health>();

        if (controls != null)
        {
            controls.isInputLocked = true;
            controls.velocity.x = 0.0f;
        }

        float timeSinceHealed = 0;
        
        for(int i = 0; i < _numberOfTicks; i++)
        {
            yield return new WaitForSeconds(_tickDelay);

            health?.HealHealth(_healPerTick);
        }

        CancelHeal(player);
    }

    private void CancelHeal(GameObject player)
    {
        if (_healingCoroutine == null)
            return;

        PlayerControls controls = player.GetComponent<PlayerControls>();

        if (controls != null)
            controls.isInputLocked = false;

        StopCoroutine(_healingCoroutine);
        _healingCoroutine = null;

        StartCoroutine(ActivateCooldown());
    }
}
