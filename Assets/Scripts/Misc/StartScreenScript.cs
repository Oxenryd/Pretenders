using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenScript : MonoBehaviour
{
    [SerializeField] Transitions _transition;
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField] TextMeshProUGUI _backStory;
    [SerializeField] TextMeshProUGUI _backStory2;

    [SerializeField] float _backStorySpeed = 5f;

    [SerializeField] float _delayTime = 10f;
    [SerializeField] float _fadeinTime = 4f;
    [SerializeField] float _textFadeTime = 1f;
    [SerializeField] float _story2Delay = 10f;
    [SerializeField] float _story2fadeTime = 1f;


    private EasyTimer _fadeOutTimer;
    private EasyTimer _startDelayTimer;
    private EasyTimer _fadeInTimer;
    private EasyTimer _textFadeTimer;
    private EasyTimer _story2DelayTimer;
    private EasyTimer _story2FadeTimer;

    private Color _textColor = new Color(84, 0, 60, 0);

    private bool _done = false;
    private bool _fadingInStory2 = false;

    private enum Phase
    {
        Delay, FadeIn, TextFade, FadeOut, Transit
    }
    private Phase _phase = Phase.Delay;

    void Awake()
    {
        _transition.Value = 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        _fadeOutTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
        _startDelayTimer = new EasyTimer(_delayTime);
        _fadeInTimer = new EasyTimer(_fadeinTime);
        _textFadeTimer = new EasyTimer(_textFadeTime);
        _story2DelayTimer = new EasyTimer(_story2Delay);
        _story2FadeTimer = new EasyTimer(_story2fadeTime);
        GameManager.Instance.InputManager.HeroPressedButton += StartProceed;

        _fadeOutTimer.Reset();
        _startDelayTimer.Reset();
        _fadeInTimer.Reset();
        _textFadeTimer.Reset();
        _story2DelayTimer.Reset();
        _story2FadeTimer.Reset();

        _backStory2.color = new Color(_backStory2.color.r, _backStory2.color.g, _backStory2.color.b, 0);

        GameManager.Instance.Music.PlayNow(Music.OPENINGTHEME);
    }



    private void StartProceed(object sender, HeroMovement e)
    {
        //var hero = e.GetComponent<Hero>();
        //if (hero.Index != 0) return;
        _phase = Phase.FadeOut;
        _fadeOutTimer.Reset();
        _transition.TransitionType = TransitionType.CircleFade;
    }

    void Update()
    {
        if (_done) return;

        if (_story2DelayTimer.Done && ( _phase == Phase.Delay || _phase == Phase.FadeIn) && !_fadingInStory2)
        {
            _fadingInStory2 = true;
            _story2FadeTimer.Reset();
            
        }
        if (_fadingInStory2)
        {
            _backStory2.color = new Color(_backStory2.color.r, _backStory2.color.g, _backStory2.color.b, _story2FadeTimer.Ratio);
        }

        switch (_phase)
        {
            case Phase.Delay:
                _transition.Value = 0;
                _backStory.rectTransform.position = new Vector3(
                    _backStory.rectTransform.position.x,
                    _backStory.rectTransform.position.y + GameManager.Instance.DeltaTime * _backStorySpeed,
                    _backStory.rectTransform.position.z);

                if (_startDelayTimer.Done)
                {
                    _phase = Phase.FadeIn;
                    _fadeInTimer.Reset();
                }


                break;

            case Phase.FadeIn:
                _transition.TransitionType = TransitionType.FadeToBlack;
                _transition.Value = _fadeInTimer.Ratio;
                _backStory2.color = new Color(_backStory2.color.r, _backStory2.color.g, _backStory2.color.b, 1 - _fadeInTimer.Ratio);
                _text.color = new Color(0, 0, 0, 0);
                if (_fadeInTimer.Done)
                {             
                    _phase = Phase.TextFade;
                    _textFadeTimer.Reset();
                }
                break;

            case Phase.TextFade:
                _backStory2.enabled = false;
                _text.color = new Color(_textColor.r, _textColor.g, _textColor.b, _textFadeTimer.Ratio);
                _backStory2.color = new Color(0, 0, 0, 0);
                break;

            case Phase.FadeOut:
                _transition.Value = 1 - _fadeOutTimer.Ratio;
                _backStory2.color = new Color(0, 0, 0, 0);
                if (_fadeOutTimer.Done)
                    _phase = Phase.Transit;                             
                break;

            case Phase.Transit:              
                    doProceed();
                    _done = true;
                break;
        }
    }

    private void doProceed()
    {
        GameManager.Instance.TransitToNextScene(GlobalStrings.SCENE_LOBBY);
    }
}
