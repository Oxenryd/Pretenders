using UnityEngine;

public static class GridCellMiddlePoint
{
    public static Vector3 Get(Grid grid, Vector3 characterPosition)
    {
        var sX = grid.cellSize.x;
        var sY = grid.cellSize.y;

        var cellX = (int)(characterPosition.x / sX);
        var cellY = (int)characterPosition.z / sY;

        var centerX = cellX * sX + cellX / 2;
        var centerY = cellY * sY + cellY / 2;

        return new Vector3(centerX, 0, centerY);
    }
}