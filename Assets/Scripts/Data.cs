using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class GloabalData
{
    public struct TrackData
    {
        public static Texture2D TerrainImage = null;
        public static Vector2 SpawnPosition = new Vector2(70, 150);
        public static float SpawnRotation = 0;
        public static Vector2 TargetPosition = new Vector2(720, 965);
    }

    public struct NetworkData
    {
        public static Stream Structure = null;
    }
}
