using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrawlerMatchManager : MonoBehaviour
{
    [SerializeField] private Transitions _transitions;
    [SerializeField] public float _deathAltitude;
    [SerializeField] private List<GameObject> players = new List<GameObject>();
    [SerializeField] private GetReadyScript _getReady = new GetReadyScript();

    private bool _fadingIn = true;
    private bool _fadingOut = false;
    private EasyTimer _fadeTimer;

    //public GameObject roundWinner;
    //private string roundWinnerText = new string(string.Empty);
    public List<GameObject> placements = new List<GameObject>();
    int playersAlive = 4;

    void Awake()
    {
        _fadeTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
    }

    void Start()
    {
        //roundWinner.gameObject.SetActive(false);
        _getReady.Activate();
        _fadeTimer.Reset();
    }

    void Update()
    {
        if (_fadingIn)
        {
            _transitions.Value = _fadeTimer.Ratio;
            if (_fadeTimer.Done)
            {
                _fadingIn = false;
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].transform.position.y <= _deathAltitude
                && !placements.Contains(players[i]))
            {
                placements.Add(players[i]);
                playersAlive--;
            }
        }
        if (playersAlive == 1 && !_fadingOut)
        {
            for (int i = 0;i < players.Count; i++)
            {
                if (!placements.Contains(players[i]))
                {
                    placements.Add(players[i]);
                }
            }
            CalculateResults();
        }

        if (_fadingOut)
        {
            _transitions.Value = 1 - _fadeTimer.Ratio;
            if (_fadeTimer.Done)
            {
                if (GameManager.Instance.Tournament)
                    GameManager.Instance.TransitToNextScene(GameManager.Instance.GetTournamentNextScene());
                else
                    GameManager.Instance.TransitToNextScene(GlobalStrings.SCENE_LOBBY);
            }
        }
    }

    void CalculateResults()
    {
        if (GameManager.Instance.Tournament)
        {
            int[] playerPositionInMatch = new int[players.Count];

            int position = 1;
            for (int i = placements.Count - 1; i >= 0; i--)
            {
                int index = placements[i].GetComponent<Hero>().Index;
                playerPositionInMatch[index] = position - 1;
                position++;
            }
            GameManager.Instance.AddNewMatchResult(new MatchResult(GameType.Brawler, playerPositionInMatch));           
        }

        _fadingOut = true;
        _fadeTimer.Reset();
        //GameManager.Instance.StartNewTournament(); //remove later
        //MatchResult result = new MatchResult(GameType.Brawler, playerPositionInMatch);
        //roundWinner.gameObject.SetActive(true);
        //roundWinnerText = roundWinner.GetComponent<Text>().text;
        //roundWinnerText = $"Player {playerPositionInMatch[0]} Won!";
        //GameManager.Instance.AddNewMatchResult(result);
    }
}
