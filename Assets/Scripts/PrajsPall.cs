using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Class that handles the behaviours of the result screen shown after a tournament.
/// </summary>
public class PrajsPall : MonoBehaviour
{
    [SerializeField] private Color[] _colors;
    [SerializeField] private Vector3[] _positions;
    [SerializeField] private Vector3[] _rotations;
    [SerializeField] private Transitions _transitions;
    [SerializeField] private Light _firstLight;
    [SerializeField] private Light _secondLight;
    [SerializeField] private Light _thirdLight;
    [SerializeField] private HeroMovement[] _heroes;
    [SerializeField] private TextMeshProUGUI _congrats;

    [SerializeField] private bool _debug = false;

    private EasyTimer _congratsTimer;
    private EasyTimer _fadeTimer;
    private bool _fadingIn = true;
    private bool _fadeintOut = false;
    private bool _congratsDelay = true;
    private bool _firstFrame = true;

    void Start()
    {
        _transitions.TransitionType = TransitionType.FadeToBlack;
        if (_debug)
        {
            GameManager.Instance.StartNewTournament();
            GameManager.Instance.AddNewMatchResult(new MatchResult(GameType.Lobby, MatchResult.GenerateRandomStandings()));
            GameManager.Instance.AddNewMatchResult(new MatchResult(GameType.Lobby, MatchResult.GenerateRandomStandings()));
            GameManager.Instance.AddNewMatchResult(new MatchResult(GameType.Lobby, MatchResult.GenerateRandomStandings()));
        }

        GameManager.Instance.InputManager.HeroPressedButton += onHeroPressed;

        _fadeTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
        _fadeTimer.Reset();
        _congratsTimer = new EasyTimer(1.5f);
        _congratsTimer.Reset();

        var results = MatchResult.GetPlayersStandings(GameManager.Instance.GetMatchResults());
        if (_debug)
            Debug.Log(results.ToString());
        for (int i = 0; i < 4; i++)
        {
            _heroes[i].transform.position = _positions[results[i]];
            _heroes[i].transform.rotation = Quaternion.Euler(0, _rotations[results[i]].y, 0);
        }

        int indexFirstPlace = getIndexInPosition(0, results);
        int indexSecondPlace = getIndexInPosition(1, results);
        int indexThirdPlace = getIndexInPosition(2, results);
            

        _firstLight.color = _colors[indexFirstPlace];
        _secondLight.color = _colors[indexSecondPlace];
        _thirdLight.color = _colors[indexThirdPlace];
    }

    private void onHeroPressed(object sender, HeroMovement e)
    {
        if (!_congratsTimer.Done) return;

        if (!e.AcceptInput)
        {
            if (e.JumpButtonDown || e.TriggerButtonDown || e.GrabButtonDown)
            {
                GameManager.Instance.InputManager.HeroPressedButton -= onHeroPressed;
                _fadeTimer.Time = 2.2f;
                _fadeTimer.Reset();
                _fadeintOut = true;
            } else if (e.PushButtonDown)
            {
                foreach (var hero in _heroes)
                {
                    hero.AcceptInput = true;
                    hero.CurrentControlScheme = ControlSchemeType.TopDown;
                    var vec3Dir = hero.transform.rotation * Vector3.forward;
                    var dir = new Vector2(vec3Dir.x, vec3Dir.z);
                    hero.TryMoveAi(dir);
                }
            }
        } else
        {
            if (e.TriggerButtonDown)
            {
                GameManager.Instance.InputManager.HeroPressedButton -= onHeroPressed;
                _fadeTimer.Time = 2.2f;
                _fadeTimer.Reset();
                _fadeintOut = true;
            }
        }

    }

    void Update()
    {
        if (_firstFrame)
        {
            _firstFrame = false;
            GameManager.Instance.Music.PlayNow(Music.OPENINGTHEME, 15.48f, 0.25f);
        }

        if (_congratsDelay)
        {
            if (_congratsTimer.Done)
            {
                _congratsDelay = false;
                _congratsTimer.Reset();
            }
        } else
        {
            _congrats.color = new Color(_congrats.color.r, _congrats.color.g, _congrats.color.b, _congratsTimer.Ratio);
        }

        if (_fadingIn)
        {
            _transitions.Value = _fadeTimer.Ratio;
        }

        if (_fadeintOut)
        {
            _transitions.Value = 1 - _fadeTimer.Ratio;
            if (_fadeTimer.Done)
            {
                GameManager.Instance.TransitToNextScene(GlobalStrings.SCENE_LOBBY);
            }
        }

    }

    private static int getIndexInPosition(int position, int[] results)
    {
        for (int i = 0; i < 4; i++)
        {
            if (results[i] == position)
                return i;
        }
        return -1;
    }
}
