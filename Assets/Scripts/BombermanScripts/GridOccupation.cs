using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class GridOccupation : MonoBehaviour
{
    [SerializeField]
    private Grid _grid;

    [SerializeField]
    private float _ratioX = 0.7f;

    [SerializeField]
    private float _ratioY = 0.75f;

    [SerializeField]
    private Vector2[] _occupiedTiles = new Vector2[8];

    public Vector2 GetHeroTile(Hero hero)
    {
        return _occupiedTiles[hero.Index];
    }

    public bool CheckOccupied(Hero hero)
    {
        var movement = hero.Movement;

        var directionCheck = new Vector3(movement.FaceDirection.x * _grid.cellSize.x * _ratioX, 0f, movement.FaceDirection.z * _grid.cellSize.y * _ratioY);

        var xy = XyFromVector3(movement.transform.position + directionCheck);

        for (int i = 0; i < _occupiedTiles.Length; i++)
        {
            if (i == hero.Index) continue;
            if (xy == _occupiedTiles[i])
                return true;
        }
        return false;
    }

    public Vector3 TileCenter(Hero hero)
    {
        return TileCenter(_occupiedTiles[hero.Index]);
    }
    public Vector3 TileCenter(Vector2 tile)
    {
        return new Vector3(tile.x * _grid.cellSize.x + _grid.cellSize.x / 2, 0f, tile.y * _grid.cellSize.y + _grid.cellSize.y / 2);
    }
    public Vector3 TileCenter(int x, int y) => TileCenter(new Vector2(x, y));
   
    public Vector2 SetOccupied(Hero hero)
    {
        var movement = hero.Movement;
        var directionCheck = new Vector3(movement.FaceDirection.x * _grid.cellSize.x * _ratioX, 0f, movement.FaceDirection.z * _grid.cellSize.y * _ratioY);
        _occupiedTiles[hero.Index] = XyFromVector3(movement.transform.position + directionCheck);
        return _occupiedTiles[hero.Index];
    }

    //public Vector2 GetHeroIsInTile(Hero hero)
    //{
    //    return XyFromVector3(hero.transform.position);
    //}

    public Vector2 SetOccupiedByBomb(Bomb bomb)
    {
        var hero = bomb.Hero;
        _occupiedTiles[hero.Index + 4] = XyFromVector3(bomb.transform.position);
        return _occupiedTiles[hero.Index + 4];
    }

    public void RemoveHero(int heroIndex)
    {
        _occupiedTiles[heroIndex] = new Vector2(-1, -1);
    }
    public void RemoveHero(Hero hero) => RemoveHero(hero.Index);

    public void RemoveOccupiedByBomb(Bomb bomb)
    {
        var hero = bomb.Hero;
        _occupiedTiles[hero.Index + 4] = new Vector2(-1, -1);
    }

    public Vector2 SetOccupiedForced(int heroIndex, Vector3 position)
    {
        _occupiedTiles[heroIndex] = XyFromVector3(new Vector3(position.x, 0f, position.z));
        return _occupiedTiles[heroIndex];
    }

    public Vector2 XyFromVector3(Vector3 position)
    {
       // int x = Mathf.RoundToInt(position.x / _grid.cellSize.x);
        //int y = Mathf.RoundToInt(position.z / _grid.cellSize.y);

        int x = (int)(position.x / _grid.cellSize.x);
        int y = (int)(position.z / _grid.cellSize.y);

        return new Vector2 (x, y);
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 4; i < _occupiedTiles.Length; i++)
        {
            _occupiedTiles[i] = new Vector2(-1, -1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
