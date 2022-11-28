using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Spawns the names for the credits that rain down on the Player. The Player can push the text objects around using physics.
/// </summary>
public class CreditsRain : MonoBehaviour
{
    private const float _spaceBetweenLines = 3.5f;
    private const float _spaceBetweenWords = 1f;

    [SerializeField] private GameObject _creditsTextPrefab;
    [SerializeField] private string _fullCreditsText = "Sample Text";

    /// <summary>
    /// Instantiate and space out the words in the credits.
    /// </summary>
    public void StartCredits()
    {
        Vector3 offset = Vector3.zero;

        foreach (string line in _fullCreditsText.Split('|'))
        {
            offset.x = 0.0f;
            foreach (string word in line.Split(" "))
            {
                GameObject wordObject = Instantiate(_creditsTextPrefab, transform.position, Quaternion.identity, transform);
                TextMeshPro _text = wordObject.GetComponent<TextMeshPro>();
                _text.text = word;
                _text.ForceMeshUpdate();
                BoxCollider box = wordObject.AddComponent<BoxCollider>();
                box.center = _text.bounds.center;
                box.size = _text.bounds.extents * 2.0f;

                // Position the words in lines and rows.
                offset.x += box.size.x / 2.0f;
                wordObject.transform.Translate(offset);
                offset.x += box.size.x / 2.0f + _spaceBetweenWords;
            }

            offset.y += _spaceBetweenLines;
        }
    }
}
