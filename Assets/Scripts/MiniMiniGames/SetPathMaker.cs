using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPathMaker : MonoBehaviour
{
    public GameObject pathStartPrefab;
    public GameObject pathEndPrefab;
    public GameObject pathPlatformPrefab;

    public int platformSize = 4;

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

    // Update is called once per frame
    void Update()
    {
        
    }
}
