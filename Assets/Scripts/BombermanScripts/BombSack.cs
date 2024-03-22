using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BombSack : MonoBehaviour
{

    [SerializeField]
    private GameObject bombPrefab;

    private int _currentBombIndex = 0;
    private int _maxCurrentBombs = 1;
    private Grid grid;


    [SerializeField]
    private Hero heroGameObject;
    private ICharacterMovement character;

    private Bomb[] bombs = new Bomb[GlobalValues.BOMBS_MAXBOMBS];
    // Start is called before the first frame update
    void Start()
    {
        GameObject gridObject = GameObject.FindWithTag(GlobalStrings.NAME_BOMBERGRID);
        grid = gridObject.GetComponent<Grid>();

        character = heroGameObject.gameObject.GetComponent<ICharacterMovement>();
        //Click L

        for (int i = 0; i < bombs.Length; i++)
        {
            var bombObj = Instantiate(bombPrefab).GetComponent<Bomb>();
            bombs[i] = bombObj;
            bombObj.SetInactive();
        }

        character.Triggered += TrySpawnBomb;

    }

    private void TrySpawnBomb(object sender, EventArgs e)
    {
        // How many active bombs???
        int activeBombs = 0;
        for (int i = 0; i < bombs.Length; i++)
        {
            if (bombs[i].IsActive)
                activeBombs++;
        }
        if (activeBombs < _maxCurrentBombs) //Yes, if lesser, we can spawn new bombs!!!
        {
            //Stuck on this work with later
            Vector3 middlePoint = GridCellMiddlePoint.Get(grid, character.GameObject.transform.position);

            bombs[_currentBombIndex].SpawnBomb(middlePoint);

            _currentBombIndex++;
            if (_currentBombIndex >= bombs.Length)
                _currentBombIndex = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
