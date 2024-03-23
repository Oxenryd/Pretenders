using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TransferAlert : MonoBehaviour
{
    private Camera _camera;
    [SerializeField] private float _animTime = 0.15f;
    [SerializeField] private float _pingCooldown = 0.15f;
    [SerializeField] private RectTransform _alert;
    [SerializeField] private UnityEngine.UI.Image _image;
    [SerializeField] private float _wobbleRange = 1f;
    [SerializeField] private Vector3 _positionOffset = new Vector3(0, 1f, 0f);

    private Transform _target;
    private float _counter = 0;
    private EasyTimer _animateTimer;
    private EasyTimer _deAnimateTimer;
    private EasyTimer _keepAliveTimer;
    private bool _signalled = false;
    private AlertMode _mode = AlertMode.Inactive;

    public AlertMode Mode
    { get { return _mode; } }


    public void Deactivate()
    {
        if (_mode != AlertMode.DeAnimating)
            _deAnimateTimer.Reset();
        _mode = AlertMode.DeAnimating;

    }

    public void Ping(HeroMovement icm, Transform recievableTransform)
    {
        if (!_signalled && Mode != AlertMode.Active)
        {
            _signalled = true;
            var hero = icm.GameObject.GetComponent<Hero>();
            var color = hero.PrimaryColor;
            Activate(recievableTransform, color);
        }
        
        _keepAliveTimer.Reset();
    }

    public void Hide()
    {
        _image.enabled = false;
    }

    public void Activate(Transform target, Color color)
    {
        _target = target;
        _alert.transform.position = _camera.WorldToScreenPoint(_target.position + _positionOffset);
        _counter = 0;
        _image.enabled = true;
        _image.color = new Color(color.r, color.g, color.b, 1f);
        _mode = AlertMode.Animating;
        gameObject.SetActive(true);
        _animateTimer.Reset();
    }

    // Start is called before the first frame update
    void Awake()
    {
        _animateTimer = new EasyTimer(_animTime);
        _deAnimateTimer = new EasyTimer(_animTime);
        _keepAliveTimer = new EasyTimer(_pingCooldown);
        _camera = Camera.main;
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = _camera;
    }

    // Update is called once per frame
    void Update()
    {

        _alert.transform.position = _camera.WorldToScreenPoint(_target.position + _positionOffset);
        var scaledY = _camera.scaledPixelHeight * (0.015f * Mathf.Cos(_counter * 15) + 0.015f);
        _image.rectTransform.localPosition = new Vector3(0, scaledY, 0);
        _alert.localScale = Vector3.one * (0.1f * Mathf.Cos(_counter * 8) + 1);

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
            case AlertMode.Animating:
                doAnimating();
                return;

            case AlertMode.DeAnimating:
                doDeAnimating();
                return;

            case AlertMode.Inactive: return;
        }
        _counter += GameManager.Instance.DeltaTime;  
    }

    private void doAnimating()
    {
        _alert.transform.localScale = Vector3.one * _animateTimer.Ratio;
        if (_animateTimer.Done)
        {
            _mode = AlertMode.Active;
        }
    }

    private void doDeAnimating()
    {
        _alert.transform.localScale = Vector3.one - (_deAnimateTimer.Ratio * Vector3.one);
        if (_deAnimateTimer.Done)
        {
            _mode = AlertMode.Inactive;
            gameObject.SetActive(false);
        }
    }
}
