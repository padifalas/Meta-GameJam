using System.Collections.Generic;
using UnityEngine;

public class CollectibleSpawner : MonoBehaviour
{
    [Header("spawn settings")]
    [Space(5)]
    public GameObject pillPrefab; // 3x3 pills 
    public GameObject curePrefab; 
    public Transform[] spawnPoints = new Transform[20]; //  20 spawn points
    
    [Header("spawn quantities")]
    [Space(5)]
    public int pillsToSpawn = 9; // 9 pills (3x3)
    public int curesToSpawn = 4; 
   
    
    private List<Transform> availableSpawnPoints = new List<Transform>();
    
    private void Start()
    {
        if (spawnPoints.Length != 20)
        {
         
            return;
        }
        
        SpawnCollectibles();
    }
    
    private void SpawnCollectibles()
    {
      
        availableSpawnPoints.Clear();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
                availableSpawnPoints.Add(spawnPoints[i]);
        }
        
       
        for (int i = 0; i < pillsToSpawn; i++)
        {
            if (availableSpawnPoints.Count > 0)
            {
                Transform spawnPoint = GetRandomSpawnPoint();
                SpawnPill(spawnPoint);
            }
        }
        
       
        for (int i = 0; i < curesToSpawn; i++)
        {
            if (availableSpawnPoints.Count > 0)
            {
                Transform spawnPoint = GetRandomSpawnPoint();
                SpawnCure(spawnPoint);
            }
        }
        
        
    }
    
    private Transform GetRandomSpawnPoint()
    {
        if (availableSpawnPoints.Count == 0) return null;
        
        int randomIndex = Random.Range(0, availableSpawnPoints.Count);
        Transform chosenPoint = availableSpawnPoints[randomIndex];
        availableSpawnPoints.RemoveAt(randomIndex); // remove so it wont be used again
        
        return chosenPoint;
    }
    
    private void SpawnPill(Transform spawnPoint)
    {
        if (pillPrefab == null || spawnPoint == null) return;
        
        GameObject pill = Instantiate(pillPrefab, spawnPoint.position, spawnPoint.rotation);
        
       
        CollectibleSystem collectibleSystem = pill.GetComponent<CollectibleSystem>();
        if (collectibleSystem == null)
            collectibleSystem = pill.AddComponent<CollectibleSystem>();
        
        collectibleSystem.collectibleType = CollectibleSystem.CollectibleType.Pill;
        
      
        pill.tag = "Collectible";
        
       
    }
    
    private void SpawnCure(Transform spawnPoint)
    {
        if (curePrefab == null || spawnPoint == null) return;
        
        GameObject cure = Instantiate(curePrefab, spawnPoint.position, spawnPoint.rotation);
        
    
        CollectibleSystem collectibleSystem = cure.GetComponent<CollectibleSystem>();
        if (collectibleSystem == null)
            collectibleSystem = cure.AddComponent<CollectibleSystem>();
        
        collectibleSystem.collectibleType = CollectibleSystem.CollectibleType.Cure;
        
       
        cure.tag = "Cure";
        
     
    }
    
    // method to respawn all collectibles (for round restart)
    public void RespawnCollectibles()
    {
        // destroy existing collectibles
        GameObject[] existingCollectibles = GameObject.FindGameObjectsWithTag("Collectible");
        GameObject[] existingCures = GameObject.FindGameObjectsWithTag("Cure");
        
        foreach (GameObject obj in existingCollectibles)
            Destroy(obj);
        
        foreach (GameObject obj in existingCures)
            Destroy(obj);
        
        // respawn
        SpawnCollectibles();
    }
    
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                // different colors for visualization
                if (i < pillsToSpawn)
                    Gizmos.color = Color.green; // pills
                else if (i < pillsToSpawn + curesToSpawn)
                    Gizmos.color = Color.blue; // cures
                else
                    Gizmos.color = Color.gray; // empty spots
                
                Gizmos.DrawWireSphere(spawnPoints[i].position, 0.5f);
                Gizmos.DrawLine(spawnPoints[i].position, spawnPoints[i].position + Vector3.up * 1f);
            }
        }
    }
}