using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// This class controls which tiles are occupied by characters and bombs in bomberman minigame.
/// </summary>
public class GridOccupation : MonoBehaviour
{
    [SerializeField]
    private Grid _grid;

    [SerializeField]
    private float _ratioX = 0.7f;

    [SerializeField]
    private float _ratioY = 0.75f;

    [SerializeField]
    private Vector2[] _occupiedTiles = new Vector2[16];

    /// <summary>
    /// Get the tile which passed in hero is currently in according to the grid.
    /// </summary>
    /// <param name="hero"></param>
    /// <returns></returns>
    public Vector2 GetHeroTile(Hero hero)
    {
        return _occupiedTiles[hero.Index];
    }
    /// <summary>
    /// Check if next tile is occupied in regards of the direction passed in hero is trying to move in.<br></br>
    /// Returns true if the tile is occupied.
    /// </summary>
    /// <param name="hero"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Get the tile center world coordinates based on a hero.
    /// </summary>
    /// <param name="hero"></param>
    /// <returns></returns>
    public Vector3 TileCenter(Hero hero)
    {
        return TileCenter(_occupiedTiles[hero.Index]);
    }
    /// <summary>
    /// Get world position of tile center of tile passed in.
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public Vector3 TileCenter(Vector2 tile)
    {
        return new Vector3(tile.x * _grid.cellSize.x + _grid.cellSize.x / 2, 0f, tile.y * _grid.cellSize.y + _grid.cellSize.y / 2);
    }
    public Vector3 TileCenter(int x, int y) => TileCenter(new Vector2(x, y));
   
    /// <summary>
    /// Mark the tile passed in Hero is trying to move to as occupied by it.
    /// </summary>
    /// <param name="hero"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Set current tile as occupied by the bomb passed in as parameter.
    /// </summary>
    /// <param name="bomb"></param>
    /// <returns></returns>
    public Vector2 SetOccupiedByBomb(Bomb bomb, int bombIndex)
    {
        var hero = bomb.Hero;
        _occupiedTiles[hero.Index + 4 * bombIndex] = XyFromVector3(bomb.transform.position);
        return _occupiedTiles[hero.Index + 4 * bombIndex];
    }

    /// <summary>
    /// Remove the hero from the grid by setting the occupied tile of hero with index to (-1, -1). 
    /// </summary>
    /// <param name="heroIndex"></param>
    public void RemoveHero(int heroIndex)
    {
        _occupiedTiles[heroIndex] = new Vector2(-1, -1);
    }
    public void RemoveHero(Hero hero) => RemoveHero(hero.Index);

    /// <summary>
    /// Remove the bomb from the grid by setting its occupied tile to (-1, -1).
    /// </summary>
    /// <param name="bomb"></param>
    public void RemoveOccupiedByBomb(Bomb bomb, int bombIndex)
    {
        var hero = bomb.Hero;
        _occupiedTiles[hero.Index + 4 * bombIndex] = new Vector2(-1, -1);
    }
    /// <summary>
    /// Get the tile a bomb belonging to hero
    /// </summary>
    /// <param name="hero"></param>
    /// <returns></returns>
    public Vector2 GetTileBomb(Hero hero, int bombIndex)
    {
        return _occupiedTiles[hero.Index + 4 * bombIndex];
    }

    /// <summary>
    /// Force a specific tile to occupied based on hero index and a position.
    /// </summary>
    /// <param name="heroIndex"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector2 SetOccupiedForced(int heroIndex, Vector3 position)
    {
        _occupiedTiles[heroIndex] = XyFromVector3(new Vector3(position.x, 0f, position.z));
        return _occupiedTiles[heroIndex];
    }

    /// <summary>
    /// Get the X,Y tile coordinates as a Vector2 based on a world position as Vector3
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Vector2 XyFromVector3(Vector3 position)
    {
       // int x = Mathf.RoundToInt(position.x / _grid.cellSize.x);
        //int y = Mathf.RoundToInt(position.z / _grid.cellSize.y);

        int x = (int)(position.x / _grid.cellSize.x);
        int y = (int)(position.z / _grid.cellSize.y);

        return new Vector2 (x, y);
    }

    void Start()
    {
        for (int i = 4; i < _occupiedTiles.Length; i++)
        {
            _occupiedTiles[i] = new Vector2(-1, -1);
        }
    }
}
