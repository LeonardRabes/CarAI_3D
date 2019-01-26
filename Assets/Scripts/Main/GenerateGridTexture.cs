using System;
using Simulation;
using UnityEngine;

public class GenerateGridTexture : MonoBehaviour
{
    public SimulationManager simulationManager;
    public Material gridMaterial;

    private Engine simulationEngine;
    private bool generated = false;

    void Update()
    {
        if (simulationManager.simulationEngine.LoopThread.IsAlive && !generated)
        {
            simulationEngine = simulationManager.simulationEngine;
            gridMaterial.mainTexture = GridToBitmap(simulationEngine.ParkourGrid);
            generated = true;
        }
    }

    private Texture2D GridToBitmap(GridUnit[][] grid)
    {
        Texture2D texture = new Texture2D(grid[0].Length, grid.Length);

        int maxVal = GridUnit.GetByLocation(simulationEngine.SpawnLocation, simulationEngine.ParkourGrid, simulationEngine.GridUnitSize).Value;

        foreach (GridUnit[] unitRow in grid)
        {
            foreach (GridUnit unit in unitRow)
            {
                if (unit.Value >= 0)
                {
                    float alpha = 0.6F;
                    float red = Math.Max(0, 255 - unit.Value / (maxVal / 200 + 1)) / 255F;
                    float green = Math.Max(0, 255 - unit.Value / (maxVal / 200 + 1)) / 255F;
                    float blue = Math.Min(255, 0 + unit.Value / (maxVal / 200 + 1)) / 255F;

                    texture.SetPixel(simulationManager.terrainImage.width / unit.UnitSize - unit.GridLocation.X, unit.GridLocation.Y, new Color(red, green, blue, alpha));
                }
                else
                {
                    texture.SetPixel(simulationManager.terrainImage.width / unit.UnitSize - unit.GridLocation.X, unit.GridLocation.Y, new Color(0, 0, 0, 0));
                }
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return texture;
    }
}
