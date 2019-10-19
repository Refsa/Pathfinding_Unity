#define LOG_TIME
#undef LOG_TIME

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Linq;

public class AStar : MonoBehaviour
{
    public static void QuadTreeAStar (QuadTree quadTree, QuadTree start, QuadTree end, out Dictionary<QuadTree, QuadTree> path)
    {
#if LOG_TIME
        var bench = new Stopwatch ( );
        var avgNeighbourLookup = new List<long> ( );
#endif

#if DEBUG
        var timeout = new Stopwatch ( );
        timeout.Start ( );
#endif

        path = new Dictionary<QuadTree, QuadTree> ( );
        var openset = new FastPriorityQueue<QuadTree> (quadTree.SubdivisionCount ( ));
        var costs = new Dictionary<QuadTree, float> ( );

        var neighbours = new List<QuadTree> ( );

        openset.Enqueue (start, 0);
        costs[start] = 0;
        path[start] = start;

        while (openset.Count > 0)
        {
#if DEBUG
            if (timeout.ElapsedMilliseconds > 1000) throw new System.OperationCanceledException ("Path Generation Timed Out");
#endif
            if (openset.First == end)
                break;

            var current = openset.Dequeue ( );

#if LOG_TIME
            bench.Restart ( );
#endif
            quadTree.Neighbours (current, ref neighbours);

#if LOG_TIME
            bench.Stop ( );
            avgNeighbourLookup.Add (bench.ElapsedTicks);
#endif

            foreach (var neighbour in neighbours)
            {
                if (neighbour.Count != 0) continue;

                float newcost = costs[current] + (current.center - neighbour.center).sqrMagnitude * 10f;

                if (!costs.ContainsKey (neighbour) || newcost < costs[neighbour])
                {
                    costs[neighbour] = newcost;

                    float nprio = newcost + (neighbour.center - end.center).sqrMagnitude * 100f;//quadTree.size;

                    if (!openset.Contains (neighbour))
                        openset.Enqueue (neighbour, nprio);
                    else
                        openset.UpdatePriority (neighbour, nprio);

                    path[neighbour] = current;
                }
            }
            neighbours.Clear ( );
        }

#if LOG_TIME
        Debug.Log ("Average Neighbour Lookup: " + avgNeighbourLookup.Average ( ) + " total " + avgNeighbourLookup.Sum ( ));
#endif
    }
}
