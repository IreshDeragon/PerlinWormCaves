using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PerlinWormCavesGenerator : MonoBehaviour
{
    public int gridHeight = 100;
    public int gridWidth = 100;
    public int gridLength = 100;
    public float blocSpacing = 1;

    public float scale = 15f;
    public float frequency = 0.5f;
    public float threshold = 0.33f;
    public float offsetx = 0;
    public float offsety = 0;
    public float offsetz = 0;

    float[,,] noiseMap;
    Color[,,] noiseColor;

    public GameObject blocTemplate;

    // Start is called before the first frame update
    void Start()
    {
        noiseMap = new float[gridHeight, gridWidth, gridLength];
        noiseColor = new Color[gridHeight, gridWidth, gridLength];

        //Cheese caves
        for (int x = 0; x < gridHeight; ++x)
        {
            for(int y = 0; y < gridWidth; ++y)
            {
                for(int z = 0; z < gridLength; ++z)
                {
                    noiseColor[x,y,z] = CalculateCheeseColor(x, y, z);
                }
            }
        }
        //PerlinWorms
        List<Vector3Int> localMaximas = FindLocalMaxima(noiseMap);
        foreach (Vector3Int maximapoint in localMaximas)
        {
            //texture.SetPixel(maximapoint.x, maximapoint.y, Color.blue);
            Vector3Int[] destinations = FindNearestMaxima(3, maximapoint, localMaximas);
            foreach (Vector3Int destination in destinations)
            {
                PerlinWorm worm = new PerlinWorm(new NoiseSettings(), maximapoint, destination);
                List<Vector3> trail = worm.MoveLength3D(30);
                foreach(Vector3 step in trail)
                {
                    try
                    {
                        noiseColor[(int)step.x, (int)step.y, (int)step.z] = Color.white;
                        noiseColor[(int)step.x+1, (int)step.y, (int)step.z] = Color.white;
                        noiseColor[(int)step.x, (int)step.y, (int)step.z+1] = Color.white;
                        noiseColor[(int)step.x, (int)step.y+1, (int)step.z] = Color.white;
                        noiseColor[(int)step.x - 1, (int)step.y, (int)step.z] = Color.white;
                        noiseColor[(int)step.x, (int)step.y, (int)step.z - 1] = Color.white;
                        noiseColor[(int)step.x, (int)step.y - 1, (int)step.z] = Color.white;
                    }
                    catch
                    {

                    }
                    
                }
                /*PerlinWorm plop = new PerlinWorm(new NoiseSettings(), maximapoint, destination);
                List<Vector2> pop = plop.MoveLength(50);
                foreach (Vector2 po in pop)
                {
                    texture.SetPixel((int)po.x, (int)po.y, Color.white);
                    texture.SetPixel((int)po.x, (int)po.y + 1, Color.white);
                    texture.SetPixel((int)po.x + 1, (int)po.y, Color.white);
                    //texture.SetPixel((int)po.x - 1, (int)po.y, Color.white);
                    //texture.SetPixel((int)po.x, (int)po.y - 1, Color.white);
                }*/
            }
        }

        //instantiation des cubes / mesh
        for (int x = 0; x < gridHeight; ++x)
        {
            for (int y = 0; y < gridWidth; ++y)
            {
                for (int z = 0; z < gridLength; ++z)
                {
                    if (noiseColor[x,y,z] == Color.black)
                    {
                        GameObject spawned = Instantiate(blocTemplate);
                        spawned.transform.position = new Vector3(x*blocSpacing, y * blocSpacing, z * blocSpacing);
                    }
                }
            }
        }
    }

    public Color CalculateCheeseColor(int x, int y, int z)
    {
        float coordX = (float)x / gridHeight * scale + offsetx;
        float coordY = (float)y / gridWidth * scale + offsety;
        float coordZ = (float)z / gridLength * scale + offsetz;
        float sample = perlinNoise.get3DPerlinNoise(new Vector3(x,y,z), frequency);

        if (sample > threshold)
        {
            noiseMap[x, y, z] = sample;
            return Color.white;
        }
        else
        {
            noiseMap[x, y, z] = 0f;
            return Color.black;
        }
    }

    public List<Vector3Int> FindLocalMaxima(float[,,] noiseMap)
    {
        List<Vector3Int> maximas = new List<Vector3Int>();
        for (int x = 0; x < noiseMap.GetLength(0); x++)
        {
            for (int y = 0; y < noiseMap.GetLength(1); y++)
            {
                for(int z = 0; z < noiseMap.GetLength(2); z++)
                {
                    var noiseVal = noiseMap[x, y, z];
                    if (noiseMap[x, y, z] >= threshold && CheckNeighbours(x, y, z, noiseMap, (neighbourNoise) => neighbourNoise > noiseVal))
                    {
                        maximas.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }
        return maximas;
    }
    static List<Vector3Int> directions = new List<Vector3Int>
    {
        new Vector3Int( 0, 1, 0), //N
        new Vector3Int( 1, 1, 0), //NE
        new Vector3Int( 1, 0, 0), //E
        new Vector3Int(-1, 1, 0), //SE
        new Vector3Int(-1, 0, 0), //S
        new Vector3Int(-1,-1, 0), //SW
        new Vector3Int( 0,-1, 0), //W
        new Vector3Int( 1,-1, 0),  //NW
        //UP
        new Vector3Int( 0, 0, 1), //UP
        new Vector3Int( 0, 1, 1), //N 
        new Vector3Int( 1, 1, 1), //NE
        new Vector3Int( 1, 0, 1), //E
        new Vector3Int(-1, 1, 1), //SE
        new Vector3Int(-1, 0, 1), //S
        new Vector3Int(-1,-1, 1), //SW
        new Vector3Int( 0,-1, 1), //W
        new Vector3Int( 1,-1, 1),  //NW
        //Down
        new Vector3Int( 0, 0, -1), //Down
        new Vector3Int( 0, 1, -1), //N 
        new Vector3Int( 1, 1, -1), //NE
        new Vector3Int( 1, 0, -1), //E
        new Vector3Int(-1, 1, -1), //SE
        new Vector3Int(-1, 0, -1), //S
        new Vector3Int(-1,-1, -1), //SW
        new Vector3Int( 0,-1, -1), //W
        new Vector3Int( 1,-1, -1),  //NW
    };
    private static bool CheckNeighbours(int x, int y, int z, float[,,] noiseMap, Func<float, bool> failCondition)
    {
        foreach (var dir in directions)
        {
            var newPost = new Vector3Int(x + dir.x, y + dir.y, z + dir.z);

            if (newPost.x < 0 || newPost.x >= noiseMap.GetLength(0) || newPost.y < 0 || newPost.y >= noiseMap.GetLength(1) || newPost.z < 0 || newPost.z >= noiseMap.GetLength(2))
            {
                continue;
            }

            if (failCondition(noiseMap[x + dir.x, y + dir.y, z + dir.z]))
            {
                return false;
            }
        }
        return true;
    }

    private Vector3Int[] FindNearestMaxima(int numberOfMaxima, Vector3Int StartMaxima, List<Vector3Int> maxima)
    {
        Vector3Int[] result = new Vector3Int[numberOfMaxima];
        float[] distances = new float[numberOfMaxima];
        for (int i = 0; i < numberOfMaxima; ++i)
        {
            distances[i] = 999999999999999999f;
            result[i] = new Vector3Int(0, 0, 0);
        }
        int cpt = 0;
        foreach (Vector3Int x in maxima)
        {
            if (x != StartMaxima)
            {
                for (int i = 0; i < numberOfMaxima; ++i)
                {
                    if (Vector3.Distance(x, StartMaxima) < distances[i])
                    {
                        for (int j = numberOfMaxima - 1; j > i + 1; --j)
                        {
                            distances[j] = distances[j - 1];
                            result[j] = result[j - 1];
                        }
                        distances[i] = Vector3.Distance(x, StartMaxima);
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

