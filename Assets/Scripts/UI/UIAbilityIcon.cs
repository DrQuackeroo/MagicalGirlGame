using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIAbilityIcon : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _cooldownText;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Image _fillImage;

    private IEnumerator _cooldownCoroutine;

    // for testing
    public Sprite _testSprite;

    private void Awake()
    {
        _cooldownText.text = string.Empty;
    }
    
    public void SetUpAbilityIconUI(Sprite sprite)
    {
        _backgroundImage.sprite = sprite;
        _fillImage.sprite = sprite;
    }

    public void StartCooldown(float cooldown)
    {
        if (_cooldownCoroutine != null)
        {
            StopCoroutine(_cooldownCoroutine);
            _cooldownCoroutine = null;
        }

        _cooldownCoroutine = UpdateAbilityIconUI(cooldown);
        StartCoroutine(_cooldownCoroutine);
    }

    private IEnumerator UpdateAbilityIconUI(float cooldown)
    {
        float timeElapsed = 0;

        while (timeElapsed < cooldown)
        {
            timeElapsed += Time.deltaTime;

            _cooldownText.text = string.Format("{0:N2}", cooldown - timeElapsed);

            _slider.value = Mathf.Min(timeElapsed / cooldown, 1);

            yield return null;
        }

        _cooldownText.text = string.Empty;
    }
}