#define LOG_TIME
#undef LOG_TIME

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using Stopwatch = System.Diagnostics.Stopwatch;
using System.Linq;

public static class FlowField
{
    public static void FlowFieldQuadTree (QuadTree quadTree, QuadTree end, out Dictionary<QuadTree, QuadTree> flowField)
    {
#if LOG_TIME
        var bench = new Stopwatch ( );
        var avgNeighbourLookup = new List<long> ( );
#endif

#if DEBUG
        var timeout = new Stopwatch ( );
        timeout.Start ( );
#endif

        flowField = new Dictionary<QuadTree, QuadTree> ( );

        var openset = new FastPriorityQueue<QuadTree> (quadTree.SubdivisionCount ( ));
        var neighbours = new List<QuadTree> ( );

        openset.Enqueue (end, 0f);
        flowField[end] = end;

        QuadTree current = null;
        float newcost = 0f;

        while (openset.Count > 0)
        {
#if DEBUG
            if (timeout.ElapsedMilliseconds > 1000) throw new System.OperationCanceledException ("flowField Generation Timed Out");
#endif

            current = openset.Dequeue ( );

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

                newcost = current.Priority + (current.center - neighbour.center).sqrMagnitude + (neighbour.center - end.center).sqrMagnitude;

                if (newcost < neighbour.Priority)
                {
                    if (!openset.Contains (neighbour))
                        openset.Enqueue (neighbour, newcost);
                    else
                        openset.UpdatePriority (neighbour, newcost);

                    flowField[neighbour] = current;
                }
            }

            neighbours.Clear ( );
        }

#if LOG_TIME
        Debug.Log ("Average Neighbour Lookup: " + avgNeighbourLookup.Average ( ) + " total " + avgNeighbourLookup.Sum ( ));
#endif
    }
}
