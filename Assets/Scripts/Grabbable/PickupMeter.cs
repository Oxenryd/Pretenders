using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PickupMeter : MonoBehaviour
{
    public bool Active = false;
    private Camera _camera;
    [SerializeField] private Vector3 _positionOffset = new Vector3(0, 1, 0);
    [SerializeField] private RectTransform _meter;
    [SerializeField] private float _defaultLengthScale = 8;
    [SerializeField][Range(0f, 1f)] private float _value;
    private EasyTimer _timer;
    private Vector3 _spawnPosition;
    private bool _aborted = false;
    private bool _paused = false;
    public event EventHandler PickupComplete;
    public event EventHandler PickupAborted;
    private Material _shader;
    protected void OnPickupComplete()
    { PickupComplete?.Invoke(this, EventArgs.Empty); }
    protected void OnPickupAborted()
    { PickupAborted?.Invoke(this, EventArgs.Empty); }

    public float Value { get { return _value; } set {  _value = value; } }

    public void Abort()
    {
        gameObject.SetActive(false);
        _aborted = true;
    }
    public void Activate(Vector3 position)
    {
        _spawnPosition = _camera.WorldToScreenPoint(position);
        gameObject.SetActive(true);
        Active = true;
        _timer.Reset();
    }

    // Start is called before the first frame update
    void Awake()
    {
        _timer = new EasyTimer(GlobalValues.CHAR_GRAB_PICKUPTIME);
        _camera = Camera.main;
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = _camera;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active) return;

        _value = _timer.Ratio;
        var scale = _meter.localScale;
        var delta = 100f;
        _meter.parent.position = new Vector3(_spawnPosition.x, _spawnPosition.y, 0);
        _meter.localScale = new Vector3(_value * _defaultLengthScale, scale.y, scale.z);
        _meter.localPosition = new Vector3(-delta + _value * delta, 0, 0);      

        if (_timer.Done)
        {
            _aborted = false;
            Active = false;
            OnPickupComplete();
            gameObject.SetActive(false);
        }
        if (_aborted)
        {
            OnPickupAborted();
            Active = false;
            _aborted = false;
            gameObject.SetActive(false);
        }
    }
}
