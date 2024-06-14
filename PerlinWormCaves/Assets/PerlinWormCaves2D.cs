using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class PerlinWormCaves2D : MonoBehaviour
{
    public int height = 100;
    public int width = 100;

    public float scale = 15f;
    public float offsetx = 0;
    public float offsety = 0;

    public GameObject prefabPlane;

    float[,] noiseMap;

    void Start()
    {
        noiseMap = new float[height, width];
        GameObject Plane = Instantiate(prefabPlane, transform);

        Renderer renderer = Plane.GetComponent<Renderer>();
        renderer.material.mainTexture = GenerateTexture();

    }

    public Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);

        for(int x = 0; x<width; ++x)
        {
            for(int y = 0; y<height; ++y)
            {
                Color color = CalculateCheeseColor(x, y);
                texture.SetPixel(x, y, color);
            }
        }
        List<Vector2Int> maxima = FindLocalMaxima(noiseMap);
        foreach(Vector2Int maximapoint in maxima)
        {
            texture.SetPixel(maximapoint.x, maximapoint.y, Color.blue);
            Vector2Int[] destinations = findNearestMaxima(3, maximapoint, maxima);
            foreach(Vector2Int destination in destinations)
            {
                PerlinWorm plop = new PerlinWorm(new NoiseSettings(), maximapoint, destination);
                List<Vector2> pop = plop.MoveLength(50);
                foreach(Vector2 po in pop)
                {
                    texture.SetPixel((int)po.x, (int)po.y, Color.white);
                    texture.SetPixel((int)po.x, (int)po.y + 1, Color.white);
                    texture.SetPixel((int)po.x + 1, (int)po.y, Color.white);
                    //texture.SetPixel((int)po.x - 1, (int)po.y, Color.white);
                    //texture.SetPixel((int)po.x, (int)po.y - 1, Color.white);
                }
            }
        }


        texture.Apply();
        return texture;
    }

    public Color CalculateColor(int x, int y)
    {
        float coordX = (float)x / width * scale + offsetx;
        float coordY = (float)y / height * scale + offsety;
        float sample = Mathf.PerlinNoise(coordX, coordY);

        float cheeseCoordX = (float)x / width * scale * 1.2f + 100f;
        float cheeseCoordY = (float)y / height * scale * 1.2f + 100f;
        float cheeseSample = Mathf.PerlinNoise(cheeseCoordX, cheeseCoordY);

        if (cheeseSample > 0.6f)
        {
            return new Color(cheeseSample, cheeseSample, cheeseSample);
        }
        else
        {
            if (sample > 0.47 && sample < 0.53)
            {
                return new Color(sample, sample, sample);
            }
            else
            {
                return Color.black;
            }
        }
        
    }
    public Color CalculateCheeseColor(int x, int y)
    {
        float coordX = (float)x / width * scale + offsetx;
        float coordY = (float)y / height * scale + offsety;
        float sample = Mathf.PerlinNoise(coordX, coordY);
        
        if (sample > 0.6f)
        {
            noiseMap[x, y] = sample;
            return Color.white;
        }
        else
        {
            noiseMap[x, y] = 0f;
            return Color.black;
        }
    }
    public static List<Vector2Int> FindLocalMaxima(float[,] noiseMap)
    {
        List<Vector2Int> maximas = new List<Vector2Int>();
        for (int x = 0; x < noiseMap.GetLength(0); x++)
        {
            for (int y = 0; y < noiseMap.GetLength(1); y++)
            {
                var noiseVal = noiseMap[x, y];
                if (noiseMap[x,y] >= 0.3 && CheckNeighbours(x, y, noiseMap, (neighbourNoise) => neighbourNoise > noiseVal))
                {
                    maximas.Add(new Vector2Int(x, y));
                }

            }
        }
        return maximas;
    }
    static List<Vector2Int> directions = new List<Vector2Int>
    {
        new Vector2Int( 0, 1), //N
        new Vector2Int( 1, 1), //NE
        new Vector2Int( 1, 0), //E
        new Vector2Int(-1, 1), //SE
        new Vector2Int(-1, 0), //S
        new Vector2Int(-1,-1), //SW
        new Vector2Int( 0,-1), //W
        new Vector2Int( 1,-1)  //NW
    };
    private static bool CheckNeighbours(int x, int y, float[,] noiseMap, Func<float, bool> failCondition)
    {
        foreach (var dir in directions)
        {
            var newPost = new Vector2Int(x + dir.x, y + dir.y);

            if (newPost.x < 0 || newPost.x >= noiseMap.GetLength(0) || newPost.y < 0 || newPost.y >= noiseMap.GetLength(1))
            {
                continue;
            }

            if (failCondition(noiseMap[x + dir.x, y + dir.y]))
            {
                return false;
            }
        }
        return true;
    }

    private Vector2Int[] findNearestMaxima(int numberOfMaxima, Vector2Int StartMaxima, List<Vector2Int> maxima)
    {
        Vector2Int[] result = new Vector2Int[numberOfMaxima];
        float[] distances = new float[numberOfMaxima];
        for(int i = 0; i<numberOfMaxima; ++i)
        {
            distances[i] = 999999999999999999f;
            result[i] = new Vector2Int(0, 0);
        }
        int cpt = 0;
        foreach(Vector2Int x in maxima)
        {
            if(x != StartMaxima)
            {
                for(int i =0; i<numberOfMaxima; ++i)
                {
                    if(Vector2.Distance(x, StartMaxima)  < distances[i])
                    {
                        for(int j =numberOfMaxima-1; j>i+1; --j)
                        {
                            distances[j] = distances[j-1];
                            result[j] = result[j-1];
                        }
                        distances[i] = Vector2.Distance(x, StartMaxima);
                        result[i] = x;
                        break;
                    }
                }
            }
            ++cpt;
        }

        return result;
    }

}

public class NoiseSettings
{
    [Min(1)]
    public int octaves = 3;
    [Min(0.001f)]
    public float startFrequency = 0.02f;
    [Min(0)]
    public float persistance = 0.5f;
}
public class PerlinWorm
{
    private Vector2 currentDirection;
    private Vector2 currentPosition;
    private Vector2 convergancePoint;
    NoiseSettings noiseSettings;
    public bool moveToConvergancepoint = false;
    [Range(0.5f, 0.9f)]
    public float weight = 0.6f;

    public PerlinWorm(NoiseSettings noiseSettings, Vector2 startPosition, Vector2 convergancePoint)
    {
        currentDirection = Random.insideUnitCircle.normalized;
        this.noiseSettings = noiseSettings;
        this.currentPosition = startPosition;
        this.convergancePoint = convergancePoint;
        this.moveToConvergancepoint = true;
    }

    public PerlinWorm(NoiseSettings noiseSettings, Vector2 startPosition)
    {
        currentDirection = Random.insideUnitCircle.normalized;
        this.noiseSettings = noiseSettings;
        this.currentPosition = startPosition;
        this.moveToConvergancepoint = false;
    }

    public Vector2 MoveTowardsConvergancePoint()
    {
        Vector3 direction = GetPerlinNoiseDirection();
        var directionToConvergancePoint = (this.convergancePoint - currentPosition).normalized;
        var endDirection = ((Vector2)direction * (1 - weight) + directionToConvergancePoint * weight).normalized;
        currentPosition += endDirection;
        return currentPosition;
    }

    public Vector2 Move()
    {
        Vector3 direction = GetPerlinNoiseDirection();
        currentPosition += (Vector2)direction;
        return currentPosition;
    }

    private Vector3 GetPerlinNoiseDirection()
    {
        float noise = SumNoise(currentPosition.x, currentPosition.y, noiseSettings); //0-1
        float degrees = RangeMap(noise, 0, 1, -90, 90);
        currentDirection = (Quaternion.AngleAxis(degrees, Vector3.forward) * currentDirection).normalized;
        return currentDirection;
    }

    public List<Vector2> MoveLength(int length)
    {
        var list = new List<Vector2>();
        foreach (var item in Enumerable.Range(0, length))
        {
            if (moveToConvergancepoint)
            {
                var result = MoveTowardsConvergancePoint();
                list.Add(result);
                if (Vector2.Distance(this.convergancePoint, result) < 1)
                {
                    break;
                }
            }
            else
            {
                var result = Move();
                list.Add(result);
            }


        }
        if (moveToConvergancepoint)
        {
            while (Vector2.Distance(this.convergancePoint, currentPosition) > 1)
            {
                weight = 0.9f;
                var result = MoveTowardsConvergancePoint();
                list.Add(result);
                if (Vector2.Distance(this.convergancePoint, result) < 1)
                {
                    break;
                }
            }
        }

        return list;
    }

    public static float SumNoise(float x, float y, NoiseSettings noiseSettings)
    {
        float amplitude = 1;
        float frequency = noiseSettings.startFrequency;
        float noiseSum = 0;
        float amplitudeSum = 0;
        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            noiseSum += amplitude * Mathf.PerlinNoise(x * frequency, y * frequency);
            amplitudeSum += amplitude;
            amplitude *= noiseSettings.persistance;
            frequency *= 2;
        }
        return noiseSum / amplitudeSum; // set range back to 0-1

    }

    public static float RangeMap(float inputValue, float inMin, float inMax, float outMin, float outMax)
    {
        return outMin + (inputValue - inMin) * (outMax - outMin) / (inMax - inMin);
    }

}
