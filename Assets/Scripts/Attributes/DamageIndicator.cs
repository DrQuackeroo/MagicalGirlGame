using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Basic component for any object that has a health script and wants to display a damage indicator
/// whenever the object takes damage. As long as the object has a health script and this damage indicator
/// script, the damage will be displayed whenever the object takes damage.
///
/// The script has a modifiable vertical offset in case there is a need for a slight adjustment.
/// Takes in a damage text prefab to be instantiated.
/// </summary>
public class DamageIndicator : MonoBehaviour
{
    [SerializeField] private GameObject _damageTextPrefab;
    [SerializeField] private float _yOffset = 0;

    public void CreateDamageIndicator(int damage, Vector3 objectPosition, float yColliderBounds)
    {
        objectPosition += new Vector3(0, yColliderBounds + _yOffset, 0);
        GameObject damageText = Instantiate(_damageTextPrefab, objectPosition, Quaternion.identity);
        damageText.transform.GetChild(0).GetComponent<TextMesh>().text = damage.ToString();
        Destroy(damageText, 0.5f);
    }
}
