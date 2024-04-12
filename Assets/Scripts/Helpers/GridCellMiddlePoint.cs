using UnityEngine;
using UnityEngine.TextCore.Text;

public static class GridCellMiddlePoint
{
    public static Vector3 Get(Grid grid, Vector3 characterPosition)
    {
        return new Vector3(Mathf.RoundToInt(characterPosition.x / grid.cellSize.x) * grid.cellSize.x, 0, Mathf.RoundToInt(characterPosition.z / grid.cellSize.z) * grid.cellSize.z);
    }
}