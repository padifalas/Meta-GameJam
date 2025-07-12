using System.Collections;
using UnityEngine;


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
    public float spawnOffset = 10f; // distnace behind players 
    
    private void Start()
    {
        if (autoFindPlayers)
        {
            FindAllPlayers();
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
        
       
    }
    
    private void CreateChasersForPlayers()
    {
        if (players == null || players.Length == 0)
        {
          
            return;
        }
        
        if (chaserPrefab == null)
        {
           
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
        
       
        if (spawnPoints != null && spawnPoints.Length > playerIndex && spawnPoints[playerIndex] != null)
        {
            spawnPosition = spawnPoints[playerIndex].position;
        }
        else
        {
            
            spawnPosition = player.position - player.forward * spawnOffset;
            spawnPosition.y = player.position.y;
        }
        
        
        GameObject chaserObject = Instantiate(chaserPrefab, spawnPosition, Quaternion.identity);
        chaserObject.name = "Chaser_" + player.name;
        
       
        ChaserAI chaserAI = chaserObject.GetComponent<ChaserAI>();
        if (chaserAI != null)
        {
            chaserAI.assignedPlayer = player;
            Debug.Log("Created chaser for player: " + player.name);
        }
        else
        {
           
        }
    }
    
   
    public void AssignChaserToPlayer(GameObject chaserObject, Transform player)
    {
        ChaserAI chaserAI = chaserObject.GetComponent<ChaserAI>();
        if (chaserAI != null)
        {
            chaserAI.assignedPlayer = player;
            Debug.Log("Manually assigned chaser to player: " + player.name);
        }
    }
}