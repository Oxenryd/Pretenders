using UnityEngine;
using UnityEngine.UI;

public class DragStruggle : MonoBehaviour
{
    private const float FLICKER_TIME = 0.0666f;

    [SerializeField] private CircleMeter _meter;
    [SerializeField] private Image _image;
    [SerializeField] private Sprite _sprite0;
    [SerializeField] private Sprite _sprite1;
    [SerializeField] private bool _active = false;
    [SerializeField][Range(0f, 1f)] private float _value;

    private EasyTimer _imageTimer;

    public bool Active
    { get { return _active; } set {  _active = value; } }

    void Start()
    {
        _imageTimer = new EasyTimer(FLICKER_TIME);
    }

    void Update()
    {
        if (!_active) return;
        
        if (_imageTimer.Done)
        {
            if (_imageTimer.CyclesEven)
                _image.sprite = _sprite0;
            else
                _image.sprite = _sprite1;
            _imageTimer.Reset();
        }
    }
}