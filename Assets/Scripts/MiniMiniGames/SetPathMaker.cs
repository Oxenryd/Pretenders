using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPathMaker : MonoBehaviour
{
    [SerializeField] private SetPathManager _manager;
    public GameObject pathStartPrefab;
    public GameObject pathEndPrefab;
    public GameObject pathPlatformPrefab;

    public int platformSize = 4;

    public bool winnerFound = false;

    void AddPlatformToPath(int rowNr, int colNr, Vector3 pathTopLeftPos)
    {
        Vector3 platformPos = new Vector3(pathTopLeftPos.x + colNr*platformSize + platformSize / 2, 0, pathTopLeftPos.z - rowNr*platformSize - platformSize / 2);
        Instantiate(pathPlatformPrefab, platformPos, Quaternion.identity);
    }

    // Start is called before the first frame update
    void Start()
    {
        Vector3 pathMidPos = GameObject.Find("SetPathPrint").transform.position;
        //float setPathWidth = GameObject.Find("SetPathPrint").bounds.size.x;
        //float setPathHeight = GameObject.Find("SetPathPrint").bounds.size.z;
        float setPathWidth = 22;
        float setPathHeight = 44;

        Vector3 pathTopLeftPos = new Vector3(pathMidPos.x - setPathWidth / 2, 0, pathMidPos.z + setPathHeight / 2);

        int pathRowCount = 10;
        int pathColumnCount = 5;
        int platformSize = 8;

        int pastPathColumnNumber = 3;

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

        if (winnerFound == false)
        {
            GameObject[] heroObjects = GameObject.FindGameObjectsWithTag("Character");
            List<Hero> heroes = new();
            foreach (var obj in heroObjects)
            {
                var hero = obj.GetComponent<Hero>();
                heroes.Add(hero);
            }

            for (global::System.Int32 heroIndex = 0; heroIndex < 4; heroIndex++)
            {
                if (IsCollidingWithGoal(heroes[heroIndex].transform.position))
                 {
                    Debug.Log("Winner! Player: " + heroIndex);

                    // Update the _scoreMultiplier of the hero's index in the GameManager
                    GameManager.Instance.SetPlayerMultiplier(heroes[heroIndex].Index, 1.5F);

                    Debug.Log("Player 0: " + GameManager.Instance.GetPlayerMultiplier(0));
                    Debug.Log("Player 1: " + GameManager.Instance.GetPlayerMultiplier(1));
                    Debug.Log("Player 2: " + GameManager.Instance.GetPlayerMultiplier(2));
                    Debug.Log("Player 3: " + GameManager.Instance.GetPlayerMultiplier(3));

                    winnerFound = true;
                    _manager.InformWinnerFound();
                }
            }
        }
    }
}
