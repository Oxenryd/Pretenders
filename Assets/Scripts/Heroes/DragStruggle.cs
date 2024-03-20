using System;
using UnityEngine;
using UnityEngine.UI;

public class DragStruggle : MonoBehaviour
{
    private const float FLICKER_TIME = 0.0666f;
    private const float FADE_TIME = 0.5f;
    private Camera _camera;
    [SerializeField] private CircleMeter _meter;
    [SerializeField] private Image _image;
    [SerializeField] private Sprite _sprite0;
    [SerializeField] private Sprite _sprite1;
    [SerializeField] private bool _active = false;
    [SerializeField][Range(0f, 1f)] private float _value;

    private bool _fading = false;
    private EasyTimer _fadeTimer;
    private EasyTimer _imageTimer;
    private ICharacterMovement _dragger;
    private ICharacterMovement _dragged;

    public bool Active
    { get { return _active; } set {  _active = value; } }

    void Start()
    {
        _imageTimer = new EasyTimer(FLICKER_TIME);      
        _fadeTimer = new EasyTimer(FADE_TIME);
        _camera = Camera.main;
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = _camera;
    }

    public void Activate(ICharacterMovement dragger, ICharacterMovement dragged)
    {
        var delta = dragged.GameObject.transform.position - dragger.GameObject.transform.position;
        transform.position = dragger.GameObject.transform.position + delta + new Vector3(0, 1.5f, 0);
        gameObject.SetActive(true);
        _active = true;
        _fading = true;
        _dragger = dragger;
        _dragged = dragged;
        _imageTimer.Reset();
        _fadeTimer.Reset();
        _value = 0;
    }

    public void Increase(float amount)
    { _value += Mathf.Max(0, amount); }
    public void Decrease(float amount)
    { _value -= Mathf.Min(0, amount); }

    void Update()
    {
        if (!_active && _fading)
        {
            doFadeOut();
            return;
        }
        if (!_active) return;

        if (_active && _fading)
        {
            doFadeIn();
            return;
        }

        if (_imageTimer.Done)
        {
            if (_imageTimer.CyclesEven)
                _image.sprite = _sprite0;
            else
                _image.sprite = _sprite1;
            _imageTimer.Reset();
        }

        _meter.Value = _value;

        if (_value >= 1f)
        {
            _dragged.IsDraggedByOther = false;
            _dragger.IsDraggingOther = false;
            _fading = true;
            _active = false;
            _fadeTimer.Reset();
        }
    }

    private void doFadeIn()
    {
        _meter.transform.parent.transform.localScale = Vector3.one * _fadeTimer.Ratio;
    }

    private void doFadeOut()
    {
        _meter.transform.parent.transform.localScale = Vector3.one - (Vector3.one * _fadeTimer.Ratio);
        if (_fadeTimer.Done)
            gameObject.SetActive(false);
    }
}