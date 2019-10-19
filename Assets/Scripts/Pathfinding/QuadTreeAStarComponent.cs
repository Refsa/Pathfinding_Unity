using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public class QuadTreeAStarComponent : MonoBehaviour
{
    [SerializeField] bool drawGrid;
    [SerializeField] bool drawQuadTree;
    [SerializeField] bool drawPath;

    [SerializeField] int gridSize;
    int oldGridSize;

    Grid<int> grid;

    QuadTree quadTree;
    Dictionary<QuadTree, QuadTree> path;
    QuadTree startQuad;
    QuadTree endQuad;

    void Start ( )
    {
        GenerateGrid ( );
        GenerateQuadTree ( );
        GeneratePath ( );
    }

    void Update ( )
    {
        GeneratePath ( );
    }

    void GenerateGrid ( )
    {
        grid = new Grid<int> (gridSize, gridSize, GridLayout.FourWay, 0);

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
                    if (i <= centeri - blocksize || i >= centeri + blocksize / 2 || j <= centerj - blocksize / 2 || j >= centerj + blocksize / 2)
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
    }

    void GenerateQuadTree ( )
    {
        int blocksize = (gridSize * 2) / 2 / 4;

        quadTree = new QuadTree (new Vector2 (-0.5f, -0.5f) + new Vector2 (gridSize, gridSize) / 2f, gridSize);

        foreach (var node in grid.Nodes)
        {
            if (node.value == 65535)
                quadTree.Insert (new Vector2 (node.x, node.y));
        }

        Debug.Log ($"Points: {quadTree.CountAll ( )} | SubTrees: {quadTree.SubdivisionCount ( )}");
    }

    void GeneratePath ( )
    {
        var sw = new Stopwatch ( );

        var start = new Vector2 (5f, 5f);
        var end = new Vector2 (gridSize - 1, gridSize - 1);

        startQuad = quadTree.QueryQuadTreeNode (start);
        endQuad = quadTree.QueryQuadTreeNode (end);

        sw.Start ( );
        AStar.QuadTreeAStar (quadTree, startQuad, endQuad, out path);
        sw.Stop ( );

        quadTree.ResetPQ ( );

        Debug.Log ("Path Count: " + path.Count + " Time: " + (sw.ElapsedTicks / 10000f).ToString ("#.####") + " ms");
    }

    void OnDrawGizmos ( )
    {
        if (grid == null || gridSize != oldGridSize)
            GenerateGrid ( );

        if (drawGrid)
            foreach (var node in grid.Nodes)
            {
                var pos = transform.position + new Vector3 (node.x, 0, node.y);
                if (node.value == 65535) Gizmos.color = Color.red;
                else Gizmos.color = Color.green;
                Gizmos.DrawWireCube (pos, Vector3.one);
            }

        if (quadTree == null || gridSize != oldGridSize)
            GenerateQuadTree ( );

        Gizmos.color = Color.magenta;
        if (drawQuadTree)
            DrawBoundaries (quadTree);

        if (path == null || gridSize != oldGridSize)
            GeneratePath ( );

        if (drawPath)
        {
            Gizmos.color = Color.blue;

            var current = endQuad;

            for (int i = 0; i < path.Count; i++)
            {
                Gizmos.DrawWireCube (new Vector3 (current.Boundary.center.x, 0f, current.Boundary.center.y), new Vector3 (current.Boundary.size.x, 1f, current.Boundary.size.y));
                current = path[current];
            }
        }

        oldGridSize = gridSize;
    }

    void DrawBoundaries (QuadTree quadTree)
    {
        if (quadTree.Subdivided)
        {
            DrawBoundaries (quadTree.northWest);
            DrawBoundaries (quadTree.northEast);
            DrawBoundaries (quadTree.southWest);
            DrawBoundaries (quadTree.southEast);
        }
        else if (quadTree.Count == 0)
        {
            Gizmos.DrawWireCube (new Vector3 (quadTree.Boundary.center.x, 0f, quadTree.Boundary.center.y), new Vector3 (quadTree.Boundary.size.x, 1f, quadTree.Boundary.size.y));
        }
    }
}
