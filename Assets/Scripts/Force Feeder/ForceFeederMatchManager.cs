using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    private const string SLASH = "/";

    [SerializeField] private WinnerTextScript _winnerText;
    [SerializeField] private GetReadyScript _getReady;
    [SerializeField] private Camera _cam;
    [SerializeField] private Unicorn[] unicorns;
    [SerializeField] private HeroMovement[] _heroes;
    [SerializeField] private FoodSpawner _foodSpawner;
    ZoomFollowGang zoomFollowScript;
    [SerializeField] private TextMeshProUGUI[] _scores;

    [SerializeField] private Transitions _transitions;
    private EasyTimer _transTimer;
    private bool _isFadingIn = true;
    private bool _isFadingOut = false;
    private bool _zoomingToWinner = false;
    // Start is called before the first frame update

    void Awake()
    {
        _transTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
        _transTimer.Reset();

        // Subscribe to the event HandleScoreReached for each unicorn.
        foreach (var unicorn in unicorns)
        {
            unicorn.OnScoreReached += HandleScoreReached;
            unicorn.OnTransfered += OnUnicornTransfered;
        }

        //Set the script to follow the Heroes
        zoomFollowScript = _cam.GetComponent<ZoomFollowGang>();
    }

    private void OnUnicornTransfered(object sender, EventArgs e)
    {
        var unicorn = (Unicorn)sender;
        _scores[unicorn.Index].text =
            GameManager.Instance.IntegerToString(Math.Min(unicorns[unicorn.Index].Score, GlobalValues.WINNING_POINTS_FORCE_FEEDER)) +
            SLASH + GameManager.Instance.IntegerToString(GlobalValues.WINNING_POINTS_FORCE_FEEDER);
    }

    void Start()
    {
        // Sets the direction of the heroes to point towards the middle of the arena. 
        foreach (var hero in _heroes)
        {
            var heroComp = hero.GetComponent<Hero>();
            switch (heroComp.Index)
            {
                case 0:
                    hero.TryMoveAi(new Vector2(-1, 0)); break;
                case 1:
                    hero.TryMoveAi(new Vector2(1, 0)); break;
                case 2:
                    hero.TryMoveAi(new Vector2(1, 0)); break;
                case 3:
                    hero.TryMoveAi(new Vector2(-1, 0)); break;
            }
        }
        _getReady.Activate();
        _getReady.CountdownComplete += onGetReadyCountedDown;

        GameManager.Instance.Music.Fadeout(1.5f);

        for (int i = 0; i < 4; i++)
        {
            _scores[i].text = GameManager.Instance.IntegerToString(unicorns[i].Score) + SLASH + GameManager.Instance.IntegerToString(GlobalValues.WINNING_POINTS_FORCE_FEEDER);
        }
    }

    /// <summary>
    /// This method handles the event when the "Get Ready" countdown finishes.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void onGetReadyCountedDown(object sender, EventArgs e)
    {
        // Starts the food spawning process.
        _foodSpawner.Running = true;
    }

    public void UpdateScore(int unicornIndex)
    {
        _scores[unicornIndex].text = GameManager.Instance.IntegerToString(unicorns[unicornIndex].Score) + SLASH + GameManager.Instance.IntegerToString(GlobalValues.WINNING_POINTS_FORCE_FEEDER);
    }

    /// <summary>
    /// This method handles the event when a score milestone is reached.
    /// </summary>
    private void HandleScoreReached()
    {

        // Create two integer arrays to store scores and match results for each unicorn.
        int[] scores = new int[unicorns.Length];
        int[] matchResult = new int[unicorns.Length];

        // Populate the scores array with the scores of each unicorn.
        foreach (var unicorn in unicorns) scores[unicorn.Index] = unicorn.Score;

        // Initialize variables to track the current match position and the index of the winner.
        int matchPosition = 0;
        int winnersIndex = -1;

        // Continue looping until all scores have been processed.
        while (scores.Any(item => item != -1))
        {
            int maxIndex = 0;
            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i] > scores[maxIndex])
                {
                    maxIndex = i;
                }
            }

            // Setting the index of the highest score
            if (matchPosition == 0) winnersIndex = maxIndex;

            //Setting the score to -1 indicating a processed score             
            scores[maxIndex] = -1;
            matchResult[maxIndex] = matchPosition;
            matchPosition++;
        }


        // Processing the result in case the game was a tournament
        if (GameManager.Instance.Tournament)
        {
            MatchResult newResult = new MatchResult(GameType.ForceFeeder, matchResult);
            GameManager.Instance.AddNewMatchResult(newResult);
        }
        
        //Setting the zoom script to follow the Hero
        Hero winner = zoomFollowScript.Targets[winnersIndex].GetComponent<Hero>();
        var hero = winner.GetComponent<HeroMovement>();
        hero.SetWinner(true);
        zoomFollowScript.SetWinner(winner.transform, false);
        _zoomingToWinner = true;

        // Activating the winnertext and transition timer 
        _transTimer.Time = _transTimer.Time * 3;
        _winnerText.Activate();
        _transTimer.Reset();

    }

    void Update()
    {
        if (_isFadingIn)
        {
            _transitions.Value = _transTimer.Ratio;
            if (_transTimer.Done)
            {
                _isFadingIn = false;
            }
        }

        if (_isFadingOut)
        {
            _transitions.Value = 1 - _transTimer.Ratio;
            if ( _transTimer.Done)
            {
                if (GameManager.Instance.Tournament)
                    GameManager.Instance.TransitToNextScene(GameManager.Instance.GetTournamentNextScene());
                else
                    GameManager.Instance.TransitToNextScene(GlobalStrings.SCENE_LOBBY);
            }
        }

        if (_zoomingToWinner && !_isFadingOut)
        {
            if (_transTimer.Done)
            {
                _isFadingOut = true;
                _transTimer.Time = _transTimer.Time / 3f;
                _transTimer.Reset();
            }
        }
    }
}
