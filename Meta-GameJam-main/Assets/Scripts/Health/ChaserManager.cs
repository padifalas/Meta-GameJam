using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ChaserManager : MonoBehaviour
{
    [Header("CHASER SETUP")]
    [Space(5)]
    public GameObject chaserPrefab; 
    public Transform[] spawnPoints; 
    public bool autoFindPlayers = true;
    public Transform[] players; 
    
    [Header("CHASER SETTINGS")]
    [Space(5)]
    public float spawnOffset = 10f; // distance behind players 
    public bool setupCollisionLayers = true; // automatically setup collision layers
    
    [Header("LAYER SETTINGS")]
    [Space(5)]
    public string playerLayerName = "Player";
    public string chaserLayerName = "Chaser";
    
    private void Start()
    {
        if (autoFindPlayers)
        {
            FindAllPlayers();
        }
        
        if (setupCollisionLayers)
        {
            SetupPlayerLayers();
        }
        
        CreateChasersForPlayers();
    }
    
    private void FindAllPlayers()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        players = new Transform[playerObjects.Length];
        
        for (int i = 0; i < playerObjects.Length; i++)
        {
            players[i] = playerObjects[i].transform;
        }
        
        Debug.Log($"found {players.Length} players for chasers");
    }
    
    private void SetupPlayerLayers()
    {
        // set all players to player layer
        if (players != null)
        {
            foreach (Transform player in players)
            {
                if (player != null)
                {
                    int playerLayer = LayerMask.NameToLayer(playerLayerName);
                    if (playerLayer != -1)
                    {
                        player.gameObject.layer = playerLayer;
                        Debug.Log($"set {player.name} to {playerLayerName} layer");
                    }
                    else
                    {
                        Debug.LogWarning($"layer '{playerLayerName}' not found! create it in project settings");
                    }
                }
            }
        }
    }
    
    private void CreateChasersForPlayers()
    {
        if (players == null || players.Length == 0)
        {
            Debug.LogWarning("no players found to create chasers for!");
            return;
        }
        
        if (chaserPrefab == null)
        {
            Debug.LogError("chaser prefab not assigned!");
            return;
        }
        
        for (int i = 0; i < players.Length; i++)
        {
            CreateChaserForPlayer(players[i], i);
        }
    }
    
    private void CreateChaserForPlayer(Transform player, int playerIndex)
    {
        Vector3 spawnPosition;
        
        // use spawn points if available
        if (spawnPoints != null && spawnPoints.Length > playerIndex && spawnPoints[playerIndex] != null)
        {
            spawnPosition = spawnPoints[playerIndex].position;
        }
        else
        {
            // spawn behind player as fallback
            spawnPosition = player.position - player.forward * spawnOffset;
            spawnPosition.y = player.position.y;
        }
        
        // create chaser
        GameObject chaserObject = Instantiate(chaserPrefab, spawnPosition, Quaternion.identity);
        chaserObject.name = "Chaser_" + player.name;
        
        // set chaser to chaser layer
        int chaserLayer = LayerMask.NameToLayer(chaserLayerName);
        if (chaserLayer != -1)
        {
            chaserObject.layer = chaserLayer;
            Debug.Log($"set {chaserObject.name} to {chaserLayerName} layer");
        }
        else
        {
            Debug.LogWarning($"layer '{chaserLayerName}' not found! create it in project settings");
        }
        
        // setup navmesh agent to ignore obstacles
        NavMeshAgent agent = chaserObject.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            agent.avoidancePriority = 99; // highest priority
            Debug.Log($"configured {chaserObject.name} navmesh agent to ignore obstacles");
        }
        
        // assign target player using reflection to avoid compilation errors
        ChaserAI chaserAI = chaserObject.GetComponent<ChaserAI>();
        if (chaserAI != null)
        {
            // try to set assignedPlayer field using reflection
            var assignedPlayerField = chaserAI.GetType().GetField("assignedPlayer");
            if (assignedPlayerField != null)
            {
                assignedPlayerField.SetValue(chaserAI, player);
                Debug.Log($"assigned {player.name} to {chaserObject.name} via reflection");
            }
            else
            {
                Debug.LogWarning($"assignedPlayer field not found in ChaserAI script!");
                Debug.LogWarning("make sure your ChaserAI script has: public Transform assignedPlayer;");
            }
        }
        else
        {
            Debug.LogError($"chaser prefab missing ChaserAI component!");
        }
    }
    
    // public method to manually assign chaser to player
    public void AssignChaserToPlayer(GameObject chaserObject, Transform player)
    {
        ChaserAI chaserAI = chaserObject.GetComponent<ChaserAI>();
        if (chaserAI != null)
        {
            // try to set assignedPlayer field using reflection
            var assignedPlayerField = chaserAI.GetType().GetField("assignedPlayer");
            if (assignedPlayerField != null)
            {
                assignedPlayerField.SetValue(chaserAI, player);
                Debug.Log($"manually assigned {player.name} to {chaserObject.name} via reflection");
            }
            else
            {
                Debug.LogWarning("assignedPlayer field not found in ChaserAI script!");
            }
        }
    }
    
    // public method to destroy all chasers (useful for round transitions)
    public void DestroyAllChasers()
    {
        GameObject[] chasers = GameObject.FindGameObjectsWithTag("Chaser");
        foreach (GameObject chaser in chasers)
        {
            Destroy(chaser);
        }
        Debug.Log("destroyed all chasers");
    }
    
    // public method to pause/resume all chasers
    public void SetChasersActive(bool active)
    {
        ChaserAI[] allChasers = FindObjectsOfType<ChaserAI>();
        foreach (ChaserAI chaser in allChasers)
        {
            chaser.enabled = active;
        }
        Debug.Log($"set all chasers active: {active}");
    }
}