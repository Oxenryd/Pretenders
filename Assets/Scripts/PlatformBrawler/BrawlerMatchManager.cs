using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BrawlerMatchManager : MonoBehaviour
{
    [SerializeField] private ZoomFollowGang _cam;
    [SerializeField] private WinnerTextScript _winnerText;
    [SerializeField] private Transitions _transitions;
    [SerializeField] public float _deathAltitude;
    [SerializeField] private List<GameObject> players = new List<GameObject>();
    [SerializeField] private GetReadyScript _getReady; // = new GetReadyScript();

    private bool _fadingIn = true;
    private bool _fadingOut = false;
    private bool _zoomingToWinner = false;
    private EasyTimer _fadeTimer;

    //public GameObject roundWinner;
    //private string roundWinnerText = new string(string.Empty);
    public List<GameObject> placements = new List<GameObject>();
    int playersAlive = 4;

    void Start()
    {
        _fadeTimer = new EasyTimer(GlobalValues.SCENE_CIRCLETRANSIT_TIME);
        _fadingIn = true;
        //roundWinner.gameObject.SetActive(false);
        List<Hero> heroes = new List<Hero>();
        foreach (var player in players)
        {
            heroes.Add(player.GetComponent<Hero>());
        }
        foreach (var hero in heroes)
        {
            switch (hero.Index)
            {
                case 0:
                case 2:
                    hero.Movement.TryMoveAi(new Vector2(1, 0)); break;

                case 1:
                case 3:
                    hero.Movement.TryMoveAi(new Vector2(-1, 0)); break;

            }
        }

        _getReady.Activate();
        _fadeTimer.Reset();
        if (GameManager.Instance.Music != null)
            GameManager.Instance.Music.Fadeout(1.5f);
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
        if (_zoomingToWinner && !_fadingOut)
        {
            if (_fadeTimer.Done)
            {
                _fadingOut = true;
                _fadeTimer.Time = _fadeTimer.Time / 3f;
                _fadeTimer.Reset();
            }
        }

    }

    void CalculateResults()
    {
        if (_zoomingToWinner || _fadingOut) return;

       int[] playerPositionInMatch = new int[players.Count];

       int position = 1;
        for (int i = placements.Count - 1; i >= 0; i--)
        {
            int index = placements[i].GetComponent<Hero>().Index;
            playerPositionInMatch[index] = position - 1;
            position++;
        }

        if (GameManager.Instance.Tournament)
        {
            GameManager.Instance.AddNewMatchResult(new MatchResult(GameType.Brawler, playerPositionInMatch));           
        }

        _zoomingToWinner = true;
        _fadeTimer.Time = _fadeTimer.Time * 3;
        _fadeTimer.Reset();
        _winnerText.Activate();
        var winnerIndex = -1;
        for (int i = 0; i < 4; i++)
        {
            if (playerPositionInMatch[i] == 0)
            {
                winnerIndex = i;
                break;
            }
        }
        var heroes = new List<Hero>();
        foreach (var obj in players)
        {
            heroes.Add(obj.GetComponent<Hero>());
        }
        var winnerTransform = heroes.Where(hero => hero.Index == winnerIndex).SingleOrDefault().transform;
        var movement = winnerTransform.GetComponent<HeroMovement>();
        movement.SetWinner(true);
        _cam.SetWinner(winnerTransform, true);
        //GameManager.Instance.StartNewTournament(); //remove later
        //MatchResult result = new MatchResult(GameType.Brawler, playerPositionInMatch);
        //roundWinner.gameObject.SetActive(true);
        //roundWinnerText = roundWinner.GetComponent<Text>().text;
        //roundWinnerText = $"Player {playerPositionInMatch[0]} Won!";
        //GameManager.Instance.AddNewMatchResult(result);
    }
}
