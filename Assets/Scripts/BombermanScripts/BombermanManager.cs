using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombermanManager : MonoBehaviour
{

    [SerializeField]
    private HeroMovement[] characterList = new HeroMovement[4];
    [SerializeField]
    private Vector3[] startCorners = new Vector3[] { new Vector3(2, 0, 2), new Vector3(42, 0, 2), new Vector3(42, 0, 38), new Vector3(2, 0, 38)};
    void Start()
    {
        RandomizeArray(startCorners);
        for(int i = 0; i < startCorners.Length; i++)
        {
            characterList[i].transform.position = startCorners[i];
        }
    }

    void Update()
    {
        
    }

    private void RandomizeArray<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

    private void PlayerDeath()
    {

    }

}
