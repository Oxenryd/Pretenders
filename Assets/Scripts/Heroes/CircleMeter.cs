using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleMeter : MonoBehaviour
{
    public bool Active = false;
    private Camera _camera;
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _meter;
    [SerializeField] private float _defaultLengthScale = 10;
    [SerializeField][Range(0f, 1f)] private float _value;
    private float _counter = 0;

    public float Value { get { return _value; } set { _value = value; } }
    public Color Color
    { get { return _image.color; } set { _image.color = value; } }

    public void Deactivate()
    {
        Active = false;
        gameObject.SetActive(false);        
    }
    public void Activate(Vector3 position)
    {
        transform.position = _camera.WorldToScreenPoint(position);
        gameObject.SetActive(true);
        Active = true;
    }

    // Start is called before the first frame update
    void Awake()
    {
        _camera = Camera.main;
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = _camera;
    }

    // Update is called once per frame
    void Update()
    {
       // if (!Active) return;

        var scale = _meter.localScale;
        _counter += GameManager.Instance.DeltaTime;
        var cos = 0.03f * MathF.Cos(_counter * 8) + 1;

        _meter.localScale = new Vector3((_value * cos) * _defaultLengthScale, (_value * cos) * _defaultLengthScale, scale.z);
    }
}
