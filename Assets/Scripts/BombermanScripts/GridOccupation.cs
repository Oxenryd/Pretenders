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
    private Vector2[] _occupiedTiles = new Vector2[4];

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
        return new Vector3(tile.x * _grid.cellSize.x, 0f, tile.y * _grid.cellSize.y);
    }
    public Vector3 TileCenter(int x, int y) => TileCenter(new Vector2(x, y));
   
    public void SetOccupied(Hero hero)
    {
        var movement = hero.Movement;
        var directionCheck = new Vector3(movement.FaceDirection.x * _grid.cellSize.x * _ratioX, 0f, movement.FaceDirection.z * _grid.cellSize.y * _ratioY);
        _occupiedTiles[hero.Index] = XyFromVector3(movement.transform.position + directionCheck);
    }

    public void SetOccupied(int heroIndex, Vector3 position)
    {
        _occupiedTiles[heroIndex] = XyFromVector3(new Vector3(position.x, 0f, position.z));
    }

    public Vector2 XyFromVector3(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / _grid.cellSize.x);
        int y = Mathf.RoundToInt(position.z / _grid.cellSize.y);

        return new Vector2 (x, y);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
