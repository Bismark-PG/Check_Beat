using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StageSetting
{
    public bool isRepeating;      // True => Trigger, Flase -> Incrose Stage Setting
    public int targetStage;       // Trigger Stage
    public int mapWidth;          // Width
    public int mapHeight;         // Height
    public int wallCount;         // Wall Count
    public int normalEnemyCount;  // Normal Type Enemy Count
    public int patrolEnemyCount;  // Chase  Type Enemy Count
}

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("Stage Info")]
    public int currentStage = 1;
    public StageSetting[] stageSettings; // Arry For Editer

    [Header("Player Settings")]
    public GameObject playerPrefab;

    [Header("Exit Settings")]
    public GameObject exitPrefab;
    public float minExitDistance = 3f;
    private GameObject currentExit;

    [Header("Enemy Settings")]
    public GameObject enemyNormalPrefab;
    public GameObject enemyPatrolPrefab;
    private List<GameObject> listEnemies = new List<GameObject>();

    [Header("Wall Settings")]
    public GameObject prefabB;
    public GameObject prefabW;
    public int wallCount = 3;         
    private List<GameObject> listWalls = new List<GameObject>();

    [Header("Map Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float yOffset = 1f;
    public int maxCount = 100;
    private List<Vector3> listPositions = new List<Vector3>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateStage();
    }

    public void GenerateStage()
    {
        ClearPreviousStage();

        // Get Current Stage For Map Setting
        StageSetting baseSetting = stageSettings[0];
        foreach (var setting in stageSettings)
        {
            if (!setting.isRepeating && currentStage >= setting.targetStage)
            {
                baseSetting = setting;
            }
        }

        // Check Repeat Setting
        StageSetting? repeatSetting = null;
        foreach (var setting in stageSettings)
        {
            if (setting.isRepeating)
            {
                repeatSetting = setting;
                break;
            }
        }

        int finalWidth = baseSetting.mapWidth;
        int finalHeight = baseSetting.mapHeight;
        int finalWall = baseSetting.wallCount;
        int finalNormal = baseSetting.normalEnemyCount;
        int finalPatrol = baseSetting.patrolEnemyCount;

        if (repeatSetting.HasValue && currentStage > baseSetting.targetStage)
        {
            int stagesPassed = currentStage - baseSetting.targetStage;        // How Many Stages Passed Since Base Setting
            int repeatCount = stagesPassed / repeatSetting.Value.targetStage; // How Many Repeat Cycles Passed

            finalWidth += repeatSetting.Value.mapWidth * repeatCount;
            finalHeight += repeatSetting.Value.mapHeight * repeatCount;
            finalWall += repeatSetting.Value.wallCount * repeatCount;
            finalNormal += repeatSetting.Value.normalEnemyCount * repeatCount;
            finalPatrol += repeatSetting.Value.patrolEnemyCount * repeatCount;
        }

        MapManager.Instance.SetMapSize(finalWidth, finalHeight);

        if (GameManager.Instance.playerTransform == null && playerPrefab != null)
        {
            SpawnPlayer();
        }

        // Player POS => Register Occupied Position
        Vector3 playerPos = GameManager.Instance.playerTransform.position;
        listPositions.Add(new Vector3(playerPos.x, 0, playerPos.z));

        // Spawn Order
        SpawnExit(playerPos);
        SpawnEnemies(playerPos, finalNormal, enemyNormalPrefab); // Normal Enemy Spawn
        SpawnEnemies(playerPos, finalPatrol, enemyPatrolPrefab); // Patrol Enemy Spawn
        SpawnWalls(finalWall);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateStageText(currentStage);
        }
    }

    void SpawnPlayer()
    {
        Vector3 spawnPos = GetValidRandomPosition(Vector3.zero, 0f, 999f, false, false);
        if (spawnPos != Vector3.zero)
        {
            spawnPos.y = 1f;
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            GameManager.Instance.playerTransform = player.transform;
            Debug.Log("[StageManager] Player Spawned!");
        }
    }

    void SpawnExit(Vector3 playerPos)
    {
        Vector3 spawnPos = GetValidRandomPosition(playerPos, minExitDistance, 999f, false, false);

        if (spawnPos != Vector3.zero)
        {
            Vector3 exitPos = new Vector3(spawnPos.x, 0.75f, spawnPos.z);
            currentExit = Instantiate(exitPrefab, exitPos, Quaternion.identity);
            listPositions.Add(new Vector3(spawnPos.x, 0, spawnPos.z)); // Position Register
            Debug.Log($"[Stage {currentStage}] Exit Spawn!");
        }
    }

    void SpawnEnemies(Vector3 playerPos, int count, GameObject prefab)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetValidRandomPosition(playerPos, 0f, 999f, false, true);

            if (spawnPos != Vector3.zero)
            {
                GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
                listEnemies.Add(enemy);
                listPositions.Add(new Vector3(spawnPos.x, 0, spawnPos.z));
            }
        }
        Debug.Log($"[Stage {currentStage}] Enemy {listEnemies.Count} Spawn!");
    }

    void SpawnWalls(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetValidRandomPosition(Vector3.zero, 0f, 999f, true, false);

            if (spawnPos == Vector3.zero)
            {
                Debug.LogWarning($"[Stage {currentStage}] Wall POS Is Full.");
                break;
            }

            RaycastHit hit; // Floor Cheak
            GameObject wallToSpawn = prefabW;

            if (Physics.Raycast(spawnPos, Vector3.down, out hit, 2f))
            {
                if (hit.collider.CompareTag("Floor_B"))
                {
                    wallToSpawn = prefabB;
                }
                else if (hit.collider.CompareTag("Floor_W"))
                {
                    wallToSpawn = prefabW;
                }
            }

            GameObject wall = Instantiate(wallToSpawn, spawnPos, Quaternion.identity);
            listWalls.Add(wall);
            listPositions.Add(new Vector3(spawnPos.x, 0, spawnPos.z)); // Position Register
        }
        Debug.Log($"[Stage {currentStage}] Wall Count {listWalls.Count} Spawn!");
    }

    Vector3 GetValidRandomPosition(Vector3 playerPos, float minDistance, float maxDistance, bool isWall, bool isEnemy)
    {
        Vector3 spawnPos = Vector3.zero;
        bool isFound = false;
        int attempts = 0;

        while (!isFound && attempts < maxCount)
        {
            attempts++;

            float RandomX = Random.Range(0, MapManager.Instance.currentWidth) + 0.5f;
            float RandomZ = Random.Range(0, MapManager.Instance.currentHeight) + 0.5f;

            spawnPos = new Vector3(RandomX, yOffset, RandomZ);

            // Get Distance on XZ Plane
            if (playerPos != Vector3.zero)
            {
                float dist = Vector3.Distance(new Vector3(playerPos.x, 0, playerPos.z), new Vector3(spawnPos.x, 0, spawnPos.z));
                if (dist < minDistance || dist > maxDistance) continue;
            }

            // Check Exit POS
            if (isEnemy && currentExit != null)
            {
                float dx = Mathf.Abs(spawnPos.x - currentExit.transform.position.x);
                float dz = Mathf.Abs(spawnPos.z - currentExit.transform.position.z);
                if (dx <= 2f && dz <= 2f) continue;
            }

            // Check Wall POS
            if (isWall)
            {
                bool isAdjacent = false;
                foreach (GameObject wall in listWalls)
                {
                    if (wall == null) continue;
                    float dx = Mathf.Abs(wall.transform.position.x - spawnPos.x);
                    float dz = Mathf.Abs(wall.transform.position.z - spawnPos.z);

                    if (dx < 1.1f && dz < 1.1f)
                    {
                        isAdjacent = true;
                        break;
                    }
                }
                if (isAdjacent) continue; // Wall Is Aleay In Range
            }

            bool isOccupied = false;
            foreach (Vector3 occupied in listPositions)
            {
                if (Mathf.Abs(occupied.x - spawnPos.x) < 0.1f && Mathf.Abs(occupied.z - spawnPos.z) < 0.1f)
                {
                    isOccupied = true;
                    break;
                }
            }
            if (!isOccupied) isFound = true;

        }

        if (!isFound)
        {
            Debug.LogWarning($"[Stage Manager] Failed to find a valid spawn position.");
        }

        return isFound ? spawnPos : Vector3.zero;
    }

    void ClearPreviousStage()
    {
        if (currentExit != null) Destroy(currentExit);

        foreach (GameObject enemy in listEnemies) Destroy(enemy);
        listEnemies.Clear();

        foreach (GameObject wall in listWalls) Destroy(wall);
        listWalls.Clear();

        listPositions.Clear();
    }

    public void StageClear()
    {
        currentStage++;
        Debug.Log($"Stage Clear! Now Stage : {currentStage}");
        GenerateStage();
    }
}