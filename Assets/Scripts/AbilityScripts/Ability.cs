using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [SerializeField] protected string _displayName = "";
    [SerializeField] protected Sprite _imageIcon = null;
    [TextArea(3, 10), SerializeField] protected string _description = "";

    public string GetName() { return _displayName; }
    public Sprite GetIcon() { return _imageIcon; }
    public string GetDescription() { return _description; }

    public abstract void Activate(GameObject player);
}