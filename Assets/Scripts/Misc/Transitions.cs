using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Transitions : MonoBehaviour
{
    [SerializeField] private TransitionType _transitionType = TransitionType.FadeToBlack;
    [SerializeField][Range(0f, 1f)] private float _value = 0f;

    [SerializeField] private Image _image;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Material _mat;
    [SerializeField] private Camera _cam;

    private int _transValueIndex;
    private int _aspectIndex;
    private int _circleBoolIndex;
    private int _fadeBoolIndex;

    public TransitionType TransitionType
    {  get { return _transitionType; }set { _transitionType = value; } }
    public float Value
    { get { return _value; } set {  _value = value; } }

    void Start()
    {
        //DontDestroyOnLoad(this);
        _transValueIndex = Shader.PropertyToID("_transitionValue");
        _aspectIndex = Shader.PropertyToID("_aspectRatio");
        _fadeBoolIndex = Shader.PropertyToID("_fadeTransition");
        _circleBoolIndex = Shader.PropertyToID("_circleTransition");
        _image.material = new Material(_mat);
    }
    void LateUpdate()
    {
        if (_value >= 1f)
        {
            _image.enabled = false;
            _value = 1f;
            return;
        }
        else if (_value < 0f)
        {
            _value = 0f;
        }

        var ratio = _cam.pixelRect.width / _cam.pixelRect.height;
        _image.enabled = true;
       // _image.rectTransform.sizeDelta = _cam.pixelRect.size;
        
        _image.material.SetFloat(_aspectIndex, ratio);

        switch (_transitionType)
        {
            case TransitionType.FadeToBlack:
                _image.color = new Color(0,0,0, 1 - _value);
                _image.material.SetFloat(_transValueIndex, 0);
                break;
            case TransitionType.CircleFade:
                _image.color = new Color(0, 0, 0, 1);
                _image.material.SetFloat(_transValueIndex, _value);
                break;
        }
    }
}
