using UnityEngine;

public static class GridFactory
{
    public static Grid<int> GenerateOuterUShape (int gridSize, int dir = 0)
    {
        var grid = new Grid<int> (gridSize, gridSize, GridLayout.FourWay, 0);

        int centeri = gridSize / 2;
        int centerj = gridSize / 2;
        int blocksize = (gridSize * 2) / 2 / 4;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (i > centeri - blocksize && i < centeri + blocksize && (dir == 0 || dir == 1))
                {
                    if ((dir == 0 && j > centerj - blocksize) || (dir == 1 && j < centerj + blocksize))
                        grid.SetNode (i, j, 65535);
                }
                else if (j > centerj - blocksize && j < centerj + blocksize && (dir == 2 || dir == 3))
                {
                    if ((dir == 2 && i > centeri - blocksize) || (dir == 3 && i < centeri + blocksize))
                        grid.SetNode (i, j, 65535);
                }
                else
                {
                    grid.SetNode (i, j, 10);
                }
            }
        }

        return grid;
    }

    public static Grid<int> GenerateInnerUShape (int gridSize, int dir = 0)
    {
        var grid = new Grid<int> (gridSize, gridSize, GridLayout.FourWay, 0);

        int centeri = gridSize / 2;
        int centerj = gridSize / 2;
        int blocksize = (gridSize * 2) / 2 / 4;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                // Generate U-Shape
                if (i > centeri - blocksize && i < centeri + blocksize && j > centerj - blocksize && j < centerj + blocksize)
                {
                    if (dir == 0 && (i <= centeri - blocksize || i >= centeri + blocksize / 2 || j <= centerj - blocksize / 2 || j >= centerj + blocksize / 2))
                    {
                        grid.SetNode (i, j, 65535);
                    }
                    else if (dir == 1 && (i <= centeri - blocksize / 2 || i >= centeri + blocksize / 2 || j <= centerj - blocksize / 2 || j >= centerj + blocksize))
                    {
                        grid.SetNode (i, j, 65535);
                    }
                    else if (dir == 2 && (i <= centeri - blocksize / 2 || i >= centeri + blocksize / 2 || j <= centerj - blocksize || j >= centerj + blocksize / 2))
                    {
                        grid.SetNode (i, j, 65535);
                    }
                    else if (dir == 3 && (i <= centeri - blocksize / 2 || i >= centeri + blocksize || j <= centerj - blocksize / 2 || j >= centerj + blocksize / 2))
                    {
                        grid.SetNode (i, j, 65535);
                    }
                }
                else
                {
                    grid.SetNode (i, j, 10);
                }
            }
        }

        return grid;
    }
}
