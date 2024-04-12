using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class BombSack : MonoBehaviour
{
    [SerializeField] private bool _enabled = false;
    [SerializeField]
    private GameObject bombPrefab;

    [SerializeField]
    private LayerMask bombLayerMask;

    private int _currentBombIndex = 0;
    private int _maxCurrentBombs = 1;
    private Grid grid;


    [SerializeField]
    private Hero heroGameObject;
    private HeroMovement character;

    private Bomb[] bombs = new Bomb[GlobalValues.BOMBS_MAXBOMBS];
    private Dictionary<Vector3, Vector3> directions = new Dictionary<Vector3, Vector3>() { { Vector3.back, new Vector3(0,0,-2) }, { Vector3.forward, new Vector3(0,0,2) }, { Vector3.left, new Vector3(-2,0,0) }, { Vector3.right, new Vector3(2,0,0) } };

    // Start is called before the first frame update
    void Start()
    {
        if (!_enabled) return;
        GameObject gridObject = GameObject.FindWithTag(GlobalStrings.NAME_BOMBERGRID);
        grid = gridObject.GetComponent<Grid>();

        character = heroGameObject.gameObject.GetComponent<HeroMovement>();
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
            RaycastHit hit;
            Physics.Raycast(middlePoint + new Vector3(0, .5f, 0), character.FaceDirection, out hit, grid.cellSize.x, bombLayerMask);
            if (!hit.collider)
            {
                for (int i = 0; i < directions.Count; i++)
                {
                    var directionKey = directions.Keys.ElementAt(i);

                    if (directionKey == character.FaceDirection)
                    {
                        bombs[_currentBombIndex].SpawnBomb(middlePoint + directions[directionKey]);
                        _currentBombIndex++;
                        if (_currentBombIndex >= bombs.Length)
                            _currentBombIndex = 0;
                    }
                }
            }
        }
    }
}
