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

    private int _currentBombIndexBuffer = 0;
    private int _currentBombIndex = 0;
    private int _maxCurrentBombs = 1;
    private Grid grid;
    private GridOccupation gridOccupation;


    [SerializeField]
    private Hero heroGameObject;
    private HeroMovement character;

    private Bomb[] bombs = new Bomb[GlobalValues.BOMBS_MAXBOMBS];
    private Dictionary<Vector3, Vector3> directions = new Dictionary<Vector3, Vector3>() { { Vector3.back, new Vector3(0,0,-2) }, { Vector3.forward, new Vector3(0,0,2) }, { Vector3.left, new Vector3(-2,0,0) }, { Vector3.right, new Vector3(2,0,0) } };
    private int _explosionLength = 5;

    /// <summary>
    /// This class handles the behavior of all the bombs.
    /// It instantiates the bombs and handles the spawning and recycling of them.
    /// </summary>
    void Start()
    {
        if (!_enabled) return;
        GameObject gridObject = GameObject.FindWithTag(GlobalStrings.NAME_BOMBERGRID);
        grid = gridObject.GetComponent<Grid>();
        gridOccupation = grid.GetComponent<GridOccupation>();

        character = heroGameObject.gameObject.GetComponent<HeroMovement>();

        for (int i = 0; i < bombs.Length; i++)
        {
            var bombObj = Instantiate(bombPrefab).GetComponent<Bomb>();
            bombs[i] = bombObj;
            bombObj.Hero = heroGameObject;
            bombObj.SetInactive();
        }

        character.Triggered += TrySpawnBomb;

    }

    /// <summary>
    /// This method activates a bomb.
    /// Only one bomb can get placed at a time until that bomb explodes.
    /// It also checks if the placement of the bomb is valid.
    /// If there is a collider nearby it will stop the player from placing a bomb
    /// Otherwise it will call the bomb class to handle the behavior of the bomb
    /// </summary>
    public void SetExplosionLength(int length)
    { _explosionLength = length; }
    public void IncreaseMaxBombs()
    {
        _maxCurrentBombs++;
    }

    private void TrySpawnBomb(object sender, EventArgs e)
    {
        if (character.IsGrabbing) return;

        int activeBombs = 0;
        for (int i = 0; i < bombs.Length; i++)
        {
            if (bombs[i].IsActive)
                activeBombs++;
        }
        if (activeBombs < _maxCurrentBombs) 
        {
            Vector3 middlePoint = GridCellMiddlePoint.Get(grid, character.GameObject.transform.position);
            RaycastHit hit;
            Physics.Raycast(middlePoint + new Vector3(0, 0.75f, 0), character.FaceDirection, out hit, grid.cellSize.x, bombLayerMask);
            if (!hit.collider && !gridOccupation.CheckOccupied(character.Hero) )
            {
                for (int i = 0; i < directions.Count; i++)
                {
                    var directionKey = directions.Keys.ElementAt(i);

                    if (directionKey == character.FaceDirection)
                    {
                        bombs[_currentBombIndexBuffer].SpawnBomb(middlePoint + directions[directionKey], _currentBombIndex + 1, _explosionLength);
                        _currentBombIndex = (_currentBombIndex + 1) % _maxCurrentBombs;     
                        _currentBombIndexBuffer++;
                        if (_currentBombIndexBuffer >= bombs.Length)
                            _currentBombIndexBuffer = 0;
                    }
                }
            }
            else
            {
                Debug.Log("Cant place");
            }
        }
    }
}
