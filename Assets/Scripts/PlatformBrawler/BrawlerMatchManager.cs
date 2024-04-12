using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrawlerMatchManager : MonoBehaviour
{
    [SerializeField] public float _deathAltitude;
    [SerializeField] private List<GameObject> players = new List<GameObject>();
    public List<GameObject> placements = new List<GameObject>();
    int playerIndex;
    int playersAlive = 4;

    void Start()
    {
        foreach (GameObject player in players)
        {
            player.GetComponent<Hero>().Index = playerIndex;
        }
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
        GameManager.Instance.StartNewTournament(); //remove later
        MatchResult result = new MatchResult(GameType.Brawler, playerPositionInMatch);
        GameManager.Instance.AddNewMatchResult(result);

        Debug.Log("Player 0: " + playerPositionInMatch[0]);
        Debug.Log("Player 1: " + playerPositionInMatch[1]);
        Debug.Log("Player 2: " + playerPositionInMatch[2]);
        Debug.Log("Player 3: " + playerPositionInMatch[3]);

    }
}
