using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunkGenerator : MonoBehaviour
{

    public AnimationCurve convergenceToPath;
    public AnimationCurve terrainCurve;

    public Material grassMaterial;
    public Material sandMaterial;

    private Vector2 offset;
    private float[,] heightMap;
    private float[,] distanceToPath;

    float minimalHeightDifference = float.MaxValue;
    float minimalWaterPenalty = float.MaxValue;
    Vector3 tangentForMinimum = Vector3.zero;

    [Tooltip("How strongly the path should avoid water. 0 = ignore, larger = avoid more.")]
    public float waterAvoidancePenalty = 200f;

    private void InitializeOffset()
    {
        Vector3 position = GetComponent<Transform>().position;
        offset = new(position.x, position.z);
    }
    public void GenerateHeightMapChunk(WorldGenerationParameters parameters)
    {
        InitializeOffset();
        heightMap = GenerateHeightMap(parameters, offset, convergenceToPath, terrainCurve);
    }

    public void GenerateDecoratedChunk(WorldGenerationParameters parameters, List<List<Vector3>> neighboringChunkPathPoints)
    {
        GenerateHeightMapChunk(parameters);
        AdjustHeightmapToPath(parameters, neighboringChunkPathPoints);

        /* Mesh Creation */
        Vector3 vertexFromHeightMapPosition(int x, int z) => new(x * parameters.scale, heightMap[x, z] * parameters.MeshHeightMultiplier, z * parameters.scale);
        Mesh mesh = GenerateMesh(heightMap, vertexFromHeightMapPosition);
        AddPathClosenessInformation(mesh, heightMap.GetLength(0), heightMap.GetLength(1), distanceToPath);

        /* Setting Mesh */
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        /* Decoration */
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        Decorator decorator = GetDecorator(parameters);
        decorator.DecorateChunk(mesh, this, parameters);
        decorator.ApplyShader(meshRenderer, parameters);
    }


    public Decorator GetDecorator(WorldGenerationParameters parameters)
    {
        return parameters.worldType switch
        {
            WorldType.ISLAND => FindAnyObjectByType<IslandDecorator>(),
            _ => FindAnyObjectByType<IslandDecorator>(),
        };
    }

    private void AdjustHeightmapToPath(WorldGenerationParameters parameters, List<List<Vector3>> neighboringChunkPathPoints)
    {
        distanceToPath = new float[heightMap.GetLength(0), heightMap.GetLength(1)];
        for (int z = 0; z < heightMap.GetLength(0); z++)
        {
            for (int x = 0; x < heightMap.GetLength(1); x++)
            {
                distanceToPath[x, z] = int.MaxValue;
            }
        }

        float[,] originalHeightMap = (float[,])heightMap.Clone();

        int onPath = parameters.onPathDistance;
        int nearPath = parameters.nearPathDistance;

        foreach (List<Vector3> chunkPath in neighboringChunkPathPoints)
        {
            for (int i = 0; i < chunkPath.Count - 1; i++)
            {
                Vector3 segmentP0 = chunkPath[i];
                Vector3 segmentP1 = chunkPath[i + 1];
                for (int z = 0; z < heightMap.GetLength(0); z++)
                {
                    float worldZ = offset.y + z * parameters.scale;
                    for (int x = 0; x < heightMap.GetLength(1); x++)
                    {
                        //maybe optimize by first cheking distance to the next points
                        float worldX = offset.x + x * parameters.scale;
                        Vector2 projectedP0 = new Vector2(segmentP0.x, segmentP0.z);
                        Vector2 projectedP1 = new Vector2(segmentP1.x, segmentP1.z);
                        Vector2 heightMapPosition = new Vector2(worldX, worldZ);
                        (float distance, float weight) = Util.MinimumDistanceToLineSegment(projectedP0, projectedP1, heightMapPosition);
                        float interpolatedHeight = Mathf.Lerp(segmentP0.y, segmentP1.y, weight) / parameters.MeshHeightMultiplier;

                        if (distance < distanceToPath[x, z])
                        {
                            distanceToPath[x, z] = distance;
                            if (distance < onPath)
                            {
                                heightMap[x, z] = interpolatedHeight;
                            }
                            else if (distance < nearPath)
                            {
                                float heightScale = 1 - Mathf.InverseLerp(onPath, nearPath, distance);
                                heightMap[x, z] = Mathf.Lerp(originalHeightMap[x, z], interpolatedHeight, heightScale);
                            }
                        }
                    }
                }
            }
        }
    }


    public (int, int, Vector3, List<Vector3>) GeneratePathWithinChunk(
        Vector3 lastPoint,
        int startX,
        int startZ,
        int chunkSize,
        Vector3 tangent,
        Direction direction,
        WorldGenerationParameters parameters
        )
    {
        var pathPoints = new List<Vector3>();

        // Starter-Chunk?
        if (offset == Vector2.zero)
        {
            (startX, startZ) = ChoosePathStartingPoint();
            pathPoints.Add(VertexCoordinate(parameters, startX, startZ));
        }

        // Winkel-Constraints
        var angleConstraints = AngleConstraints(direction);
        angleConstraints = UpdatedAngleConstraints(angleConstraints, tangent, Util.DirectionToVector(direction));

        const int iterationLimit = 300;    // max. Schleifendurchläufe
        int iteration = 0;

        do
        {
            // letzten Punkt holen (oder lastPoint, falls leer)
            Vector3 previousPoint;
            if (pathPoints.Count > 0)
            {
                previousPoint = pathPoints[pathPoints.Count - 1];
            }
            else
            {
                previousPoint = lastPoint;
            }

            // nächstes Segment berechnen
            (int newX, int newZ) = DetermineNextPathPoint(
                heightMap,
                parameters,
                tangent,
                (startX, startZ),
                previousPoint.y,
                Math.Clamp(angleConstraints.Item1, -30, 0),
                Math.Clamp(angleConstraints.Item2, 0, 30)
            );

            // innerhalb der Grenzen halten
            if (direction == Direction.EAST && newX == 0) newX++;
            else if (direction == Direction.WEST && newX == chunkSize) newX--;
            else if (newZ == 0) newZ++;

            var coordinate = VertexCoordinate(parameters, newX, newZ);

            // maximale Steigung sicherstellen
            float planarDist = Vector2.Distance(
                new Vector2(coordinate.x, coordinate.z),
                new Vector2(previousPoint.x, previousPoint.z)
            );
            float rise = coordinate.y - previousPoint.y;
            if (Math.Abs(rise / planarDist) > parameters.maximumIncline)
            {
                coordinate.y = previousPoint.y
                            + Math.Sign(rise)
                            * planarDist
                            * parameters.maximumIncline;
            }

            pathPoints.Add(coordinate);

            // Tangent & Winkel-Constraints updaten
            var newTangent = new Vector3(newX - startX, 0, newZ - startZ);
            angleConstraints = UpdatedAngleConstraints(angleConstraints, newTangent, tangent);
            tangent = newTangent;

            // neue Start-Koords
            startX = newX;
            startZ = newZ;

            // Iterations-Limit prüfen
            iteration++;
            if (iteration >= iterationLimit)
            {
                Debug.LogWarning(
                    $"GeneratePathWithinChunk: reached iteration limit ({iterationLimit}) at chunk offset {offset}. " +
                    "Falling back to partial path."
                );
                break;
            }
        }
        while (!Util.IsChunkBorder(chunkSize, startX, startZ));

        return (startX, startZ, tangent, pathPoints);
    }


    public (int, int) ChoosePathStartingPoint()
    {
        float accumulatedHeights = 0;
        for (int z = 0; z < heightMap.GetLength(0); z++)
        {
            for (int x = 0; x < heightMap.GetLength(1); x++)
            {
                accumulatedHeights += heightMap[x, z];
            }
        }
        float averageHeight = accumulatedHeights / (heightMap.GetLength(0) * heightMap.GetLength(1));

        (int, int) closestToAveragePosition = (0, 0);
        float minDifference = float.MaxValue;
        for (int z = 0; z < heightMap.GetLength(0); z++)
        {
            for (int x = 0; x < heightMap.GetLength(1); x++)
            {
                float difference = Math.Abs(minDifference - averageHeight);
                if (difference < minDifference)
                {
                    closestToAveragePosition = (x, z);
                    minDifference = difference;
                }
            }
        }

        return closestToAveragePosition;
    }

    public (int, int) DetermineNextPathPoint(float[,] heightMap, WorldGenerationParameters parameters, Vector3 tangent, (int, int) indexInVertexGrid, float currentHeight, float maxAngleToLeft, float maxAngleToRight)
    {
        /* default values */
        float defaultConeAngle = parameters.maximumPathTilt * 2;
        float defaultLeftAngle = -parameters.maximumPathTilt;
        int rayLength = parameters.pathSegmentLength;
        int rays = 10;
        int chunkSize = parameters.chunkSize;

        /* constrained values */
        float angleConstraints = Math.Abs(maxAngleToLeft) + Math.Abs(maxAngleToRight);
        float leftAngle = maxAngleToLeft < defaultLeftAngle ? defaultLeftAngle : maxAngleToLeft;
        float coneAngle = angleConstraints < defaultConeAngle ? angleConstraints : defaultConeAngle;
        float angleBetweenRays = coneAngle / rays;

        /* rotations */
        Quaternion toLeft = Quaternion.AngleAxis(leftAngle, Vector3.up);
        Quaternion nextRay = Quaternion.AngleAxis(angleBetweenRays, Vector3.up);

        float minimalHeightDifference = float.MaxValue;
        Vector3 tangentForMinimum = new();
        tangent = toLeft * tangent;
        tangent = tangent.normalized * rayLength;
        for (int i = 0; i < rays; i++)
        {
            // sample height
            float candidateHeight = heightMap[
            (int)Math.Clamp(indexInVertexGrid.Item1 + tangent.x, 0, chunkSize),
            (int)Math.Clamp(indexInVertexGrid.Item2 + tangent.z, 0, chunkSize)
            ];

            // 1) base cost = how steep the step is
            float heightDifference = Math.Abs(currentHeight - candidateHeight);

            float worldY = candidateHeight * parameters.MeshHeightMultiplier;
            float penalty = 0f;
            if (worldY < parameters.seaLevel + 1f)  // “+1” gives a tiny buffer
            {
                penalty = waterAvoidancePenalty;
            }

            float totalCost = heightDifference + penalty;

            if (totalCost < minimalHeightDifference + minimalWaterPenalty)
            {
                minimalHeightDifference = heightDifference;
                minimalWaterPenalty = penalty;
                tangentForMinimum = tangent;
            }

            tangent = nextRay * tangent;
        }

        int x = (int)Math.Clamp(indexInVertexGrid.Item1 + tangentForMinimum.x, 0, chunkSize);
        int z = (int)Math.Clamp(indexInVertexGrid.Item2 + tangentForMinimum.z, 0, chunkSize);
        return (x, z);
    }

    public (float, float) UpdatedAngleConstraints((float, float) angleConstraints, Vector3 from, Vector3 to)
    {
        float angle = Vector3.SignedAngle(from, to, Vector3.up);
        return (angleConstraints.Item1 + angle, angleConstraints.Item2 + angle);
    }


    public (float left, float right) AngleConstraints(Direction direction)
    {
        switch (direction)
        {
            case Direction.NORTH:
                return (-90, 90);
            case Direction.NORTHEAST:
            case Direction.NORTHWEST:
                return (-45, 45);
            case Direction.EAST:
                return (-90, 0);
            case Direction.WEST:
                return (0, 90);
            case Direction.SOUTH:
                Debug.LogWarning("AngleConstraints received SOUTH — using default constraints");
                // Fallback auf voller Links‐/Rechtsreichweite
                return (-90, 90);
            default:
                Debug.LogWarning($"AngleConstraints received unexpected direction {direction}. Using defaults.");
                return (-90, 90);
        }
    }

    public Vector3 VertexCoordinate(WorldGenerationParameters parameters, int heightmapX, int heightmapZ)
    {
        Vector3 coordinate = new Vector3(heightmapX * parameters.scale + offset.x, heightMap[heightmapX, heightmapZ] * parameters.MeshHeightMultiplier, heightmapZ * parameters.scale + offset.y);
        if (coordinate.y < parameters.seaLevel + 5)
        {
            coordinate.y = parameters.seaLevel + 5;
        }
        return coordinate;
    }

    public Texture2D GenerateChunkMap(WorldGenerationParameters parameters, List<List<Vector3>> neighboringChunkPathPoints, Vector2 position)
    {
        offset = position;
        heightMap = GenerateHeightMap(parameters, position, convergenceToPath, terrainCurve);
        AdjustHeightmapToPath(parameters, neighboringChunkPathPoints);
        Vector3 vertexFromHeightMapPosition(int x, int z) => new(x * parameters.scale, heightMap[x, z] * parameters.MeshHeightMultiplier, z * parameters.scale);
        Mesh mesh = GenerateMesh(heightMap, vertexFromHeightMapPosition);
        //AddBiomeInformation(mesh, parameters, heightMap.GetLength(0), heightMap.GetLength(1), position);
        AddPathClosenessInformation(mesh, heightMap.GetLength(0), heightMap.GetLength(1), distanceToPath);
        Decorator decorator = GetDecorator(parameters);
        return decorator.CreateMap(mesh, position, parameters, 25);
    }


    public static float[,] GenerateHeightMap(WorldGenerationParameters wgParams, Vector2 offset, AnimationCurve convergenceToPath, AnimationCurve terrainCurve)
    {
        float[,] heightMap = new float[wgParams.chunkSize + 1, wgParams.chunkSize + 1];
        Vector2[] octaveOffsets = GenerateOctaveOffsets(wgParams.seed, wgParams.octaves, offset);

        for (int z = 0; z <= wgParams.chunkSize; z++)
        {
            for (int x = 0; x <= wgParams.chunkSize; x++)
            {
                float noiseHeight = GenerateNoise(x, z, wgParams, octaveOffsets);
                heightMap[x, z] = terrainCurve.Evaluate(noiseHeight);
            }
        }
        return heightMap;
    }
    public static float[,] GenerateSwamps(float[,] heightMap, WorldGenerationParameters wgParams, Vector2 offset)
    {
        int width = heightMap.GetLength(0);
        int length = heightMap.GetLength(1);

        Vector2[] octaveOffset = GenerateOctaveOffsets(wgParams.seed, 1, offset);
        for (int z = 0; z < width; z++)
        {
            for (int x = 0; x < length; x++)
            {
                if (heightMap[x, z] * wgParams.MeshHeightMultiplier < -160)
                {
                    heightMap[x, z] = GenerateNoise(x, z, wgParams, octaveOffset) * 10;
                }
            }
        }
        return heightMap;
    }


    private static Vector2[] GenerateOctaveOffsets(int seed, int numOcatves, Vector2 offset)
    {
        System.Random offsetGen = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[numOcatves];
        for (int i = 0; i < numOcatves; i++)
        {
            float offsetX = offsetGen.Next(-100000, 100000) + offset.x + 10000;
            float offsetY = offsetGen.Next(-100000, 100000) + offset.y + 10000;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
        return octaveOffsets;
    }

    public static float GenerateNoise(int x, int z, WorldGenerationParameters wgParams, Vector2[] octaveOffsets)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int k = 0; k < octaveOffsets.Length; k++)
        {
            float sample_x = (x * wgParams.scale + octaveOffsets[k].x) / wgParams.noisefactor * frequency;
            float sample_z = (z * wgParams.scale + octaveOffsets[k].y) / wgParams.noisefactor * frequency;
            float perlinValue = Mathf.PerlinNoise(sample_x, sample_z) * 2 - 1;
            noiseHeight += perlinValue * amplitude;
            amplitude *= wgParams.persistence;
            frequency *= wgParams.lacunarity;
        }

        return noiseHeight;
    }

    public static Mesh GenerateMesh(float[,] heightMap, Func<int, int, Vector3> vertexFromHeightMapPosition)
    {
        int width = heightMap.GetLength(0);
        int length = heightMap.GetLength(1);

        Vector3[] vertices = new Vector3[width * length];
        Vector2[] uvs = new Vector2[width * length];
        int[] triangles = new int[(width - 1) * (length - 1) * 6];

        int vertexIndex = 0;
        int triangleIndex = 0;
        for (int z = 0; z < width; z++)
        {
            for (int x = 0; x < length; x++)
            {
                vertices[vertexIndex] = vertexFromHeightMapPosition(x, z);
                uvs[vertexIndex] = new Vector2(x / (float)(length - 1), z / (float)(width - 1));
                if (x < length - 1 && z < width - 1)
                {
                    triangles[triangleIndex] = vertexIndex;
                    triangles[triangleIndex + 1] = vertexIndex + length;
                    triangles[triangleIndex + 2] = vertexIndex + length + 1;
                    triangles[triangleIndex + 3] = vertexIndex;
                    triangles[triangleIndex + 4] = vertexIndex + length + 1;
                    triangles[triangleIndex + 5] = vertexIndex + 1;
                    triangleIndex += 6;
                }
                vertexIndex++;
            }
        }

        Mesh mesh = new()
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }

    public static void AddPathClosenessInformation(Mesh mesh, int width, int length, float[,] pathCloseness)
    {
        Vector2[] pathClosenessUVs = new Vector2[width * length];
        int index = 0;
        for (int z = 0; z < width; z++)
        {
            for (int x = 0; x < length; x++)
            {
                pathClosenessUVs[index] = new Vector2(pathCloseness[x, z], 0);
                index++;
            }
        }
        mesh.uv3 = pathClosenessUVs;
    }

    public Vector2 GetOffset()
    {
        return offset;
    }

    public enum Biome
    {
        MOUNTAINS
    }
}