using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPathMaker : MonoBehaviour
{
    [SerializeField] private SetPathManager _manager;
    // Declare prefab Game Objects
    public GameObject pathStartPrefab;
    public GameObject pathEndPrefab;
    public GameObject pathPlatformPrefab;

    public int platformSize = 4;

    public bool winnerFound = false;

    private bool _winnerInformed = false;
    private int _winnerIndex = -1;
    private Transform _winnerTransform;
    private EasyTimer _gameTimer;

    // Method which adds a platform to the set path during its creation
    void AddPlatformToPath(int rowNr, int colNr, Vector3 pathTopLeftPos)
    {
        Vector3 platformPos = new Vector3(pathTopLeftPos.x + colNr*platformSize + platformSize / 2, 0, pathTopLeftPos.z - rowNr*platformSize - platformSize / 2);
        Instantiate(pathPlatformPrefab, platformPos, Quaternion.identity);
    }

    // Start is called before the first frame update
    void Start()
    {
        
        Vector3 pathMidPos = GameObject.Find("SetPathPrint").transform.position;
        // Since the size of the path is always the same, this is the value you get from taking the width and height from the gameobject in the scene
        // Did this for performance reasons instead of directly taking it each time running the program
        float setPathWidth = 22;
        float setPathHeight = 44;

        // The position on the top left of the path. Used mainly for calculations of where to place things relative to each other in the scene
        Vector3 pathTopLeftPos = new Vector3(pathMidPos.x - setPathWidth / 2, 0, pathMidPos.z + setPathHeight / 2);

        int pathRowCount = 10;
        int pathColumnCount = 5;
       // int platformSize = 8;

        int pastPathColumnNumber = 3;

        // Creates a random path by putting each platform either in front on the left, in front, or in front on the right of the last placed platform on the path
        for (int row = 0; row < pathRowCount; row++)
        {
            int curRowSetPlatform = Random.Range(pastPathColumnNumber - 1, pastPathColumnNumber + 1);

            if (curRowSetPlatform < 0)
            {
                curRowSetPlatform = 1;
            }
            else if (curRowSetPlatform > pathColumnCount)
            {
                curRowSetPlatform = pathColumnCount;
            }

            AddPlatformToPath(row, curRowSetPlatform, pathTopLeftPos);

            pastPathColumnNumber = curRowSetPlatform;
        }
    }

    public void InformWinner(int heroIndex, Transform winnerTransform)
    {
        _winnerIndex = heroIndex;
        _winnerInformed = true;
        _winnerTransform = winnerTransform;
    }

    // Method that checks if a position is colliding with the goal platform
    // Used to check if the players positions are collidiing with the goal
    bool IsCollidingWithGoal(Vector3 posToCheck)
    {
        Vector3 goalPos = pathEndPrefab.transform.position;

        if (posToCheck.x > goalPos.x)
        {
            if (posToCheck.z > 40) // Goalpaths Z value == 40 (FIX LATER HARDCODE)
            {
                return true;
            }
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        // Checks if the winner is found and then informs the program to continue to the next minigame if that is the case
        if (_winnerInformed)
        {
            _winnerInformed = false;
            GameManager.Instance.SetPlayerMultiplier(_winnerIndex, 1.5F);
            _manager.InformWinnerFound(_winnerTransform);
        }
    }
}
