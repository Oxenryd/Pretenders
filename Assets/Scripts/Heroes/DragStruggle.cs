using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class DragStruggle : MonoBehaviour
{
    private const float FLICKER_TIME = 0.05f;
    private const float FADE_TIME = 0.5f;
    private Camera _camera;
    [SerializeField] private CircleMeter _meter;
    [SerializeField] private Image _image;
    [SerializeField] private Sprite _sprite0;
    [SerializeField] private Sprite _sprite1;
    [SerializeField] private bool _active = false;
    [SerializeField][Range(0f, 1f)] private float _value;

    private float _minValue = 0;
    private bool _fading = false;
    private EasyTimer _fadeTimer;
    private EasyTimer _imageTimer;
    private EasyTimer _maxStruggleTime;
    private ICharacterMovement _dragger;
    private ICharacterMovement _dragged;

    public void Abort()
    {
        _dragger.DragStruggle = null;
        _dragger.IsDraggingOther = false;
        _dragger.IsGrabbing = false;
        _dragged.ReleaseFromDrag();
        _fading = true;
        _active = false;
        _fadeTimer.Reset();
    }

    public bool Active
    { get { return _active; } set {  _active = value; } }

    void Awake()
    {
        _imageTimer = new EasyTimer(FLICKER_TIME);      
        _fadeTimer = new EasyTimer(FADE_TIME);
        _maxStruggleTime = new EasyTimer(GlobalValues.CHAR_STRUGGLE_MAX_TIME);
        _camera = Camera.main;
    }

    public void Activate(ICharacterMovement dragger, ICharacterMovement dragged)
    {
        _maxStruggleTime.Reset();
        gameObject.SetActive(true);
        _active = true;
        _fading = true;
        _dragger = dragger;
        _dragged = dragged;

        var hero1 = dragger.GameObject.GetComponent<Hero>();
        var hero2 = dragged.GameObject.GetComponent<Hero>();

        var col = Color.Lerp(hero1.PrimaryColor, hero2.PrimaryColor, 0.5f);

        _meter.Color = new Color(col.r, col.g, col.b, 1f);
        _image.color = new Color(col.r, col.g, col.b, 1f);
        _imageTimer.Reset();
        _fadeTimer.Reset();
        _value = 0.5f;
        _meter.Value = _value;
        var delta = dragged.GameObject.transform.position - dragger.GameObject.transform.position;
        transform.position = _camera.WorldToScreenPoint(_dragger.GameObject.transform.position + delta + new Vector3(0, 1.5f, 0));
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

        var delta = _dragged.GameObject.transform.position - _dragger.GameObject.transform.position;
        transform.position = _camera.WorldToScreenPoint(_dragger.GameObject.transform.position + delta + new Vector3(0, 1.5f, 0));


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


        _value = Math.Max(_maxStruggleTime.Ratio, _value);
        _meter.Value = _value;

        if (_value >= 1f)
        {
            Abort();
        }
    }

    private void doFadeIn()
    {
        _meter.transform.parent.transform.localScale = Vector3.one * _fadeTimer.Ratio;
        if (_fadeTimer.Done)
            _fading = false;
    }

    private void doFadeOut()
    {
        _meter.transform.parent.transform.localScale = Vector3.one - (Vector3.one * _fadeTimer.Ratio);
        if (_fadeTimer.Done)
            gameObject.SetActive(false);
    }
}