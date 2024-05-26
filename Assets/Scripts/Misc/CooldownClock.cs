using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class that controls and behaviour of a cooldown and its clock meter visual.
/// </summary>
public class CooldownClock : MonoBehaviour
{
    private Camera _camera;

    [SerializeField] private Image _visare;
    [SerializeField] private Image _ram;
    [SerializeField][Range(0f, 1f)] private float _value;
    [SerializeField][Range(0f, 1f)] private float _maxAlpha = 1f;
    [SerializeField] private Material _material;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Vector3 _posOffset = Vector3.zero;
    private Transform _target;
    private EasyTimer _timer;
    private EasyTimer _shaderTimer;
    private EasyTimer _fadeInTimer;
    private float _counter = 0;
    private int _valueId;
    private int _alphaId;
    private bool _fadingIn = false;
    private float _actualAlpha;
    public Vector3 PositionOffset
    { get { return _posOffset; } set { _posOffset = value; } } 
    public Camera Camera { get { return _camera; } }
    public RectTransform RectTransform { get { return _rectTransform; } }
    public bool Active { get; set; } = false;
    public float Value
    { get { return _value; } set { _value = value; } }
    public float MaxBaseAlpha
    { get { return _maxAlpha; } set { _maxAlpha = value; } }


    public event EventHandler CooldownDone;
    protected void OnCooldownDone()
    { CooldownDone?.Invoke(this, EventArgs.Empty); }


    /// <summary>
    /// Make the clock visible and reset the cooldown timer.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="coolDownTime"></param>
    public void Activate(Transform target, float coolDownTime)
    {
        _fadingIn = true;
        _fadeInTimer.Reset();
        _target = target;
        gameObject.SetActive(true);
        Active = true;
        _timer.Time = coolDownTime;
        _timer.Reset();      
    }

    /// <summary>
    /// Hidew the clock.
    /// </summary>
    public void Deactivate()
    {
        Active = false;
        gameObject.SetActive(false);
    }




    // Start is called before the first frame update
    void Awake()
    {
        _camera = Camera.main;
        _material = Instantiate(_material);
        _ram.material = _material;
        _shaderTimer = new EasyTimer(0.016667f);
        _timer = new EasyTimer(1f);
        _fadeInTimer = new EasyTimer(1f);
        _valueId = Shader.PropertyToID("_Value");
        _alphaId = Shader.PropertyToID("_BaseAlpha");

        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = _camera;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active) return;

        if (_fadingIn && !_fadeInTimer.Done)
        {
            _actualAlpha = Mathf.Clamp(_fadeInTimer.Ratio, 0f, _maxAlpha);
        } else if (_fadeInTimer.Done)
        {
            _fadingIn = false;
        }

        if (_shaderTimer.Done)
        {
            var angle = (1 - _value) * 360f;
            _visare.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
            _ram.material.SetFloat(_valueId, _value);
            _ram.material.SetFloat(_alphaId, _actualAlpha);
            _shaderTimer.Reset();
        }
        _counter += GameManager.Instance.DeltaTime;
        var yScale = (0.05f * Mathf.Cos(_counter * 6) + 1);
        var xScale = (0.05f * Mathf.Sin(_counter * 6) + 1);
        var scaledY = _camera.scaledPixelHeight * (0.0001f * Mathf.Sin(_counter * 6) + 0.0001f);
        _rectTransform.position = _camera.WorldToScreenPoint(_target.position + new Vector3(0, scaledY, 0) + _rectTransform.localRotation * PositionOffset);
        _rectTransform.localScale = new Vector3(1 * xScale, 1 * yScale, 1);

        if (_value >= 0.999f)
        {
            Deactivate();
            OnCooldownDone();            
        }
    }
}
