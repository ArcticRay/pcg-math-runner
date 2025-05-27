using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TerrainChunkSystem : MonoBehaviour
{
    public GameObject chunkPrefab;
    public GameObject player;
    public WorldGenerationParameters parameters;
    public bool updateChunkgrid;
    private Dictionary<(int, int), GameObject> chunks;
    private Dictionary<(int, int), List<Vector3>> pathPointsOfChunk;
    private (int, int) previousChunkIndex;
    private (int, int) currentChunkIndex;
    void Start()
    {
        if (!updateChunkgrid) return;
        WorldGenerationParameters parameters = WorldGenerationParameterSerialization.GetWorldGenerationParameters();
        previousChunkIndex = (0, 0);
        currentChunkIndex = previousChunkIndex;
        GenerateChunkGrid(parameters);
    }

    void Update()
    {
        if (!updateChunkgrid) return;
        player = GameObject.Find("Player");
        Vector3 playerPosition = player.transform.position;
        (int, int) chunkIndex = (
                (int)Math.Floor(
                    playerPosition.x / parameters.ScaledChunkSize()
                ),
                (int)Math.Floor(
                    playerPosition.z / parameters.ScaledChunkSize()
                )
            );

        if (chunkIndex != currentChunkIndex)
        {
            previousChunkIndex = currentChunkIndex;
            currentChunkIndex = chunkIndex;
            UpdateChunks(chunkIndex, parameters);
        }
    }
    public void GenerateChunkGrid(WorldGenerationParameters parameters)
    {
        if (parameters == null)
        {
            return;
        }
        else
        {
            parameters.RevalidateParameters();
            this.parameters = parameters;
        }

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        try
        {
            GeneratePath(parameters);
            UpdateChunks(currentChunkIndex, parameters);
            Debug.Log("Time taken: " + stopwatch.ElapsedMilliseconds + " ms");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            Debug.LogError("All retries failed, reloading scene.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }

    public void UpdateChunks((int, int) currentPosition, WorldGenerationParameters parameters)
    {
        List<(int, int)> chunksToRemove = new List<(int, int)>(chunks.Keys);

        /* Add new chunks */
        int renderDistance = parameters.renderDistance;
        for (int x = currentPosition.Item1 - renderDistance; x <= currentPosition.Item1 + renderDistance; x++)
        {
            for (int z = currentPosition.Item2 - renderDistance; z <= currentPosition.Item2 + renderDistance; z++)
            {
                (int, int) chunkIndex = (x, z);
                if (!chunks.ContainsKey(chunkIndex))
                {
                    /* Chunk not generated yet -> generate */
                    int chunkX = x * this.parameters.ScaledChunkSize();
                    int chunkZ = z * this.parameters.ScaledChunkSize();
                    GameObject chunk = Instantiate(chunkPrefab, new Vector3(chunkX, 0, chunkZ), Quaternion.identity);
                    chunk.GetComponent<TerrainChunkGenerator>().GenerateDecoratedChunk(this.parameters, GetPathPointsOfNeighboringChunks(x, z));
                    chunks.Add(chunkIndex, chunk);
                }
                else
                {
                    /* Chunk already exists and is within render distance -> do not delete */
                    chunksToRemove.Remove(chunkIndex);
                }
            }
        }

        /* Remove old chunks */
        foreach ((int, int) chunkIndex in chunksToRemove)
        {
            if (chunks.TryGetValue(chunkIndex, out GameObject chunk))
            {
                DestroyImmediate(chunk);
                chunks.Remove(chunkIndex);
            }
        }
    }

    public List<List<Vector3>> GetPathPointsOfNeighboringChunks(int chunkIndexX, int chunkIndexZ)
    {
        List<List<Vector3>> result = new();
        for (int x = chunkIndexX - 1; x <= chunkIndexX + 1; x++)
        {
            for (int z = chunkIndexZ - 1; z <= chunkIndexZ + 1; z++)
            {
                List<Vector3> chunkPathPoints;
                if (pathPointsOfChunk.TryGetValue((x, z), out chunkPathPoints))
                {
                    result.Add(chunkPathPoints);
                }
            }
        }
        return result;
    }

    public void GeneratePath(WorldGenerationParameters parameters)
    {
        chunks = new();
        pathPointsOfChunk = new();
        int withinChunkX = parameters.chunkSize / 2;
        int withinChunkZ = 0;
        (int, int) currentChunkIndex = (0, 0);
        Vector3 tangent = Vector3.forward;

        List<Vector3> pathPoints = new();
        //pathPoints.Add(new Vector3(parameters.ScaledChunkSize()/2f, 0, 0));

        float desiredLength = parameters.walkDuration;
        float pathLength = 0;
        Direction direction = Direction.NORTH;
        int i = 0;
        while (pathLength < desiredLength)
        {
            if (direction == Direction.NORTHWEST || direction == Direction.NORTHEAST) print(direction + " in " + currentChunkIndex);
            //print(direction + " " + currentChunk);
            /* Generate Heightmap */
            int chunkX = currentChunkIndex.Item1 * parameters.ScaledChunkSize();
            int chunkZ = currentChunkIndex.Item2 * parameters.ScaledChunkSize();
            GameObject currentChunkPrefab = Instantiate(chunkPrefab, new Vector3(chunkX, 0, chunkZ), Quaternion.identity);
            TerrainChunkGenerator chunkGenerator = currentChunkPrefab.GetComponent<TerrainChunkGenerator>();
            chunkGenerator.GenerateHeightMapChunk(parameters);


            /* Generate Path on Heightmap */
            //Add points to path
            List<Vector3> newPoints;
            (withinChunkX, withinChunkZ, tangent, newPoints) = chunkGenerator.GeneratePathWithinChunk(pathPoints.Count > 0 ? pathPoints[pathPoints.Count - 1] : Vector3.zero, withinChunkX, withinChunkZ, parameters.chunkSize, tangent, direction, parameters);
            pathLength += Util.DeterminePathLength(newPoints);
            if (pathLength > desiredLength)
            {
                newPoints = ShortenPath(newPoints, pathLength - desiredLength);
            }
            pathPoints.AddRange(newPoints); //newPoints includes the point on the chunk border where the path stops, but not the one where it starts (except the chunk at (0,0))
            //Add points to current chunks' path segment
            if (currentChunkIndex != (0, 0))
            {
                newPoints.Insert(0, pathPoints[pathPoints.Count - 1 - newPoints.Count]); //add the last path point added before this iteration to the path points of the current chunk
            }
            pathPointsOfChunk.Add(currentChunkIndex, newPoints);


            /* Destroy Heightmap again */
            DestroyImmediate(currentChunkPrefab);


            /* Calculate next chunks' index */
            direction = Util.ChunkBorderDirection(parameters.chunkSize, withinChunkX, withinChunkZ);
            (currentChunkIndex, (withinChunkX, withinChunkZ)) = Util.NextChunk(currentChunkIndex, (withinChunkX, withinChunkZ), direction, parameters.chunkSize);


            /* Prevent freeze of application in case of error */
            if (i == 100) throw new Exception("Too many iterations in terrain chunk system");
            i++;
        }

        /* Create spline */
        for (int j = 0; j < pathPoints.Count - 1; j++)
        {
            float distance = (pathPoints[j] - pathPoints[j + 1]).magnitude;
            if (distance < parameters.minPathSegmentLength * parameters.scale)
            {
                pathPoints.RemoveAt(j + 1);
            }
        }
        SplineManagerNew splineManager = FindFirstObjectByType<SplineManagerNew>();
        splineManager.UseMasterControlPoints(pathPoints);
    }

    private List<Vector3> ShortenPath(List<Vector3> pathPoints, float distance)
    {
        while (distance >= 0 && pathPoints.Count >= 2)
        {
            Vector3 last = pathPoints[pathPoints.Count - 1];
            Vector3 beforeLast = pathPoints[pathPoints.Count - 2];
            distance -= (last - beforeLast).magnitude;
            pathPoints.Remove(last);
        }
        return pathPoints;
    }

    public void DestroyChunkGrid()
    {
        if (chunks == null) return;
        foreach (GameObject chunk in chunks.Values)
        {
            DestroyImmediate(chunk);
        }
        chunks.Clear();
    }
}

public class PathGenerationException : Exception
{
    public PathGenerationException(string message) : base(message) { }
}