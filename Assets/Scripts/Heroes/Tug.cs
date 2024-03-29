using System;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Tug : MonoBehaviour
{
    private const float FLICKER_TIME = 0.05f;
    private const float FADE_TIME = 0.5f;
    private Camera _camera;
    private Vector3 _metersOffset = new Vector3(0, 2.5f, 0);
    private Vector3 _buttonOffset = new Vector3(0, 3f,0);
    private Vector3 _textOffset = new Vector3(0, 0.5f, 0);

    [SerializeField] private CircleMeter _meter1;
    [SerializeField] private CircleMeter _meter2;
    [SerializeField] private RectTransform _meter1RectTransform;
    [SerializeField] private RectTransform _meter2RectTransform;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Image _image;
    [SerializeField] private Sprite _sprite0;
    [SerializeField] private Sprite _sprite1;
    [SerializeField] private bool _active = false;
    [SerializeField][Range(0f, 1f)] private float _value;
    [SerializeField] private float _textStartSize = 36f;
    [SerializeField] private float _wobbleRange = 10f;
    [SerializeField] private float _wobbleTextTime = 0.1f;

    private RectTransform _canvasTransform;
    private Vector3 _delta;

    private Vector3 _meter1OrgScale;
    private Vector3 _meter2OrgScale;
    private Vector3 _imageOrgScale;
    private Vector3 _textOrgScale;

    private Grabbable _thisGrabbable;

    private bool _fading = false;
    private EasyTimer _wobbleTextTimer;
    private EasyTimer _fadeTimer;
    private EasyTimer _imageTimer;
    private EasyTimer _maxStruggleTime;
    private HeroMovement _hero1;
    private HeroMovement _hero2;

    private bool _textGrowing = true;


    public float Duration
    { get; set; } = 0;
    public float Ratio
    { get { return Duration / _maxStruggleTime.Time; } }
    public void Abort()
    {
        _fading = true;
        _active = false;
        _fadeTimer.Reset();
    }

    public Grabbable Grabbable
    { get { return _thisGrabbable; } set { _thisGrabbable = value; } }

    public bool Active
    { get { return _active; } set { _active = value; } }

    void Awake()
    {
        _imageTimer = new EasyTimer(FLICKER_TIME);
        _fadeTimer = new EasyTimer(FADE_TIME);
        _maxStruggleTime = new EasyTimer(GlobalValues.TUG_MAX_TUG_TIME);
        _wobbleTextTimer = new EasyTimer(_wobbleTextTime);
        _camera = Camera.main;
        transform.localScale = Vector3.one;
        _meter1OrgScale = _meter1RectTransform.localScale;
        _meter2OrgScale = _meter2RectTransform.localScale;
        _imageOrgScale = _image.rectTransform.localScale;
        _textOrgScale = _text.rectTransform.localScale;
        
    }

    void Start()
    {
        _canvasTransform = GameObject.FindGameObjectWithTag(GlobalStrings.NAME_UIOVERLAY).GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }

    public void SetMaxTime(float maxTime)
    {
        _maxStruggleTime = new EasyTimer(maxTime);
        _maxStruggleTime.Reset();
    }

    public void Activate(HeroMovement hero1, HeroMovement hero2)
    {
        Duration = 0;
        _maxStruggleTime.Reset();
        gameObject.SetActive(true);
        _active = true;
        _fading = true;
        _hero1 = hero1;
        _hero2 = hero2;

        _value = 0.5f;
        _imageTimer.Reset();
        _fadeTimer.Reset();

        var heroOfHero1 = hero1.GameObject.GetComponent<Hero>();
        var heroOfHero2 = hero2.GameObject.GetComponent<Hero>();

        var color1 = heroOfHero1.PrimaryColor; 
        _meter1.Color = new Color(color1.r, color1.g, color1.b, 1f);
        var color2 = heroOfHero2.PrimaryColor;
        _meter2.Color = new Color(color2.r, color2.g, color2.b, 1f);

        _meter2.Value = _value;
        _meter1.Value = _value;

        _delta = _hero2.GameObject.transform.position - _hero1.GameObject.transform.position;
        

        _meter1.Activate(_hero1.transform.position + _metersOffset);
        _meter2.Activate(_hero2.transform.position + _metersOffset);

        _text.rectTransform.localPosition = _camera.WorldToScreenPoint(_hero1.transform.position + _delta * 0.5f + _textOffset) / _canvasTransform.localScale.x;
        _image.rectTransform.localPosition = _camera.WorldToScreenPoint(_hero1.transform.position + _delta * 0.5f + _buttonOffset) / _canvasTransform.localScale.x;
        _meter1RectTransform.localPosition = _camera.WorldToScreenPoint(_hero1.transform.position + _metersOffset) / _canvasTransform.localScale.x;
        _meter2RectTransform.localPosition = _camera.WorldToScreenPoint(_hero2.transform.position + _metersOffset) / _canvasTransform.localScale.x;
    }

    public void Increase(float amount)
    { _value += amount * (MathF.Pow(_maxStruggleTime.Ratio, 2) * GlobalValues.TUG_TIME_MULTIPLIER + 1); }

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

        _text.rectTransform.localPosition = _camera.WorldToScreenPoint(_hero1.transform.position + _delta * 0.5f + _textOffset) / _canvasTransform.localScale.x;
        _image.rectTransform.localPosition = _camera.WorldToScreenPoint(_hero1.transform.position + _delta * 0.5f + _buttonOffset) / _canvasTransform.localScale.x;
        _meter1RectTransform.localPosition = _camera.WorldToScreenPoint(_hero1.transform.position + _metersOffset) / _canvasTransform.localScale.x;
        _meter2RectTransform.localPosition = _camera.WorldToScreenPoint(_hero2.transform.position + _metersOffset) / _canvasTransform.localScale.x;


        if (_imageTimer.Done)
        {
            if (_imageTimer.CyclesEven)
                _image.sprite = _sprite0;
            else
                _image.sprite = _sprite1;
            _imageTimer.Reset();
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


        Duration += GameManager.Instance.DeltaTime;

        // Keeping the values rational to 1 and each other.
        _meter1.Value = _value + 1 - (2 * _value);
        _meter2.Value = _value;

        winningCondition();
    }

    protected virtual void winningCondition()
    {      
        if (_value >= 1f) // Hero2 winner
        {
            Grabbable.Drop();

            _hero1.ReleaseFromTug(false);
            _hero2.ReleaseFromTug(true);

            Grabbable.Grab(_hero2);
          
            _hero1.TryShove(-_hero1.FaceDirection, _hero2);
            _hero2.TryBump(-_hero2.FaceDirection, GlobalValues.CHAR_BUMPFORCE);

            Abort();
        } else if (_value <= 0f) // Hero1 winner
        {
            Grabbable.Drop();

            _hero1.ReleaseFromTug(true);
            _hero2.ReleaseFromTug(false);

            Grabbable.Grab(_hero1);
                    
            _hero2.TryShove(-_hero2.FaceDirection, _hero1);
            _hero1.TryBump(-_hero1.FaceDirection, GlobalValues.CHAR_BUMPFORCE);

            Abort();
        }
    }

    private void doFadeIn()
    {
        Duration += GameManager.Instance.DeltaTime;
        _meter1RectTransform.localScale = _meter1OrgScale * _fadeTimer.Ratio;
        _meter2RectTransform.localScale = _meter2OrgScale * _fadeTimer.Ratio;
        _image.rectTransform.localScale = _imageOrgScale * _fadeTimer.Ratio;
        _text.rectTransform.localScale = _textOrgScale * _fadeTimer.Ratio;
        if (_fadeTimer.Done)
            _fading = false;
    }

    private void doFadeOut()
    {
        _meter1RectTransform.localScale = _meter1OrgScale - (_meter1OrgScale * _fadeTimer.Ratio);
        _meter2RectTransform.localScale = _meter2OrgScale - (_meter2OrgScale * _fadeTimer.Ratio);
        _image.rectTransform.localScale = _imageOrgScale - (_imageOrgScale * _fadeTimer.Ratio);
        _text.rectTransform.localScale = _textOrgScale - (_textOrgScale * _fadeTimer.Ratio);

        if (_fadeTimer.Done)
            gameObject.SetActive(false);
    }
}
