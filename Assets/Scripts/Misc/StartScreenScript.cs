using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenScript : MonoBehaviour
{
    private string[] _messages = {
        GlobalStrings.START_MSG0,
        GlobalStrings.START_MSG1,
        GlobalStrings.START_MSG2,
        GlobalStrings.START_MSG3
    };
    private int _currentMsg = 0;
    private bool _fadeingTextIn = false;
    private bool _fadeingTextOut = false;
    private bool _holdingText = false;

    [SerializeField] Transitions _transition;
    [SerializeField] TextMeshProUGUI _text;
    [SerializeField] TextMeshProUGUI _backStory;

    [SerializeField] float _backStorySpeed = 5f;

    [SerializeField] float _delayBetweenMsgTime = 2f;
    [SerializeField] float _fadeinTime = 4f;
    [SerializeField] float _textFadeTime = 1f;
    [SerializeField] float _textHoldTime = 3f;


    private EasyTimer _fadeOutTimer;
    private EasyTimer _msgDelayTimer;
    private EasyTimer _fadeInTimer;
    private EasyTimer _textFadeTimer;
    private EasyTimer _textHoldTimer;

    private Color _textColor = new Color(84, 0, 60, 0);

    private bool _done = false;

    private enum Phase
    {
        Messages, TextFade, FadeOut, Transit
    }
    private Phase _phase = Phase.Messages;

    void Awake()
    {
        _transition.Value = 0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        _fadeOutTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
        _msgDelayTimer = new EasyTimer(_delayBetweenMsgTime);
        _fadeInTimer = new EasyTimer(_fadeinTime);
        _textFadeTimer = new EasyTimer(_textFadeTime);
        _textHoldTimer = new EasyTimer(_textHoldTime);

        GameManager.Instance.InputManager.HeroPressedButton += StartProceed;

        _fadeOutTimer.Reset();
        _msgDelayTimer.Reset();
        _fadeInTimer.Reset();
        _textFadeTimer.Reset();

        GameManager.Instance.Music.PlayNow(Music.OPENINGTHEME);
    }



    private void StartProceed(object sender, HeroMovement e)
    {
        _phase = Phase.FadeOut;
        _fadeOutTimer.Reset();
        _transition.TransitionType = TransitionType.CircleFade;
    }

    void Update()
    {
        if (_done) return;

        switch (_phase)
        {
            case Phase.Messages:
                _transition.TransitionType = TransitionType.FadeToBlack;
                _transition.Value = _fadeInTimer.Ratio;
                _text.color = new Color(0, 0, 0, 0);
                if (_msgDelayTimer.Done && !_fadeingTextIn && !_holdingText && !_fadeingTextOut)
                {
                    _textFadeTimer.Reset();
                    _fadeingTextIn = true;
                    _backStory.text = _messages[_currentMsg];
                    _backStory.color = new Color(1f, 1f, 1f, _textFadeTimer.Ratio);
                }

                if (_fadeingTextIn && !_holdingText)
                {
                    _backStory.color = new Color(1f, 1f, 1f, _textFadeTimer.Ratio);
                    if (_textFadeTimer.Done)
                    {
                        _fadeingTextIn = false;
                        _holdingText = true;
                        _textHoldTimer.Reset();
                    }
                }

                if (_holdingText)
                {
                    _backStory.color = new Color(1f, 1f, 1f, 1f);
                    if (_textHoldTimer.Done)
                    {
                        _holdingText = false;
                        _fadeingTextOut = true;
                        _textFadeTimer.Reset();
                    }
                }

                if (_fadeingTextOut)
                {
                    _backStory.color = new Color(1f, 1f, 1f, 1 - _textFadeTimer.Ratio);
                    if ( _textFadeTimer.Done)
                    {
                        _fadeingTextOut = false;
                        _currentMsg++;
                    }
                }

                break;

            case Phase.TextFade:
                _text.color = new Color(_textColor.r, _textColor.g, _textColor.b, _textFadeTimer.Ratio);
                break;

            case Phase.FadeOut:
                _transition.Value = 1 - _fadeOutTimer.Ratio;
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
