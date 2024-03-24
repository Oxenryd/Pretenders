using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CooldownClock : MonoBehaviour
{
    private Camera _camera;

    [SerializeField] private Image _visare;
    [SerializeField] private Image _ram;
    [SerializeField][Range(0f, 1f)] private float _value;
    [SerializeField] private Material _material;
    [SerializeField] private RectTransform _rectTransform;

    private Vector3 _targetPosition;

    private EasyTimer _timer;
    private EasyTimer _shaderTimer;
    private float _counter = 0;
    public bool Active { get; set; } = true;
    private void Activate(Vector3 position, float coolDownTime)
    {
        _timer.Time = coolDownTime;
        _timer.Reset();
        gameObject.SetActive(true);
    }

    private int _valueId;
    // Start is called before the first frame update
    void Awake()
    {
        //var bodyRend = transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
        //bodyRend.sharedMaterial = new Material(_primMat);
        //bodyRend.sharedMaterial.color = _primaryColor;
        //var headRend = transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>();
        //headRend.sharedMaterial = new Material(_secMat);
        //headRend.sharedMaterial.color = _secondaryColor;
        _camera = Camera.main;
        _material = Instantiate(_material);
        _ram.material = _material;
        _shaderTimer = new EasyTimer(0.016667f);
        _timer = new EasyTimer(1f);
        _valueId = Shader.PropertyToID("_Value");


        // Remove later
        _targetPosition = _rectTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active) return;

        if (_shaderTimer.Done)
        {
            var angle = (1 - _value) * 360f;
            _visare.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
            _ram.material.SetFloat(_valueId, _value);
            _shaderTimer.Reset();
        }
        _counter += GameManager.Instance.DeltaTime;
        //(0.1f * Mathf.Cos(_counter * 8) + 1)
        var yScale = (0.05f * Mathf.Cos(_counter * 6) + 1);
        var xScale = (0.05f * Mathf.Sin(_counter * 6) + 1);
        var scaledY = _camera.scaledPixelHeight * (0.001f * Mathf.Sin(_counter * 6) + 0.001f);
        _rectTransform.localPosition = new Vector3(0, scaledY, 0);
        _rectTransform.localScale = new Vector3(1 * xScale, 1 * yScale, 1); //new Vector3(1 * xScale, 1 * yScale, 1);
    }
}
