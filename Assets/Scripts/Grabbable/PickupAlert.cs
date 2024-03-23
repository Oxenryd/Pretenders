using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;



public class PickupAlert : MonoBehaviour
{
    private const string TUG = "Tug!!";
    private const string GRAB = "Grab!";

    private Camera _camera;
    [SerializeField] private float _animTime = 0.3f;
    [SerializeField] private float _wobbleTextTime = 0.1f;
    [SerializeField] private float _wobbleButtonTime = 0.4f;
    [SerializeField] private float _pingCooldown = 0.15f;
    [SerializeField] private RectTransform _alert;
    [SerializeField] private UnityEngine.UI.Image _image;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private float _wobbleRange = 10f;
    [SerializeField] private float _textStartSize = 36f;
    [SerializeField] private Vector3 _positionOffset = new Vector3(0, 3, 0f);

    private Transform _target;

    private bool _signalled = false;


    private Vector3 _orgScale = Vector3.one;
    private EasyTimer _animateTimer;
    private EasyTimer _deAnimateTimer;
    private EasyTimer _wobbleTextTimer;
    private EasyTimer _wobbleButtonTimer;
    private EasyTimer _keepAliveTimer;
    private bool _textGrowing = true;
    private AlertMode _mode = AlertMode.Inactive;

    public AlertMode Mode
    { get { return _mode; } }

    public void Ping(HeroMovement icm, Transform grabbableTransform, bool tug)
    {
        if (!_signalled && _mode != AlertMode.Active)
        {
            if (tug)
                _text.text = TUG;
            else
                _text.text = GRAB;
            _signalled = true;
            var hero = icm.GameObject.GetComponent<Hero>();
            var color = hero.PrimaryColor;
            Activate(grabbableTransform, color);
        }

        _keepAliveTimer.Reset();
    }


    public void Deactivate()
    {
        if (_mode != AlertMode.DeAnimating)
            _deAnimateTimer.Reset();
        _mode = AlertMode.DeAnimating;
        
    }
    public void Hide()
    {
        _image.enabled = false;
        _text.enabled = false;
    }

    public void Activate(Transform target, Color color)
    {
        _image.enabled = true;
        _text.enabled = true;
        _image.color = new Color(color.r, color.g, color.b, 1f);
        _text.color = _image.color;
        _text.fontSize = _textStartSize - _wobbleRange * 0.5f;
        _mode = AlertMode.Animating;
        _target = target;
        gameObject.SetActive(true);
        _animateTimer.Reset();
    }

    // Start is called before the first frame update
    void Awake()
    {
        _keepAliveTimer = new EasyTimer(_pingCooldown);
        _animateTimer = new EasyTimer(_animTime);
        _deAnimateTimer = new EasyTimer(_animTime/2);
        _wobbleTextTimer = new EasyTimer(_wobbleTextTime);
        _wobbleButtonTimer = new EasyTimer(_wobbleButtonTime);
        _camera = Camera.main;
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = _camera;
    }

    // Update is called once per frame
    void Update()
    {
        _alert.parent.transform.position = _camera.WorldToScreenPoint(_target.position + _positionOffset);


        if (_signalled && _keepAliveTimer.Done)
        {
            _signalled = false;
            _deAnimateTimer.Reset();
            _mode = AlertMode.DeAnimating;
        } else if (_signalled)
        {
            if (_mode != AlertMode.Active)
                _mode = AlertMode.Animating;
        }

        switch (_mode)
        {
            case AlertMode.Animating: doAnimating();
                return;
                
            case AlertMode.DeAnimating: doDeAnimating();
                return;
                
            case AlertMode.Inactive: return;
        }

       if (_wobbleTextTimer.Done)
       {
           _textGrowing = !_textGrowing;
           _text.fontSize = _textGrowing ? _textStartSize - _wobbleRange * 0.5f : _textStartSize + _wobbleRange * 0.5f;
           _wobbleTextTimer.Reset();
       }
        if (_textGrowing)
            _text.fontSize = _textStartSize - _wobbleRange * 0.5f + _wobbleTextTimer.Ratio * _wobbleRange;
        else
            _text.fontSize = _textStartSize + _wobbleRange * 0.5f - _wobbleTextTimer.Ratio * _wobbleRange;

    }

    private void doAnimating()
    {
        _alert.parent.transform.localScale = Vector3.one * _animateTimer.Ratio;
        if (_animateTimer.Done)
            _mode = AlertMode.Active;
    }

    private void doDeAnimating()
    {
        _alert.parent.transform.localScale = _orgScale - (_deAnimateTimer.Ratio * _orgScale);
        if (_deAnimateTimer.Done)
        {
            _mode = AlertMode.Inactive;
            gameObject.SetActive(false);
        }
    }
}
