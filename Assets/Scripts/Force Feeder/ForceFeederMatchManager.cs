using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : MonoBehaviour
{

    [SerializeField] private Unicorn[] unicorns;

    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to the event for each unicorn
        foreach (var unicorn in unicorns)
        {
            unicorn.OnScoreReached += HandleScoreReached;
        }
    }

    private int[] HandleScoreReached()
    {
        int[] results = new int[unicorns.Length];

        foreach(var unicorn in unicorns)
        {
            results[unicorn.Index] = unicorn.Score;
        }
        return results;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
