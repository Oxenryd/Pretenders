using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Simple class that handles the bobbing and displaying of winner text in matches.
/// </summary>
public class WinnerTextScript : MonoBehaviour
{
    private bool _active = false;
    [SerializeField] private TextMeshProUGUI _text;
    private EasyTimer _timer;
    private float _counter = 0;
    [SerializeField] private float _wobbleSpeed = 10f;
    [SerializeField] private float _wobbleAmp = 0.3f;
    public void Activate()
    {
        _active = true;
        _timer.Reset();
        _text.enabled = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        _timer = new EasyTimer(1f);
        _text.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_active) return;

        _counter += GameManager.Instance.DeltaTime;
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, _timer.Ratio);

        float wobbleScale = _wobbleAmp * Mathf.Cos(_counter * _wobbleSpeed) + 1f;

        _text.rectTransform.localScale = new Vector3(wobbleScale, wobbleScale, 1);
    }
}
