# Pathfinding_Unity
Different pathfinding implementations in the Unity Game Engine

# Currently Implemented Pathfinding Algorithms
## A*:
- [x] Solving on a QuadTree  
Currently the A* implementation for QuadTrees does not use custom cost for movement. This can be solved in different ways by using multiple layers or implementing the cost directly in the QuadTree node class. The latter would be more optimal in terms of performance, the former will allow you to make use of dynamic updates to the pathfinding with better performance.  
- [x] Solving on a Grid  
Uses a grid class which contains nodes that have the cost of movement. Currently this is not optimized for grid-based search.  
- [ ] Solving on a Graph
- [ ] Skip/Waypoint A*
- [ ] Solving on a NavMesh
- [ ] Threading Optimization
## D*:
- [ ] Solving on a QuadTree
- [ ] Solving on a Grid
- [ ] Solving on a Graph
## Flow-Field:
- [ ] Solving on a QuadTree
- [ ] Solving on a Grid
- [ ] Solving on a Graph
## RRT (Graph):
- [ ] RRG
- [ ] RRT*

# Currently Implemented Utilities
## Quad-Tree (Recursive, Managed):
The current implementation for QuadTrees can be used dynamically by clearing it and then reinserting all the points. It's not optimized for dynamic use, but can still handle reinserting >50k points.  
It's currently without its outer handler which means that it wont check if points are inside the QuadTrees boundary when inserted. This is a minor optimization tweak where it only uses the quadrant based on the center of the QuadTree node. Points outside the boundary will always end up on the edges of the corresponding quadrant it would end up in.  
You can run this on a thread, but that requires storing points to be updated in a container to be digested by the thread. Multiple Threads can also be created by splitting the QuadTree between multiple threads, but it does not support multi-threaded queries.  
- [x] Insertion
- [x] Collision Search
- [x] Point Search
- [x] Range Search
- [x] Nearest Neighbour Search
- [ ] Threading Optimization
- [ ] Runtime Collapse Down
- [ ] Runtime Collapse Up
- [ ] Skip QuadTree
- [ ] Generate Optimized NavMesh

## Nav-Mesh:
- [ ] NavMesh Graph
