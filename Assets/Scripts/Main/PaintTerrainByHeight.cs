using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintTerrain : MonoBehaviour
{
    [System.Serializable]
    public class SplatHeight
    {
        public int textureIndex;
        public float startingHeight;
    }

    public SplatHeight[] splatHeights;

    void Start()
    {
        TerrainData terrainData = Terrain.activeTerrain.terrainData;
        float[,,] splatMapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float terrainHeight = terrainData.GetHeight(y, x);

                float[] splat = new float[splatHeights.Length];

                for (int i = 0; i < splatHeights.Length; i++)
                {
                    if (i == splatHeights.Length - 1 && terrainHeight >= splatHeights[i].startingHeight)
                    {
                        splat[i] = 1;
                    }
                    else if (terrainHeight >= splatHeights[i].startingHeight && terrainHeight <= splatHeights[i + 1].startingHeight)
                    {
                        splat[i] = 1;
                    }
                }

                for (int i = 0; i < splatHeights.Length; i++)
                {
                    splatMapData[x, y, i] = splat[i];
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, splatMapData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
