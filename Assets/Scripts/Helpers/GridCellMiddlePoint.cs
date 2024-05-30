using UnityEngine;
using UnityEngine.TextCore.Text;

public static class GridCellMiddlePoint
{
    public static Vector3 Get(Grid grid, Vector3 characterPosition)
    {
        return new Vector3((int)(characterPosition.x / grid.cellSize.x) * grid.cellSize.x + grid.cellSize.x / 2, 0f, (int)(characterPosition.z / grid.cellSize.y) * grid.cellSize.y + grid.cellSize.y / 2);
    }
}