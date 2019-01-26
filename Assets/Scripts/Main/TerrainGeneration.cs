using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    public int debth = 20;
    public float randomScale = 1;
    public float environmentOffset = -0.2F;
    public Texture2D terrainImage;

    private int width;
    private int height;

    private void Start()
    {
        if (GloabalData.TrackData.TerrainImage != null)
        {
            terrainImage = GloabalData.TrackData.TerrainImage;
        }

        Terrain terrain = GetComponent<Terrain>();
        width = terrainImage.width;
        height = terrainImage.height;

        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    private TerrainData GenerateTerrain(TerrainData data)
    {
        data.heightmapResolution = width + 1;

        data.size = new Vector3(width, debth, height);
        data.SetHeights(0, 0, LoadHeights());
        return data;
    }

    private float[,] LoadHeights()
    {
        float[,] heightsF = GenerateNoiseHeights();

        var pixels = terrainImage.GetPixels32();

        Debug.Log(width);
        Debug.Log(height);
        int i = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float val = (pixels[i].r + pixels[i].g + pixels[i++].b) / 255.0F * 3;
                if (val >= heightsF[x, y])
                {
                    heightsF[x, y] = val;
                }
                
            }
        }

        return heightsF;
    }

    private float[,] GenerateNoiseHeights()
    {
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                heights[x, y] = CalculateNoiseHeight(x, y);
            }
        }

        return heights;
    }

    private float CalculateNoiseHeight(int x, int y)
    {
        float xCoord = (float)x / width * randomScale;
        float yCoord = (float)y / height  * randomScale;

        return Mathf.PerlinNoise(xCoord, yCoord) + environmentOffset;
    }
} 
