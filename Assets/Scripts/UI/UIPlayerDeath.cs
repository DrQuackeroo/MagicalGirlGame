using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIPlayerDeath : MonoBehaviour
{
    [SerializeField] LayerMask _playerLayer;

    [SerializeField] private GameObject _gameOverScreenUI;

    [SerializeField] private List<Image> _UIImageElements;

    [SerializeField] private List<TextMeshProUGUI> _UITextElements;

    [SerializeField] private float _transitionTime;

    private bool _clickedButton = false;

    private void Awake()
    {
        Add();

        SetUpUIElements();
    }

    private void OnEnable()
    {
        Add();
    }

    private void OnDisable()
    {
        Remove();
    }

    private void Add()
    {
        FindPlayer()?.eventHasDied.AddListener(OnPlayerDeath);
    }

    private void Remove()
    {
        FindPlayer()?.eventHasDied.RemoveListener(OnPlayerDeath);
    }

    public void OnPlayerDeath()
    {
        Remove();

        _gameOverScreenUI.SetActive(true);

        StartCoroutine(TransitionUIElements());
    }

    public void OnClickRetry()
    {
        if (!_clickedButton)
        {
            _clickedButton = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }
    }

    public void OnClickReturnToTitle()
    {/*
        if (!_clickedButton)
        {
            _clickedButton = true;

            // returns the player to the title screen; will update when title screen scene is added
        }
        */
    }

    private Health FindPlayer()
    {
        GameObject[] goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        for (int i = 0; i < goArray.Length; i++)
        {
            if (_playerLayer == (_playerLayer | (1 << goArray[i].layer)))
            {
                return goArray[i].GetComponent<Health>();
            }
        }

        return null;
    }

    private void SetUpUIElements()
    {
        for (int i = 0; i < _UIImageElements.Count; i++)
        {
            Color c = _UIImageElements[i].color;
            _UIImageElements[i].color = new Color(c.r, c.g, c.b, 0f);
        }

        for (int j = 0; j < _UITextElements.Count; j++)
        {
            Color c = _UITextElements[j].color;
            _UITextElements[j].color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    private IEnumerator TransitionUIElements()
    {
        float transition = 0;

        while (transition  < _transitionTime)
        {
            transition = Mathf.Min(_transitionTime, transition + Time.deltaTime);
            for (int i = 0; i < _UIImageElements.Count; i++)
                TransitionUIImage(_UIImageElements[i], transition / _transitionTime);

            for (int j = 0; j < _UITextElements.Count; j++)
                TransitionUIText(_UITextElements[j], transition / _transitionTime);

            print(255 * transition / _transitionTime);

            yield return null;
        }
    }

    private void TransitionUIImage(Image image, float opacity)
    {
        Color c = image.color;
        image.color = new Color(c.r, c.g, c.b, opacity);
    }

    private void TransitionUIText(TextMeshProUGUI text, float opacity)
    {
        Color c = text.color;
        text.color = new Color(c.r, c.g, c.b, opacity);
    }
}