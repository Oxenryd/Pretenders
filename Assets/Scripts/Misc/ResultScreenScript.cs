using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class ResultScreenScript : MonoBehaviour
{
   
    private const string PLUS = "+ ";
    [SerializeField] private bool _debugCreateBogusStats;
    [SerializeField] private RectTransform[] _transforms;
    [SerializeField] private Transitions _transitions;
    [SerializeField] private Vector3[] _targetPositions;
    [SerializeField] private int[] _pointsBeforeLast;
    [SerializeField] private float[] _intermediatePoints;
    [SerializeField] private float[] _pointsLastMatch;
    [SerializeField] private TextMeshProUGUI[] _pointTexts;
    [SerializeField] private TextMeshProUGUI[] _plusTexts;
    [SerializeField] private TextMeshProUGUI[] _positionTexts;
    private EasyTimer _fadeTimer;
    private EasyTimer _delayTimer;
    private EasyTimer _moveTimer;
    private EasyTimer _countingTimer;
    private EasyTimer _showPosTimer;
    private bool _fadeingIn = true;
    private bool _leaving = false;
    private bool _counting = false;
    private bool _doneCounting = false;
    private bool _movingPending = false;
    private bool _moving = false;
    private bool _showingPos = false;
    private int[] _targetStandings;
    private Vector3[] _startPositions;
    private int _bgColorIndex = -1;
    private int _leaderIndex = -1;
    private Color _previousColor;
    [SerializeField] private Material _bgMaterial;
    [SerializeField] private Color[] _bgColors;

    // Start is called before the first frame update
    void Start()
    {
        _bgColorIndex = Shader.PropertyToID("_color");

        if (_debugCreateBogusStats)
        {
            setDebugStats();
        }
        _showPosTimer = new EasyTimer(1.6f);
        _startPositions = new Vector3[4];
        _intermediatePoints = new float[] { 0f, 0f, 0f, 0f };
        _countingTimer = new EasyTimer(1f);
        _moveTimer = new EasyTimer(1.2f);
        _delayTimer = new EasyTimer(1.2f);
        _delayTimer.Reset();
        _pointsBeforeLast = new int[] { 0, 0, 0, 0 };
        for (int i = 0; i < 4; i++)
        {
            var points = 0f;
            var results = GameManager.Instance.GetMatchResults();
            if (results.Length > 1)
            {
                
                for (int j = 0; j < results.Length - 1; j++)
                {
                    points += results[j].Scores[i];
                }
                _pointsBeforeLast[i] = Mathf.RoundToInt(points);
            }
        }

        _pointsLastMatch = GameManager.Instance.GetMatchResults()[^1].Scores;

        for (int i = 0; i < 4; i++)
        {
            _pointTexts[i].text = GameManager.Instance.IntegerToString(_pointsBeforeLast[i]);
            _plusTexts[i].text = PLUS + GameManager.Instance.IntegerToString(Mathf.RoundToInt(_pointsLastMatch[i]));
        }

        _transitions.TransitionType = TransitionType.FadeToBlack;
        _transitions.Value = 0;
        _fadeTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME * 2);
        _fadeTimer.Reset();
        GameManager.Instance.InputManager.HeroPressedButton += transit;

        _targetStandings = getPositions(true, out int previousLeader);
        for (int i = 0; i < 4; i++)
        {
            var thisPlayerPosition = _targetStandings[i];
            _transforms[i].localPosition = _targetPositions[thisPlayerPosition];
            _startPositions[i] = _transforms[i].localPosition;
        }

        _previousColor = _bgColors[previousLeader];
        _bgMaterial.SetColor(_bgColorIndex, _previousColor);

        _targetStandings = getPositions(false, out _leaderIndex);

        for (int i = 0; i < 4; i++)
        {
            _positionTexts[i].color = new Color(_positionTexts[i].color.r, _positionTexts[i].color.g, _positionTexts[i].color.b, 0);
        }
        
    }

    private void transit(object sender, HeroMovement e)
    {
        _fadeTimer.Reset();
        _leaving = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_fadeingIn)
        {
            _transitions.Value = _fadeTimer.Ratio;
            if (_fadeTimer.Done)
                _fadeingIn = false;
        }


        if (_delayTimer.Done && !_doneCounting && !_counting )
        {
            _counting = true;
            _countingTimer.Reset();
        }
            
        if (_counting)
        {
            for (int i = 0; i < 4; i++)
            {
                _intermediatePoints[i] = _pointsBeforeLast[i] + _pointsLastMatch[i] * _countingTimer.Ratio;
                _pointTexts[i].text = GameManager.Instance.IntegerToString(Mathf.RoundToInt(_intermediatePoints[i]));
            }
            if (_countingTimer.Done)
            {
                _counting = false;
                _doneCounting = true;
                _movingPending = true;
                _delayTimer.Reset();
            }
        }

        if (_movingPending)
        {
            if (_delayTimer.Done)
            {
                _moving = true;
                _movingPending = false;
                _moveTimer.Reset();

            }    
        }

        if (_moving)
        {
            for (int i = 0; i < 4; i++)
            {
                _transforms[i].localPosition = Vector3.Lerp(_startPositions[i], _targetPositions[ _targetStandings[i] ], _moveTimer.Ratio);
                _plusTexts[i].color = new Color(_plusTexts[i].color.r, _plusTexts[i].color.g, _plusTexts[i].color.b, 1 - _moveTimer.Ratio);
            }
            if (_moveTimer.Done)
            {
                _showingPos = true;
                _moving = false;
                _showPosTimer.Reset();
            }
        }

        if (_showingPos)
        {
            for (int i = 0; i < 4; i++)
            {
                _positionTexts[i].color = new Color(_positionTexts[i].color.r, _positionTexts[i].color.g, _positionTexts[i].color.b, _showPosTimer.Ratio);
                _bgMaterial.SetColor(_bgColorIndex, Color.Lerp(_previousColor, _bgColors[_leaderIndex], _showPosTimer.Ratio));
            }
        }

        if (_leaving)
        {
            _transitions.Value = 1 - _fadeTimer.Ratio;
            if ( _fadeTimer.Done)
            {
                if (GameManager.Instance.DebuggingResultScreen)
                    GameManager.Instance.TransitToNextScene(GlobalStrings.SCENE_LOBBY);
                else
                    GameManager.Instance.TransitToNextScene(GameManager.Instance.NextScene);
            }
        }
    }

    private int[] getPositions(bool resultBeforeLast, out int leadingPosition)
    {
        bool leaderSet = false;
        leadingPosition = -1;
        var results = GameManager.Instance.GetMatchResults();
        float[] scores = new float[4];
        int border = resultBeforeLast ? results.Length - 1 : results.Length;
        for (int i = 0; i < 4; i++)
        {
            float total = 0f;

            for (int j = 0; j < border; j++)
            {
                total += results[j].Scores[i];
            }
            scores[i] = total;
        }

        var playerPositions = new int[] { -1, -1, -1, -1}; 
        int currentPos = 0;
        bool[] positioned = new bool[] { false, false, false, false };
        while (true)
        {
            int currentHighestPlayerIndex = -1;
            float highest = float.NegativeInfinity;
            for (int i = 0; i < 4; i++)
            {
                if (positioned[i]) continue;

                if (scores[i] > highest)
                {
                    highest = scores[i];
                    currentHighestPlayerIndex = i;
                }               
            }

            if (!leaderSet)
            {
                leadingPosition = currentHighestPlayerIndex;
                leaderSet = true;
            }

            positioned[currentHighestPlayerIndex] = true;
            playerPositions[currentHighestPlayerIndex] = currentPos;
            currentPos++;
            if (currentPos > 3) break;
        }
        return playerPositions;
    }

    private void setDebugStats()
    {
        GameManager.Instance.StartNewTournament();

        int[] randomStandings()
        {
            int[] standings = new int[] { -1, -1, -1, -1 };
            bool[] taken = new bool[] { false, false, false, false };
            System.Random random = new System.Random();
            for (int i = 0; i < 4; i++)
            {
                while (true)
                {
                    var pos = random.Next(0, 4);
                    if (!taken[pos])
                    {
                        taken[pos] = true;
                        standings[i] = pos;
                        break;
                    }
                }
            }
            return standings;
        }


        GameManager.Instance.DebuggingResultScreen = true;
        GameManager.Instance.AddNewMatchResult(new MatchResult(GameType.Lobby, randomStandings()));
        GameManager.Instance.AddNewMatchResult(new MatchResult(GameType.Lobby, randomStandings()));
    }
}
