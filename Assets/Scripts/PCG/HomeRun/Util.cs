using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Util class provides functions mainly used for path generation.
/// </summary>
public static class Util {
    /// <summary>
    /// Given a path returns its length in world coordinates.
    /// </summary>
    /// <param name="path">A list of path points which make up a path.</param>
    /// <returns>The path length.</returns>
    public static float DeterminePathLength(List<Vector3> path) {
        float length = 0;
        for(int i = 0; i < path.Count - 1; i++) {
            Vector3 difference = path[i] - path[i+1];
            length += difference.magnitude;
        }
        return length;
    }

    /// <summary>
    /// Determines whether (x,z) is a chunk border.
    /// </summary>
    /// <param name="chunkSize">The number of vertices along an axis of a chunk.</param>
    /// <param name="x">The x-index of the vertex within the chunk.</param>
    /// <param name="z">The z-index of the vertex within the chunk.</param>
    /// <returns>True if (x,z) is a chunk border, false otherwise.</returns>
    public static bool IsChunkBorder(int chunkSize, int x, int z) {
        return x == 0 || x == chunkSize || z == 0 || z == chunkSize;
    }

    /// <summary>
    /// Returns the chunk index and vertex index of the next chunk when leaving a current chunk in a certain direction.
    /// </summary>
    /// <param name="currentChunk">The index of the current chunk within a chunk grid.</param>
    /// <param name="exitPositionCurrentChunk">The index of the vertex within the current chunk. This should be a chunk border.</param>
    /// <param name="direction">The direction of the exit.</param>
    /// <param name="chunkSize">The chunk size.</param>
    /// <returns>A tuple. The first element is the index of the next chunk within the chunk grid. The second element is the index of the vertex within the next chunk.</returns>
    public static ((int,int),(int,int)) NextChunk((int,int) currentChunk, (int,int) exitPositionCurrentChunk, Direction direction, int chunkSize) {
        switch(direction) {
            case Direction.NORTH:
                currentChunk = (currentChunk.Item1, currentChunk.Item2 + 1);
                exitPositionCurrentChunk.Item2 = 0;
                break;
            case Direction.NORTHEAST:
                currentChunk = (currentChunk.Item1 + 1, currentChunk.Item2 + 1);
                exitPositionCurrentChunk = (0,0);
                break;
            case Direction.EAST:
                currentChunk = (currentChunk.Item1 + 1, currentChunk.Item2);
                exitPositionCurrentChunk.Item1 = 0;
                break;
            case Direction.SOUTHEAST:
                currentChunk = (currentChunk.Item1 + 1, currentChunk.Item2 - 1);
                exitPositionCurrentChunk = (0, chunkSize);
                break;
            case Direction.SOUTH:
                currentChunk = (currentChunk.Item1, currentChunk.Item2 - 1);
                exitPositionCurrentChunk.Item2 = chunkSize;
                break;
            case Direction.SOUTHWEST:
                currentChunk = (currentChunk.Item1 - 1, currentChunk.Item2 - 1);
                exitPositionCurrentChunk = (chunkSize, chunkSize);
                break;
            case Direction.WEST:
                currentChunk = (currentChunk.Item1 - 1, currentChunk.Item2);
                exitPositionCurrentChunk.Item1 = chunkSize;
                break;
            case Direction.NORTHWEST:
                currentChunk = (currentChunk.Item1 - 1, currentChunk.Item2 + 1);
                exitPositionCurrentChunk = (chunkSize, 0);
                break;
        }
        return (currentChunk, exitPositionCurrentChunk);
    }

    /// <summary>
    /// Determines the what chunk border a vertex lies on and returns the direction of this chunk border.
    /// </summary>
    /// <param name="chunkSize">The chunk size.</param>
    /// <param name="x">The x-index of the vertex within the chunk.</param>
    /// <param name="z">The z-index of the vertex within the chunk.</param>
    /// <returns>The direction of the chunk border a given vertex lies on.</returns>
    public static Direction ChunkBorderDirection(int chunkSize, int x, int z) {
        if(z == chunkSize) {
            if(x == chunkSize) return Direction.NORTHEAST;
            else if(x == 0) return Direction.NORTHWEST;
            return Direction.NORTH;
        }
        else if (x == chunkSize) return Direction.EAST;
        else if (x == 0) return Direction.WEST;
        else return Direction.SOUTH; //no need to differenciate between south variants
    }

    /// <summary>
    /// Maps a direction to a Vector3.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <returns>The corresponding direction.</returns>
    /// <exception cref="Exception">This exception never occurs.</exception>
    public static Vector3 DirectionToVector(Direction direction) {
        return direction switch
        {
            Direction.NORTH => Vector3.forward,
            Direction.NORTHEAST => Vector3.forward + Vector3.right,
            Direction.EAST => Vector3.right,
            Direction.SOUTHEAST => Vector3.right + Vector3.back,
            Direction.SOUTH => Vector3.back,
            Direction.SOUTHWEST => Vector3.back + Vector3.left,
            Direction.WEST => Vector3.left,
            Direction.NORTHWEST => Vector3.left + Vector3.forward,
            _ => throw new Exception("Cannot map direction "+direction)
        };
    }

    /// <summary>
    /// Determines the minimum distance to a line segment from a point p.
    /// </summary>
    /// <param name="from">The starting point of the line segment.</param>
    /// <param name="to">The end point of the line segment.</param>
    /// <param name="p">The point.</param>
    /// <returns>The minimum distance to the line segment from point p.</returns>
    public static (float, float) MinimumDistanceToLineSegment(Vector2 from, Vector2 to, Vector2 p) {
        //https://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment
        // Return minimum distance between line segment vw and point p
        float l2 = DistanceSquared(from, to);  // i.e. |w-v|^2 -  avoid a sqrt
        if (l2 == 0.0) return (Vector2.Distance(from, p), 1);   // v == w case
        // Consider the line extending the segment, parameterized as v + t (w - v).
        // We find projection of point p onto the line.
        // It falls where t = [(p-v) . (w-v)] / |w-v|^2
        // We clamp t from [0,1] to handle points outside the segment vw.
        float t = Math.Max(0, Math.Min(1, Vector2.Dot(p - from, to - from) / l2));
        Vector2 projection = from + t * (to - from);  // Projection falls on the segment
        return (Vector2.Distance(projection, p), t);
    }

    /// <summary>
    /// Calculates the euclidian distance between two points, squared.
    /// </summary>
    /// <param name="v">The starting point.</param>
    /// <param name="w">The end point.</param>
    /// <returns>The euclidian distance between two points, squared.</returns>
    private static float DistanceSquared(Vector2 v, Vector2 w) {
        float x = v.x - w.x;
        float y = v.y - w.y;
        return x*x + y*y;
    }
}