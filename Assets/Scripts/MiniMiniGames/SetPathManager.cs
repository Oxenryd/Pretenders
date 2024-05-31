using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetPathManager : MonoBehaviour
{
    public const string CLOCKTEXT = "Time Left: ";

    [SerializeField] private ZoomFollowGang _cam;
    [SerializeField] private GetReadyScript _getReady;
    [SerializeField] private WinnerTextScript _winnerText;
    [SerializeField] private Transitions _transitions;
    [SerializeField] private HeroMovement[] _heroes;
    [SerializeField] private TextMeshProUGUI _clockText;

    // Declare Game Object Prefabs
    public GameObject headlightPrefab;
    public GameObject[] spHeroes;

    [SerializeField] private float[] _heroesMax = new float[] { 0f,0f,0f,0f};
    // Declare variables
    private float groundLvl;
    private Vector3 startPosition;
    private bool _isFadingIn = true;
    private bool _isFadingOut = false;
    private bool _zoomingToWinner = false;
    private EasyTimer _fadeTimer;
    private EasyTimer _gameTimer;

    private bool _timeIsUp = false;


    public void InformWinnerFound(Transform winnerTransform)
    {
        var hero = winnerTransform.GetComponent<HeroMovement>();
        hero.SetWinner(false);
        _cam.SetWinner(winnerTransform, false);
        _winnerText.Activate();
        _zoomingToWinner = true;
        _fadeTimer.Time = _fadeTimer.Time * 3;
        _fadeTimer.Reset();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var hero in _heroes)
        {
            hero.TryMoveAi(new Vector2(0, 1));
        }

        // Create a fade in transition to the minigame
        _getReady.CountdownComplete += onCountdownComplete;
        _transitions.TransitionType = TransitionType.CircleFade;
        _fadeTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
        _fadeTimer.Reset();
        _getReady.Activate();
        
        // Reset the score multipliers from the previous mini minigame
        GameManager.Instance.ResetPlayerMultipliers();
        GameManager.Instance.Music.Fadeout(1.5f);

        Vector3 lightOneStartPos = new Vector3 (0, 0, 0);
        Vector3 lightTwoStartPos = new Vector3 (0, 0, 0);
        Vector3 lightThreeStartPos = new Vector3 (0, 0, 0);

        // Spawn in one/multiple light prefabs

        // Initiate default values
        groundLvl = -2F;
        startPosition = new Vector3(20, 3, 3);

        // Add all heroes to the hero list
        spHeroes = GameObject.FindGameObjectsWithTag("Character");
        _gameTimer = new EasyTimer(GlobalValues.SETPATH_GAME_TIME);
    }

    private void onCountdownComplete(object sender, EventArgs e)
    {
        _clockText.enabled = true;
        _gameTimer.Reset();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the time is up without anyone having reached the goal
        // If so, then make the winner the player that made it the closest to the goal
        if (_gameTimer.Done && !_timeIsUp)
        {
            _timeIsUp = true;
            var bestIndex = -1;
            var bestResult = float.NegativeInfinity;
            for (int i = 0; i < 4;  i++)
            {
                if (_heroesMax[i] > bestResult)
                {
                    bestIndex = i;
                    bestResult = _heroesMax[i];
                }
            }
            InformWinnerFound(_heroes[bestIndex].transform);
        }


        // Randomize the lights next location
        // Move the lights to the next location

        // Check if any hero has fallen of the trail and tp them back if so
        foreach (GameObject heroObj in spHeroes)
        {
            if (heroObj.transform.position.y < groundLvl)
            {
                heroObj.transform.position = startPosition;
            }
        }

        if (_clockText.enabled)
        {
            _clockText.text = CLOCKTEXT + GameManager.Instance.IntegerToString((int)(_gameTimer.Time - (_gameTimer.Time * _gameTimer.Ratio)));
        }

        // Check maximum depth achieved
        for (int i = 0; i < 4; i++)
        {
            _heroesMax[i] = Mathf.Max(_heroesMax[i], _heroes[i].transform.position.z);
        }

        if (_isFadingIn)
        {
            _transitions.Value = _fadeTimer.Ratio;
            if (_fadeTimer.Done)
            {
                _isFadingIn = false;
            }
        }

        // If the minigame is over and needs to start fading out into the next scene
        if (_isFadingOut)
        {
            _transitions.Value = 1 - _fadeTimer.Ratio;
            if ( _fadeTimer.Done)
            {
                if (GameManager.Instance.Tournament)
                    GameManager.Instance.TransitToNextScene(GameManager.Instance.GetTournamentNextScene());
                else
                    GameManager.Instance.TransitToNextScene(GlobalStrings.SCENE_LOBBY);
            }
        }

        // Check if the minigame is in the last stage where the winner is shown more across the screen
        if (_zoomingToWinner && !_isFadingOut)
        {
            _clockText.color = new Color(_clockText.color.r, _clockText.color.g, _clockText.color.b, 1 - _fadeTimer.Ratio);
            if (_fadeTimer.Done)
            {
                _clockText.enabled = false;
                _isFadingOut = true;
                _fadeTimer.Time = _fadeTimer.Time / 3f;
                _fadeTimer.Reset();
            }
        }
    }
}
