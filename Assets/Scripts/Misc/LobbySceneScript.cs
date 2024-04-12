using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneScript : MonoBehaviour
{
    [SerializeField] Transitions _transition;

    private EasyTimer _fadeTimer;
    private bool _fadedIn = false;

    // Start is called before the first frame update
    void Start()
    {
        _fadeTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
        if (GameManager.Instance.FromSceneLoaded)
            GameManager.Instance.Transitions.Value = 0;
        else
            _transition.Value = 1;
        _fadeTimer.Reset();
        GameManager.Instance.Music.Fadeout(3f);

        //DEBUG DEBUG DEBUG
        GameManager.Instance.InputManager.Heroes[0].PressedPushButton += testTournamentRando;
    }

    private void testTournamentRando(object sender, EventArgs e)
    {
        GameManager.Instance.StartNewTournament();
        var sb = new StringBuilder();
        foreach (var game in GameManager.Instance.TournamentGameList)
        {
            sb.Append($" - {GameManager.Instance.GetTournamentNextScene()} - ");
        }
        Debug.Log( sb.ToString() );

        //System.Random rand = new System.Random();
        //var playerScores = new float[]
        //{
        //    GlobalValues.
        //};
        //var matchResult = new 

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