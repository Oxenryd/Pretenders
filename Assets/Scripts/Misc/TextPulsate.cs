using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextPulsate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _textSize = 36;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _amplitudePercentage = 0.2f;

    private float _counter = 0;

    // Update is called once per frame
    void Update()
    {
        _counter += GameManager.Instance.DeltaTime;
        _text.fontSize = _textSize * (_amplitudePercentage * Mathf.Cos(_counter * _speed)) + _textSize;
    }
}
