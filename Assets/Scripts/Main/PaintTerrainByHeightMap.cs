using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintTerrainByHeightMap : MonoBehaviour
{
    [System.Serializable]
    public class SplatHeight
    {
        public int textureIndex;
        public float startingHeight;
    }

    [System.Serializable]
    public class TreeSettings
    {
        public int treeIndex;
        public float minimalHeight;
        public float maximalHeight;
    }

    public Texture2D terrainImage;
    public SplatHeight[] splatHeights;

    public int treeAmount = 100;
    public TreeSettings[] treeSettings;

    void Start()
    {
        if (GloabalData.TrackData.TerrainImage != null)
        {
            terrainImage = GloabalData.TrackData.TerrainImage;
        }

        int width = terrainImage.width;
        int height = terrainImage.height;
        TerrainData terrainData = Terrain.activeTerrain.terrainData;
        float[,,] splatMapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        TreeInstance[] treeInstances = new TreeInstance[treeAmount];
        int treesPlaced = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float imageHeight = terrainImage.GetPixel(y, x).grayscale;

                // paint textures
                float[] splat = new float[splatHeights.Length];

                for (int i = 0; i < splatHeights.Length; i++)
                {
                    if (i == splatHeights.Length - 1 && imageHeight >= splatHeights[i].startingHeight)
                    {
                        splat[i] = 1;
                    }
                    else if (imageHeight >= splatHeights[i].startingHeight && imageHeight <= splatHeights[i + 1].startingHeight)
                    {
                        splat[i] = 1;
                    }
                }

                for (int i = 0; i < splatHeights.Length; i++)
                {
                    splatMapData[x, y, i] = splat[i];
                }

                // paint trees

                for (int i = 0; i < treeSettings.Length; i++)
                {
                    TreePrototype prototype = terrainData.treePrototypes[i];
                    TreeSettings settings = treeSettings[i];

                    float xCoord = (float)x / width;
                    float yCoord = (float)y / height;
                    float theight = terrainData.GetHeight(y, x) / terrainData.size.y;
                    float heightDifference = settings.maximalHeight - settings.minimalHeight;


                    float random = Random.value;
                    float scale = 1000;
                    float noise = Mathf.PerlinNoise(xCoord * scale * (i + 1), yCoord * scale * (i + 1));

                    if (noise > 0.52F && x % 70 == 0 && y % 40 == 0 && treesPlaced < treeInstances.Length && imageHeight <= 0)
                    {
                        TreeInstance tree = new TreeInstance();
                        tree.heightScale = Mathf.Clamp(settings.minimalHeight + heightDifference * noise, settings.minimalHeight, settings.maximalHeight);
                        tree.widthScale = tree.heightScale;
                        tree.position = new Vector3(yCoord, 0, xCoord);
                        tree.prototypeIndex = treeSettings[i].treeIndex;
                        tree.color = new Color32(255, 255, 255, 0);
                        tree.lightmapColor = new Color32(255, 255, 255, 0);

                        treeInstances[treesPlaced++] = tree;
                    }
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatMapData);
        terrainData.treeInstances = treeInstances;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
