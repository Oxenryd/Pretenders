using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LobbySceneScript : MonoBehaviour
{
    [SerializeField] Transitions _transition;
    [SerializeField] GetReadyScript _readyScript;

    public bool IncludeMiniMiniGames = false;

    private EasyTimer _fadeTimer;
    private bool _fadedIn = false;

    // Start is called before the first frame update
    void Start()
    {
        var resultScreenTest = GameObject.FindWithTag(GlobalStrings.NAME_RESULTSCREEN_DEBUG).GetComponent<TransitionZoneScript>();
        resultScreenTest.TriggeredTransition += resultScreenLoadTest;

        _readyScript.CountdownComplete += (sender, args) =>
        {
            Debug.Log("Done counting in!");
        };

        _fadeTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
        if (GameManager.Instance.FromSceneLoaded)
            GameManager.Instance.Transitions.Value = 0;
        else
            _transition.Value = 1;
        _fadeTimer.Reset();
        GameManager.Instance.Music.Fadeout(3f);

        //DEBUG DEBUG DEBUG
        //GameManager.Instance.InputManager.Heroes[0].PressedPushButton += testTournamentRando;

       _readyScript.Activate();
    }

    private void resultScreenLoadTest(object sender, EventArgs e)
    {
        GameManager.Instance.StartNewTournament();

        GameManager.Instance.DebuggingResultScreen = true;

        GameManager.Instance.AddNewMatchResult(new MatchResult(GameType.Lobby, MatchResult.GenerateRandomStandings()));
        GameManager.Instance.AddNewMatchResult(new MatchResult(GameType.Lobby, MatchResult.GenerateRandomStandings()));

    }

    /// <summary>
    /// DEBUG DEBUG DEBUG
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void testTournamentRando(object sender, EventArgs e)
    {
        GameManager.Instance.StartNewTournament();
        var sb = new StringBuilder();
        foreach (var game in GameManager.Instance.TournamentGameList)
        {
            sb.Append($" - {GameManager.Instance.GetTournamentNextScene()} - ");
        }
        Debug.Log( sb.ToString() );

        System.Random rand = new System.Random();

        var positionTaken = new bool[] { false, false, false, false };
        List<int> positions = new();

        for (int i = 0; i < 4; i++)
        {
            while (true)
            {
                var pos = rand.Next(0, 4);
                if (!positionTaken[pos])
                {
                    positionTaken[pos] = true;
                    positions.Add(pos);
                    break;
                }
            }
        }
        var matchResult = new MatchResult(GameType.Lobby, positions.ToArray());

    }

    void Update()
    {
        if (!_fadedIn)
        {
            if (GameManager.Instance.FromSceneLoaded)
                GameManager.Instance.Transitions.Value = _fadeTimer.Ratio;
        }
        if (_fadeTimer.Done)
            _fadedIn = true;  

    }
}