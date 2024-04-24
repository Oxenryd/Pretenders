using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BrawlerMatchManager : MonoBehaviour
{
    [SerializeField] public float _deathAltitude;
    [SerializeField] private List<GameObject> players = new List<GameObject>();
    [SerializeField] private GetReadyScript _getReady = new GetReadyScript();
    //public GameObject roundWinner;
    //private string roundWinnerText = new string(string.Empty);
    public List<GameObject> placements = new List<GameObject>();
    int playersAlive = 4;

    void Start()
    {
        //roundWinner.gameObject.SetActive(false);
        _getReady.Activate();
    }

    void Update()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].transform.position.y <= _deathAltitude
                && !placements.Contains(players[i]))
            {
                placements.Add(players[i]);
                playersAlive--;
            }
        }
        if (playersAlive == 1)
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
    }

    void CalculateResults()
    {
        int[] playerPositionInMatch = new int[players.Count];

        int position = 1;
        for (int i = placements.Count - 1; i >= 0; i--)
        {
            int index = placements[i].GetComponent<Hero>().Index;
            playerPositionInMatch[index] = position;
            position++;
        }
        GameManager.Instance.TransitToNextScene(GlobalStrings.SCENE_LOBBY);
        //GameManager.Instance.StartNewTournament(); //remove later
        //MatchResult result = new MatchResult(GameType.Brawler, playerPositionInMatch);
        //roundWinner.gameObject.SetActive(true);
        //roundWinnerText = roundWinner.GetComponent<Text>().text;
        //roundWinnerText = $"Player {playerPositionInMatch[0]} Won!";
        //GameManager.Instance.AddNewMatchResult(result);
    }
}
