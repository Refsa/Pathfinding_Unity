using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public class QuadTreeAStarComponent : MonoBehaviour
{
    [SerializeField] bool drawGrid;
    [SerializeField] bool drawQuadTree;
    [SerializeField] bool drawPath;

    [SerializeField] int gridSize;
    [SerializeField] int dir;
    int oldGridSize;

    Grid<int> grid;

    QuadTree quadTree;
    Dictionary<QuadTree, QuadTree> path;
    QuadTree startQuad;
    QuadTree endQuad;

    Stopwatch benchSW;

    void Start ( )
    {
        benchSW = new Stopwatch ( );

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
        if (grid == null)
        {
            grid = GridFactory.GenerateOuterUShape (gridSize, dir);
            //grid = GridFactory.GenerateInnerUShape (gridSize, dir);
        }
    }

    void GenerateQuadTree ( )
    {
        if (benchSW == null) benchSW = new Stopwatch ( );

        benchSW.Restart ( );
        int blocksize = (gridSize * 2) / 2 / 4;

        quadTree = new QuadTree (new Vector2 (-0.5f, -0.5f) + new Vector2 (gridSize, gridSize) / 2f, gridSize);

        foreach (var node in grid.Nodes)
        {
            if (node.value == 65535)
                quadTree.Insert (new Vector2 (node.x, node.y));
        }

        //ParallelEnumerable.Range (
        //    0, grid.Nodes.GetLength (0)
        //).ForAll (
        //    x => ParallelEnumerable.Range (0, grid.Nodes.GetLength (1)).ForAll (y =>
        //    {
        //        if (grid.Nodes[x, y].value == 65535)
        //            quadTree.Insert (new Vector2 (grid.Nodes[x, y].x, grid.Nodes[x, y].y));
        //    })
        //);

        benchSW.Stop ( );

        Debug.Log ($"Points: {quadTree.CountAll ( )} | SubTrees: {quadTree.SubdivisionCount ( )} | Time: {benchSW.ElapsedTicks / 10000f} ms");
    }

    void GeneratePath ( )
    {
        if (benchSW == null) benchSW = new Stopwatch ( );

        var start = new Vector2 (5f, 5f);
        var end = new Vector2 (gridSize - 1, gridSize - 1);

        startQuad = quadTree.QueryQuadTreeNode (start);
        endQuad = quadTree.QueryQuadTreeNode (end);

        benchSW.Restart ( );
        AStar.QuadTreeAStar (quadTree, startQuad, endQuad, out path);
        benchSW.Stop ( );

#if UNITY_EDITOR
        quadTree.ResetPQ ( );
#endif

        Debug.Log ("Path Count: " + path.Count + " Time: " + (benchSW.ElapsedTicks / 10000f).ToString ("#.####") + " ms");
    }

    private void OnGUI ( )
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea (new Rect (0f, dir * 50f, 250f, 150f));

        GUILayout.Label ("Last Path Time: " + (benchSW.ElapsedTicks / 10000f).ToString ("#.####"));

        GUILayout.EndArea ( );
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
                Gizmos.DrawWireCube (transform.position + new Vector3 (current.Boundary.center.x, 0f, current.Boundary.center.y),
                    new Vector3 (current.Boundary.size.x, 1f, current.Boundary.size.y));
                if (!path.TryGetValue (current, out current))
                {
                    break;
                }
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
            Gizmos.DrawWireCube (transform.position + new Vector3 (quadTree.Boundary.center.x, 0f, quadTree.Boundary.center.y),
                new Vector3 (quadTree.Boundary.size.x, 1f, quadTree.Boundary.size.y));
        }
    }
}
