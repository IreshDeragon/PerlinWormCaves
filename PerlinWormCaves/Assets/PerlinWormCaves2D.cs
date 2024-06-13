using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        noiseMap[x, y] = sample;
        if (sample > 0.6f)
        {
            return Color.white;
        }
        else
        {
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
                if (CheckNeighbours(x, y, noiseMap, (neighbourNoise) => neighbourNoise > noiseVal))
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
}
