using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using Priority_Queue;

/// <summary>
/// A QuadTree optimized for dynamic point allocation
/// Clear all points, then reinsert them for best performance
/// </summary>
public class QuadTree : FastPriorityQueueNode
{
    static int maxNodes = 1;
    static float minBoundary = 0.5f;

    public float size;
    public Vector2 center;
    public Vector2[ ] points;

    int count;
    bool subdivided;
    bool isLeaf; // Not dynamic, shows if the QuadTree is at the given minimum boundary
    Rect boundary; // Used in queries and not modifications

    public QuadTree northWest;
    public QuadTree northEast;
    public QuadTree southWest;
    public QuadTree southEast;

    public bool Subdivided => subdivided;
    public int Count => count;
    public Rect Boundary => boundary;

    public QuadTree (Vector2 center, float size)
    {
        this.size = size;
        this.center = center;
        this.subdivided = false;
        this.boundary = new Rect (Vector2.zero, new Vector2 (size, size));
        this.boundary.center = center;

        points = new Vector2[50];
        count = 0;

        // Create sub-quadtrees until minimum boundary is reached.
        // Provides better approximation of performance as all quadtrees are pre-created
        if (this.size > minBoundary)
        {
            points = new Vector2[maxNodes + 1];
            isLeaf = false;

            var halfsize = this.size / 2f;
            var fourthsize = this.size / 4f;

            var nw = this.center + new Vector2 (-fourthsize, fourthsize);
            var ne = this.center + new Vector2 (fourthsize, fourthsize);
            var sw = this.center + new Vector2 (-fourthsize, -fourthsize);
            var se = this.center + new Vector2 (fourthsize, -fourthsize);

            northWest = new QuadTree (nw, halfsize);
            northEast = new QuadTree (ne, halfsize);
            southWest = new QuadTree (sw, halfsize);
            southEast = new QuadTree (se, halfsize);
        }
        else
        {
            isLeaf = true;
        }
    }

    /// <summary>
    /// Entry point for inserting points into the quadtree
    /// </summary>
    /// <param name="point"></param>
    public void Insert (Vector2 point)
    {
        if (subdivided)
        {
            InsertIntoSubQuad (point);
            return;
        }
        else if (isLeaf || count < maxNodes)
        {
            points[count++] = point;
            return;
        }

        Subdivide ( );
        InsertIntoSubQuad (point);
    }

    /// <summary>
    /// Inserts the point into the correct sub-quadtree based on the center point
    /// Does not do any bound checking, so a point outside the quad can be entered
    /// </summary>
    /// <param name="point"></param>
    public void InsertIntoSubQuad (Vector2 point)
    {
        bool gx = point.x >= center.x;
        bool gy = point.y >= center.y;

        // East/West Split
        if (gx)
        {
            // North/South Split
            if (gy)
                northEast.Insert (point);
            else
                southEast.Insert (point);
        }
        else
        {
            // North/South Split
            if (gy)
                northWest.Insert (point);
            else
                southWest.Insert (point);
        }
    }

    /// <summary>
    /// Moves points from this quadtree into the correct sub-quadtrees and
    /// marks itself as subdivided
    /// </summary>
    public void Subdivide ( )
    {
        for (int i = 0; i < count; i++)
        {
            InsertIntoSubQuad (points[i]);
        }
        count = 0;

        subdivided = true;
    }

    /// <summary>
    /// Sets the count to 0 this quadtree and all its sub-quadtrees
    /// </summary>
    public void Clear ( )
    {
        if (subdivided)
        {
            subdivided = false;
            northWest.Clear ( );
            northEast.Clear ( );
            southWest.Clear ( );
            southEast.Clear ( );
        }

        count = 0;
    }

    /// <summary>
    /// Counts the objects under this quadtree
    /// </summary>
    /// <returns>An int, which is the count of objects</returns>
    public int CountAll ( )
    {
        int c = count;
        if (subdivided)
        {
            c += northWest.CountAll ( );
            c += northEast.CountAll ( );
            c += southWest.CountAll ( );
            c += southEast.CountAll ( );
        }
        return c;
    }

    public int SubdivisionCount ( )
    {
        int c = 0;

        if (subdivided)
        {
            c += northWest.SubdivisionCount ( );
            c += northEast.SubdivisionCount ( );
            c += southWest.SubdivisionCount ( );
            c += southEast.SubdivisionCount ( );
        }
        else
        {
            c = 1;
        }

        return c;
    }

    public void Neighbours (QuadTree center, ref List<QuadTree> neighbours)
    {
        var horizontal = new Vector2 (center.size / 2f + minBoundary, 0f);
        var vertical = new Vector2 (0f, center.size / 2f + minBoundary);

        var left = center.center - horizontal;
        var right = center.center + horizontal;
        var up = center.center - vertical;
        var down = center.center + vertical;

        neighbours.Add (QueryQuadTreeNode (left));
        neighbours.Add (QueryQuadTreeNode (up));
        neighbours.Add (QueryQuadTreeNode (right));
        neighbours.Add (QueryQuadTreeNode (down));

        neighbours.RemoveAll (qt => qt == null);
    }

    public QuadTree QueryQuadTreeNode (Vector2 point)
    {
        if (!boundary.Contains (point))
            return null;

        QuadTree quadTree = null;

        if (subdivided)
        {
            quadTree = northWest.QueryQuadTreeNode (point);
            if (quadTree != null) return quadTree;
            quadTree = northEast.QueryQuadTreeNode (point);
            if (quadTree != null) return quadTree;
            quadTree = southWest.QueryQuadTreeNode (point);
            if (quadTree != null) return quadTree;
            quadTree = southEast.QueryQuadTreeNode (point);
            if (quadTree != null) return quadTree;
        }
        else
        {
            quadTree = this;
        }

        return quadTree;
    }

    /// <summary>
    /// Queries all points within the given range
    /// </summary>
    /// <param name="range">A rect to act as a bounding box for wanted points</param>
    /// <returns>A list of points that fell within the given range</returns>
    public List<Vector2> QueryRange (Rect range)
    {
        var result = new List<Vector2> ( );

        if (!boundary.Overlaps (range)) return result;

        if (subdivided)
        {
            result.AddRange (northWest.QueryRange (range));
            result.AddRange (northEast.QueryRange (range));
            result.AddRange (southWest.QueryRange (range));
            result.AddRange (southEast.QueryRange (range));
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                var v2 = new Vector2 (points[i].x, points[i].y);
                if (range.Contains (v2)) result.Add (v2);
            }
        }

        return result;
    }

    /// <summary>
    /// Simpler query with less garbage that checks if any point is within the collision range of another.
    /// Uses circles to check range
    /// </summary>
    /// <param name="range">The bounding box of the object to query</param>
    /// <param name="point">The center point of the object to query</param>
    /// <returns>true/false depending on if something is colliding with the object</returns>
    public bool QueryCollision (Rect range, Vector2 point)
    {
        if (!boundary.Overlaps (range)) return false;

        if (subdivided)
        {
            if (northWest.QueryCollision (range, point)) return true;
            if (northEast.QueryCollision (range, point)) return true;
            if (southWest.QueryCollision (range, point)) return true;
            if (southEast.QueryCollision (range, point)) return true;
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                if (points[i] == point) continue;
                if (Vector2.Distance (point, points[i]) < range.size.x) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Simple check to see if the quadtree contains the given point
    /// </summary>
    /// <param name="point">The point to look for</param>
    /// <returns>true/false if the point was found or not</returns>
    public bool QueryPoint (Vector2 point)
    {
        if (points.Contains (point)) return true;

        if (subdivided)
        {
            if (northWest.QueryPoint (point)) return true;
            if (northEast.QueryPoint (point)) return true;
            if (southWest.QueryPoint (point)) return true;
            if (southEast.QueryPoint (point)) return true;
        }

        return false;
    }

    public void ResetPQ ( )
    {
#if UNITY_EDITOR
        Queue = null;
        QueueIndex = 0;
#endif

        if (subdivided)
        {
            northWest.ResetPQ ( );
            northEast.ResetPQ ( );
            southWest.ResetPQ ( );
            southEast.ResetPQ ( );
        }
    }
}
