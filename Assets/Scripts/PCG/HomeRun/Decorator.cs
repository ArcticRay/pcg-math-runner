using System;
using UnityEngine;


public abstract class Decorator : MonoBehaviour {

    public Material material;

    public abstract void DecorateChunk(Mesh chunkMesh, ChunkGenerator chunkGenerator, WorldGenerationParameters parameters);

    public abstract Texture2D CreateMap(Mesh chunkMesh, Vector2 position, WorldGenerationParameters parameters, int isoInterval);

    public bool WithinDistanceToPath(Vector2[] pathProximity, int position, float distance) {
        return pathProximity[position].x < distance;
    }
    public virtual void ApplyShader(MeshRenderer meshRenderer, WorldGenerationParameters parameters) {
        meshRenderer.sharedMaterial = material;
    }

    public Tuple<GameObject, int> RandomGameObject(ChunkGenerator parent, System.Random randomNumberGenerator, Vector3 position, Quaternion rotation, params GameObject[] gameObjects) {
        int random = randomNumberGenerator.Next(gameObjects.Length);
        GameObject newGameObject = Instantiate(gameObjects[random], position, rotation);
    	newGameObject.transform.SetParent(parent.transform);
        return new Tuple<GameObject, int>(newGameObject, random);
    }

    public Vector3 PositionNoise(System.Random randomNumberGenerator, float yComponent) {
        return new Vector3((float)(1-randomNumberGenerator.NextDouble())*2, yComponent, (float)(1-randomNumberGenerator.NextDouble())*2);
    }

    public Vector3 PositionNoise(System.Random randomNumberGenerator, float yComponent, int minimumDistance) {
        float x = (float)(1 - randomNumberGenerator.NextDouble())*10;
        float y = (float)(1 - randomNumberGenerator.NextDouble())*10;
        return new Vector3(x + Math.Sign(x) * minimumDistance, yComponent, y + Math.Sign(y) * minimumDistance);
    }

    public void RandomUpscale(GameObject gameObject, System.Random randomNumberGenerator, int min, int max) {
        gameObject.transform.localScale = new Vector3(
            randomNumberGenerator.Next(min, max),
            randomNumberGenerator.Next(min, max),
            randomNumberGenerator.Next(min, max)
        );
    }
    public void RandomEvenUpscale(GameObject gameObject, System.Random randomNumberGenerator, int min, int max)
    {
        int scale = randomNumberGenerator.Next(min, max);
        gameObject.transform.localScale = new Vector3(
            scale,
            scale,
            scale
        );
    }

    public void RandomRotation(GameObject gameObject, System.Random randomNumberGenerator) {
        gameObject.transform.Rotate(Vector3.up, randomNumberGenerator.Next(0, 360));
    }

    public Quaternion TiltWithTerrain(Vector3 normal) {
        return Quaternion.FromToRotation(Vector3.up, normal);
    }
}