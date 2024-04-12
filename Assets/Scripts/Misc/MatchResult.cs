﻿using System.Collections.Generic;

public class MatchResult
{
    public MatchResult(GameType gameType, int[] playerPositionInMatch)
    {
        GameType = gameType;
        float[] playerScores = new float[4];
        for (int i = 0; i < playerScores.Length; i++)
        {
            playerScores[i] = GlobalValues.TOURNAMENT_SCORES[playerPositionInMatch[i]] * GameManager.Instance.GetPlayerMultiplier(i);
        }
        Scores = playerScores;
    }

    public GameType GameType { get; private set; }
    public float[] Scores { get; private set; }

    /// <summary>
    /// Returns the standing for each player index: => GetStandings[0] == 3 means player index 0 is in last place.
    /// </summary>
    /// <param name="currentResults"></param>
    /// <returns></returns>
    public static int[] GetPlayersStandings(MatchResult[] currentResults)
    {
        Dictionary<int, float> playerScores = new Dictionary<int, float>();
        foreach (MatchResult result in currentResults)
        {
            for (int i = 0; i < result.Scores.Length; i++)
            {
                if (!playerScores.ContainsKey(i))
                    playerScores.Add(i, result.Scores[i]);
                else
                    playerScores[i] = playerScores[i] + result.Scores[i];
            }
        }

        int[] actualPositions = new int[4];

        var positionsToDistribute = 0;
        var counted = new bool[] { false, false, false, false };
        var highestScore = float.NegativeInfinity;
        var highestId = -1;
        while (true)
        {
            for (int i = 0; i < 4; i++)
            {
                if (!counted[i])
                {
                    if (playerScores[i] > highestScore)
                    {
                        highestScore = playerScores[i];
                        highestId = i;
                    }
                }
            }

            actualPositions[highestId] = positionsToDistribute;
            positionsToDistribute++;
            highestScore = float.NegativeInfinity;
            counted[highestId] = true;

            if (positionsToDistribute > 3)
                break;
        }

        return actualPositions;
    }
}