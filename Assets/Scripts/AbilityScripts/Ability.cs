using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [SerializeField] protected string _displayName = "";
    [SerializeField] protected Sprite _imageIcon = null;
    [TextArea(3, 10), SerializeField] protected string _description = "";
    [SerializeField] protected float _cooldown = 0;
    [Tooltip("SFX is played when this Ability is used.")]
    [SerializeField] protected AudioClip _soundEffect;
    [Tooltip("Effect object created when Ability is used. Can be left empty for no effect.")]
    [SerializeField] protected GameObject _effectObject;

    public bool IsOnCooldown { get; private set; }

    public string GetName() { return _displayName; }
    public Sprite GetIcon() { return _imageIcon; }
    public string GetDescription() { return _description; }
    public float GetCooldown() { return _cooldown; }

    public abstract void Activate(GameObject player);
    // Called when the ability button is released.
    public abstract void Deactivate(GameObject player);

    public virtual IEnumerator ActivateCooldown()
    {
        IsOnCooldown = true;
        UIAbilityIconsManager.ShowCooldown(this);
        yield return new WaitForSeconds(_cooldown);
        IsOnCooldown = false;
    }
}