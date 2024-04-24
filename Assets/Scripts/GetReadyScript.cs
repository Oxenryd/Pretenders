using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetReadyScript : MonoBehaviour
{
    [SerializeField] private bool _enabled = true;

    [SerializeField] private TextMeshProUGUI _ready;
    [SerializeField] private TextMeshProUGUI _three;
    [SerializeField] private TextMeshProUGUI _two;
    [SerializeField] private TextMeshProUGUI _one;
    [SerializeField] private TextMeshProUGUI _go;

    public event EventHandler CountdownComplete;
    protected void OnCountdownComplete()
    { CountdownComplete?.Invoke(this, EventArgs.Empty); }

    private bool _active = false;

    private HeroMovement[] _heroes;

    private float _counter = 0;

    private EasyTimer _startDelayTimer;
    private EasyTimer _countdown;
    enum State
    {
        StartDelay, GetReady, Three, Two, One, Go, Outro
    }

    private State _state = State.StartDelay;

    public void Activate()
    {
        if (!_enabled)
        {
            Debug.Log("GetReady is not enabled. Will not activate.");
            return;
        }
        _state = State.StartDelay;
        _active = true;
        _startDelayTimer.Reset();
        _countdown.Reset();
        _counter = 0;
        foreach (var hero in _heroes)
        {
            hero.AcceptInput = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _startDelayTimer = new EasyTimer(1.6f);
        _countdown = new EasyTimer(1f);
        var heroes = GameObject.FindGameObjectsWithTag(GlobalStrings.CHARACTER_TAG);
        _heroes = new HeroMovement[heroes.Length];
        for (int i = 0; i < heroes.Length; i++)
        {
            _heroes[i] = heroes[i].GetComponent<HeroMovement>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_enabled) return;
        if (!_active) return;

        _counter += GameManager.Instance.DeltaTime;

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
                    _two.enabled = true;
                    proceed(State.Two);
                }
                break;

            case State.Two:
                _three.color = new Color(_three.color.r, _three.color.g, _three.color.b, 1 - Mathf.Clamp(_countdown.Ratio * 2, 0f, 1f));
                _three.rectTransform.localScale = (1 - _countdown.Ratio) * Vector3.one;

                _two.rectTransform.localScale = (Mathf.Clamp(_countdown.Ratio, 0f, 0.5f) * -38f + 20) * Vector3.one;
                _two.color = new Color(_two.color.r, _two.color.g, _two.color.b, Mathf.Clamp(_countdown.Ratio * 3, 0f, 1f));
                if (_countdown.Done)
                {
                    _three.enabled = false;
                    _countdown.Reset();
                    _one.enabled = true;
                    proceed(State.One);
                }
                break;

            case State.One:
                _two.color = new Color(_two.color.r, _two.color.g, _two.color.b, 1 - Mathf.Clamp(_countdown.Ratio * 2, 0f, 1f));
                _two.rectTransform.localScale = (1 - _countdown.Ratio) * Vector3.one;

                _one.rectTransform.localScale = (Mathf.Clamp(_countdown.Ratio, 0f, 0.5f) * -38f + 20) * Vector3.one;
                _one.color = new Color(_one.color.r, _one.color.g, _one.color.b, Mathf.Clamp(_countdown.Ratio * 3, 0f, 1f));
                if (_countdown.Done)
                {
                    _two.enabled = false;
                    _countdown.Reset();
                    _go.enabled = true;
                    proceed(State.Go);
                }
                break;

            case State.Go:
                _one.color = new Color(_one.color.r, _one.color.g, _one.color.b, 1 - Mathf.Clamp(_countdown.Ratio * 2, 0f, 1f));
                _one.rectTransform.localScale = (1 - _countdown.Ratio) * Vector3.one;

                _go.rectTransform.localScale = (Mathf.Clamp(_countdown.Ratio, 0f, 0.5f) * -38f + 20) * Vector3.one;
                _go.color = new Color(_go.color.r, _go.color.g, _go.color.b, Mathf.Clamp(_countdown.Ratio * 3, 0f, 1f));

                _go.rectTransform.localScale = (0.05f * Mathf.Cos(_counter * 20f) + 1) * Vector3.one;

                if (_countdown.Done)
                {
                    _one.enabled = false;
                    _countdown.Reset();
                    proceed(State.Outro);
                    OnCountdownComplete();
                    foreach (var hero in _heroes)
                    {
                        hero.AcceptInput = true;
                    }
                }
                break;

            case State.Outro:
                _go.rectTransform.localScale = (0.05f * Mathf.Cos(_counter * 20f) + 1) * Vector3.one;
                _go.color = new Color(_go.color.r, _go.color.g, _go.color.b, 1 - Mathf.Clamp(_countdown.Ratio * 2, 0f, 1f));
                if ( _countdown.Done)
                {
                    _go.enabled = false;
                    _active = false;
                }
                break;
        }
        
    }

    private void proceed(State next)
    {
        _state = next;
    }
}
