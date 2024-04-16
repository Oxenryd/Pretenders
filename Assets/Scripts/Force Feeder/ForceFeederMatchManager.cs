using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private Unicorn[] unicorns;
    ZoomFollowGang zoomFollowScript;

    [SerializeField] private Transitions _transitions;
    private EasyTimer _transTimer;
    private bool _isFadeing = false;
    // Start is called before the first frame update
    void Start()
    {
        _transTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);

        // Subscribe to the event for each unicorn
        foreach (var unicorn in unicorns)
        {
            unicorn.OnScoreReached += HandleScoreReached;
        }
        zoomFollowScript = _cam.GetComponent<ZoomFollowGang>();
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
        GameManager.Instance.StartNewTournament();
        MatchResult newResult = new MatchResult(GameType.ForceFeeder, matchResult);

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

        _isFadeing = true;
        _transTimer.Reset();

        if (GameManager.Instance.Tournament)
        {
            GameManager.Instance.AddNewMatchResult(newResult);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (_isFadeing)
        {
            _transitions.Value = 1 - _transTimer.Ratio;
            if (_transTimer.Done)
            {
                GameManager.Instance.TransitToNextScene(GlobalStrings.SCENE_LOBBY);
            }
        }
    }
}
