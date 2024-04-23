using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetPathManager : MonoBehaviour
{
    public GameObject headlightPrefab;
    public GameObject[] spHeroes;

    private float groundLvl;
    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 lightOneStartPos = new Vector3 (0, 0, 0);
        Vector3 lightTwoStartPos = new Vector3 (0, 0, 0);
        Vector3 lightThreeStartPos = new Vector3 (0, 0, 0);

        // Spawn in one/multiple light prefabs

        // Initiate default values
        groundLvl = 0.1F;
        startPosition = new Vector3(20, 3, 3);

        // Add all heroes to the hero list
        spHeroes = GameObject.FindGameObjectsWithTag("Character");
    }

    // Update is called once per frame
    void Update()
    {
        // Randomize the lights next location
        // Move the lights to the next location

        // Check if any hero has fallen of the trail and tp them back if so
        foreach (GameObject heroObj in spHeroes)
        {
            if (heroObj.transform.position.y < groundLvl)
            {
                heroObj.transform.position = startPosition;
            }
        }
    }
}
