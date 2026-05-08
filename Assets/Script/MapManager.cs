using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Floor Prefabs")]
    public GameObject floorPrefabB;
    public GameObject floorPrefabW;

    [Header("Map Size Settings")]
    public int currentWidth = 5;  // Start Width
    public int currentHeight = 5; // Start Height
    public int maxWidth = 15;     // Max Width
    public int maxHeight = 15;    // Max Height

    [Header("Expansion Settings")]
    public int expandInterval = 5; // Expand Stage Interval
    public int expandAmount = 2;   // Amount To Expand Each Time

    // POS List
    private HashSet<Vector2Int> generatedTiles = new HashSet<Vector2Int>();

    // Parent OBJ
    private Transform mapParent;

    void Awake()
    {
        Instance = this;
        // Make Parent OBJ
        mapParent = new GameObject("Environment_Map").transform;
    }
    
    // Check Expand Timming
    public void SetMapSize(int width, int height)
    {
        currentWidth = width;
        currentHeight = height;
        GenerateTiles(currentWidth, currentHeight);
        Debug.Log($"[MapManager] Map Set Done : {currentWidth} x {currentHeight}");
    }

    void GenerateTiles(int targetWidth, int targetHeight)
    {
        for (int x = 0; x < targetWidth; x++)
        {
            for (int z = 0; z < targetHeight; z++)
            {
                Vector2Int POS = new Vector2Int(x, z);
                if (generatedTiles.Contains(POS)) continue;

                // Check Floor Color
                bool IsBlack = (x + z) % 2 == 0;
                GameObject PrefabToSpawn = IsBlack ? floorPrefabB : floorPrefabW;

                // Set Offset
                Vector3 SpawnPos = new Vector3(x + 0.5f, 0, z + 0.5f);

                Instantiate(PrefabToSpawn, SpawnPos, Quaternion.identity, mapParent);
                generatedTiles.Add(POS); // List Add
            }
        }
    }
}