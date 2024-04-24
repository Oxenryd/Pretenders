using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private GetReadyScript _getReady;
    [SerializeField] private Camera _cam;
    [SerializeField] private Unicorn[] unicorns;
    [SerializeField] private FoodSpawner _foodSpawner;
    ZoomFollowGang zoomFollowScript;

    [SerializeField] private Transitions _transitions;
    private EasyTimer _transTimer;
    private bool _isFadingIn = true;
    private bool _isFadingOut = false;
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
    }

    private void onGetReadyCountedDown(object sender, EventArgs e)
    {
        _foodSpawner.Running = true;
    }

    private void HandleScoreReached()
    {
        int[] scores = new int[unicorns.Length];

        foreach(var unicorn in unicorns) scores[unicorn.Index] = unicorn.Score;

        int[] scoresSorted = new int[scores.Length];
        Array.Copy(scores, scoresSorted, scores.Length);
        Array.Sort(scoresSorted, (x, y) => y.CompareTo(x));

        int[] matchResult = new int[scores.Length];

        for (int i = 0; i < scores.Length; i++)
        {
            for (int j = 0; j < scoresSorted.Length; j++)
            {
                if (scores[i] == scoresSorted[j])
                {
                    matchResult[i] = j;
                    break;
                }
            }  
        }
        //Debug
       // GameManager.Instance.StartNewTournament();
       if (GameManager.Instance.Tournament)
       {
            MatchResult newResult = new MatchResult(GameType.ForceFeeder, matchResult);
            GameManager.Instance.AddNewMatchResult(newResult);
        }
        

        int index = -1; // Indexet för det största talet (som du vet redan är 3)

        for (int i = 0; i < 4; i++)
        {
            if (matchResult[i] == 0)
            {
                index = i;
                break; // Du behöver inte fortsätta söka efter fler 3:or om du har hittat en
            }
        }
        Transform winner = zoomFollowScript.Targets[index];
        zoomFollowScript.Targets = new Transform[] { winner };

        _isFadingOut = true;
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
    }
}
