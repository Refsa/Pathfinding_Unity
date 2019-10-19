using System.Runtime.CompilerServices;

public enum GridLayout
{
    FourWay,
    EightWay
}

public class GridNode<T>
{
    public int x;
    public int y;
    public T value;

    /// <summary>
    /// N, S, E, W,
    /// NE, NW, SE, SW
    /// </summary>
    public (int, int) [ ] neighbours;

    public GridNode (int x, int y, T value = default (T))
    {
        this.x = x;
        this.y = y;
        this.value = value;
    }

    public void Setup (int width, int height, GridLayout layout)
    {
        if (layout == GridLayout.FourWay)
            this.neighbours = new (int, int) [4];
        else
            this.neighbours = new (int, int) [8];

        var px = x + 1;
        var nx = x - 1;
        var py = y + 1;
        var ny = y - 1;

        if (InBounds (x, py, width, height)) neighbours[0] = (x, py);
        if (InBounds (x, ny, width, height)) neighbours[1] = (x, ny);
        if (InBounds (px, y, width, height)) neighbours[2] = (px, y);
        if (InBounds (nx, y, width, height)) neighbours[3] = (nx, y);

        if (layout == GridLayout.EightWay)
        {
            if (InBounds (px, py, width, height)) neighbours[4] = (px, py);
            if (InBounds (nx, py, width, height)) neighbours[5] = (nx, py);
            if (InBounds (px, ny, width, height)) neighbours[6] = (px, ny);
            if (InBounds (nx, ny, width, height)) neighbours[7] = (nx, ny);
        }
    }

    [MethodImpl (MethodImplOptions.AggressiveInlining)]
    bool InBounds (int px, int py, int maxX, int maxY)
    {
        if (px >= 0 && px < maxX && py >= 0 && py < maxY)
            return true;
        return false;
    }
}

public class Grid<T>
{
    public int width;
    public int height;
    public GridLayout layout;

    GridNode<T>[, ] nodes;

    public GridNode<T>[, ] Nodes => nodes;

    public Grid (int width, int height, GridLayout layout, T defaultvalue = default (T))
    {
        this.layout = layout;
        this.width = width;
        this.height = height;

        nodes = new GridNode<T>[width, height];

        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                nodes[i, j] = new GridNode<T> (i, j, defaultvalue);
                nodes[i, j].Setup (width, height, layout);
            }
    }

    public GridNode<T> GetNode (int x, int y)
    {
        if (x < width && y < height)
            return nodes[x, y];

        return new GridNode<T> (0, 0);
    }

    public T GetNodeValue (int x, int y)
    {
        return nodes[x, y].value;
    }

    public void SetNode (int x, int y, T value)
    {
        nodes[x, y].value = value;
    }
}
