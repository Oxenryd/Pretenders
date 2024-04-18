using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombermanManager : MonoBehaviour
{

    [SerializeField]
    private HeroMovement[] characterList = new HeroMovement[4];
    [SerializeField]
    private Vector3[] startCorners = new Vector3[] { new Vector3(2, 0, 2), new Vector3(42, 0, 2), new Vector3(42, 0, 38), new Vector3(2, 0, 38)};
    [SerializeField]
    private Grid _grid;
    public Queue deathQueue = new Queue();
    void Start()
    {
        RandomizeArray(startCorners);
        for (int i = 0; i < startCorners.Length; i++)
        {
            if (startCorners[i] == new Vector3(2, 0, 2) || startCorners[i] == new Vector3(42, 0, 2))
            {
                characterList[i].ForceRotation(Vector3.zero);
                characterList[i].transform.position = GridCellMiddlePoint.Get(_grid, startCorners[i]);
            }
            if (startCorners[i] == new Vector3(2, 0, 38) || startCorners[i] == new Vector3(42, 0, 38))
            {
                characterList[i].ForceRotation(new Vector3(0, -180, 0));
                characterList[i].transform.position = GridCellMiddlePoint.Get(_grid, startCorners[i]);
            }
        }
    }
    void Update()
    {
        
    }

    public void AddPlayerDeathToQueue(int playerId)
    {
        deathQueue.Enqueue(playerId);
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
