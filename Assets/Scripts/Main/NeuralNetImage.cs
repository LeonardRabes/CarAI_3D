using System;
using System.Collections.Generic;
using UnityEngine;
using Simulation;

namespace Assets.Scripts.Main
{
    public static class NeuralNetImage
    {
        public static Texture2D CreateTexture(Brain targetBrain, Texture2D targetTexture, Texture2D backTexture, Rect border, Texture2D inputN, Texture2D hiddenN, Texture2D outputN)
        {
            targetTexture.SetPixels(backTexture.GetPixels());

            int width = (int)border.width;
            int height = (int)border.height;

            int columnWidth = width / targetBrain.AllLayers.Length;
            int[] rowHeight = new int[targetBrain.AllLayers.Length];
            for (int i = 0; i < targetBrain.AllLayers.Length; i++)
            {
                rowHeight[i] = height / targetBrain.AllLayers[i].Length;
            }

            Vector2[][] nodePos = new Vector2[targetBrain.AllLayers.Length][];

            for (int layer = 0; layer < targetBrain.AllLayers.Length; layer++) // get node points
            {
                nodePos[layer] = new Vector2[targetBrain.AllLayers[layer].Length];

                int currentHeight = 0;
                for (int neuron = 0; neuron < targetBrain.AllLayers[layer].Length; neuron++)
                {
                    if (neuron == 0)
                    {
                        currentHeight += rowHeight[layer] / 2;
                    }
                    else
                    {
                        currentHeight += rowHeight[layer];
                    }

                    nodePos[layer][neuron] = new Vector2(columnWidth / 2 + columnWidth * layer + border.x, rowHeight[layer] / 2 + rowHeight[layer] * neuron + border.y);
                }
            }

            for (int layer = 0; layer < targetBrain.AllLayers.Length - 1; layer++) // draw lines between points
            {
                for (int neuron = 0; neuron < targetBrain.AllLayers[layer].Length; neuron++)
                {
                    Neuron targetNode = targetBrain.AllLayers[layer][neuron];

                    for (int i = 0; i < targetNode.OutputConnections.Length; i++)
                    {
                        Vector2 p0 = nodePos[layer][neuron];
                        Vector2 p1 = nodePos[layer + 1][i];
                        Color col = Color.black;
                        float weight = targetBrain.AllLayers[layer + 1][i].Weight[neuron];

                        if (weight > 0)
                        {
                            col = Color.white;
                        }

                        if (weight != 0)
                        {
                            DrawLine((int)p0.x, (int)p0.y, (int)p1.x, (int) p1.y, targetTexture, col);
                        }
                    }
                }
            }

            for (int layer = 0; layer < targetBrain.AllLayers.Length; layer++) // draw nodes at points
            {
                for (int neuron = 0; neuron < targetBrain.AllLayers[layer].Length; neuron++)
                {
                    Neuron targetNode = targetBrain.AllLayers[layer][neuron];
                    Vector2 node = nodePos[layer][neuron];
                    Texture2D tex = null;

                    if (targetNode.Type == Neuron.NeuronType.InputNeuron)
                    {
                        tex = inputN;
                    }
                    else if (targetNode.Type == Neuron.NeuronType.HiddenNeuron)
                    {
                        tex = hiddenN;
                    }
                    else if (targetNode.Type == Neuron.NeuronType.OutputNeuron)
                    {
                        tex = outputN;
                    }

                    AlphaBlendTextures((int)node.x - tex.width / 2, (int)node.y - tex.height / 2, tex.width, tex.height, tex, targetTexture);
                }
            }

            return targetTexture;
        }

        public static void DrawLine(int x0, int y0, int x1, int y1, Texture2D tex, Color col)
        {
            int dy = (int)(y1 - y0);
            int dx = (int)(x1 - x0);
            int stepx, stepy;

            if (dy < 0) { dy = -dy; stepy = -1; }
            else { stepy = 1; }
            if (dx < 0) { dx = -dx; stepx = -1; }
            else { stepx = 1; }
            dy <<= 1;
            dx <<= 1;

            float fraction = 0;

            tex.SetPixel(x0, y0, col);
            if (dx > dy)
            {
                fraction = dy - (dx >> 1);
                while (Mathf.Abs(x0 - x1) > 1)
                {
                    if (fraction >= 0)
                    {
                        y0 += stepy;
                        fraction -= dx;
                    }
                    x0 += stepx;
                    fraction += dy;
                    tex.SetPixel(x0, y0, col);
                }
            }
            else
            {
                fraction = dx - (dy >> 1);
                while (Mathf.Abs(y0 - y1) > 1)
                {
                    if (fraction >= 0)
                    {
                        x0 += stepx;
                        fraction -= dy;
                    }
                    y0 += stepy;
                    fraction += dx;
                    tex.SetPixel(x0, y0, col);
                }
            }
        }

        public static void AlphaBlendTextures(int x, int y, int blockWidth, int blockHeight, Texture2D upper, Texture2D lower)
        {
            Color[] pixelsUpper = upper.GetPixels();
            Color[] pixelsLower = lower.GetPixels(x, y, blockWidth, blockHeight);

            for (int i = 0; i < pixelsLower.Length; i++)
            {
                pixelsLower[i] = pixelsUpper[i] * pixelsUpper[i].a + pixelsLower[i] * (1 - pixelsUpper[i].a);
                pixelsLower[i].a = 1;
            }

            lower.SetPixels(x, y, blockWidth, blockHeight, pixelsLower);
        }
    }
}
