using System;
using System.Linq;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private WinnerTextScript _winnerText;
    [SerializeField] private GetReadyScript _getReady;
    [SerializeField] private Camera _cam;
    [SerializeField] private Unicorn[] unicorns;
    [SerializeField] private FoodSpawner _foodSpawner;
    ZoomFollowGang zoomFollowScript;

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
        // Subscribe to the event for each unicorn
        foreach (var unicorn in unicorns)
        {
            unicorn.OnScoreReached += HandleScoreReached;
        }
        zoomFollowScript = _cam.GetComponent<ZoomFollowGang>();
    }

    void Start()
    {
        _getReady.Activate();
        _getReady.CountdownComplete += onGetReadyCountedDown;

        GameManager.Instance.Music.Fadeout(1.5f);
    }

    private void onGetReadyCountedDown(object sender, EventArgs e)
    {
        _foodSpawner.Running = true;
    }

    private void HandleScoreReached()
    {
        int[] scores = new int[unicorns.Length];
        int[] matchResult = new int[unicorns.Length];

        foreach (var unicorn in unicorns) scores[unicorn.Index] = unicorn.Score;

        int matchPosition = 0;
        int winnersIndex = -1;
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

            // Setting the winners index
            if (matchPosition == 0) winnersIndex = maxIndex;


            scores[maxIndex] = -1;
            matchResult[maxIndex] = matchPosition;
            matchPosition++;
        }


        //Debug
        if (GameManager.Instance.Tournament)
        {
            MatchResult newResult = new MatchResult(GameType.ForceFeeder, matchResult);
            GameManager.Instance.AddNewMatchResult(newResult);
        }
        

        Transform winner = zoomFollowScript.Targets[winnersIndex];
        //zoomFollowScript.Targets = new Transform[] { winner };

        zoomFollowScript.SetWinner(winner, false);
        _zoomingToWinner = true;
        _transTimer.Time = _transTimer.Time * 3;
        _winnerText.Activate();
        _transTimer.Reset();

    }

    // Update is called once per frame
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
