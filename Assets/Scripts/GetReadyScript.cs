using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetReadyScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ready;
    [SerializeField] private TextMeshProUGUI _three;
    [SerializeField] private TextMeshProUGUI _two;
    [SerializeField] private TextMeshProUGUI _one;
    [SerializeField] private TextMeshProUGUI _go;

    private bool _active = false;

    private EasyTimer _startDelayTimer;
    private EasyTimer _countdown;
    enum State
    {
        StartDelay, GetReady, Three, Two, One, Go
    }

    private State _state = State.StartDelay;

    public void Activate()
    {
        _state = State.StartDelay;
        _active = true;
        _startDelayTimer.Reset();
        _countdown.Reset();
    }

    // Start is called before the first frame update
    void Start()
    {
        _startDelayTimer = new EasyTimer(1.6f);
        _countdown = new EasyTimer(1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!_active) return;

        switch (_state)
        {
            case State.StartDelay:
                if (_startDelayTimer.Done)
                {
                    proceed(State.GetReady);
                    _startDelayTimer.Reset();
                }
                break;

            case State.GetReady:
                _ready.enabled = true;
                if (_startDelayTimer.Done)
                {
                    proceed(State.Three);
                    _countdown.Reset();
                    _ready.enabled = false;
                    _three.enabled = true;
                }
                break;

            case State.Three:
                _three.rectTransform.localScale = (Mathf.Clamp(_countdown.Ratio, 0f, 0.5f) * -38f + 20) * Vector3.one;
                _three.color = new Color(_three.color.r, _three.color.g, _three.color.b, Mathf.Clamp(_countdown.Ratio * 3, 0f, 1f));
                if (_countdown.Done)
                {
                    _countdown.Reset();
                    proceed(State.Two);
                }
                break;

            case State.Two:
                _three.color = new Color(_three.color.r, _three.color.g, _three.color.b, 1 - Mathf.Clamp(_countdown.Ratio * 2, 0f, 1f));
                _three.rectTransform.localScale = (1 - _countdown.Ratio) * Vector3.one;
                break;
        }
        
    }

    private void proceed(State next)
    {
        _state = next;
    }
}
