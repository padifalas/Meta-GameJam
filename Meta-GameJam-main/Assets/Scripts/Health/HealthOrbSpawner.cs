using System.Collections;
using UnityEngine;

public class HealthOrbSpawner : MonoBehaviour
{
    [Header("SPAWNING SETTINGS")]
    [Space(5)]
    public GameObject healthOrbPrefab;
    public Transform[] spawnPoints; 
    public float spawnInterval = 10f; // timme between spawns
    public float spawnVariation = 3f; // random variation in spawn time
    public int maxOrbs = 5; //  in scene at once
    
    [Header("AUTO SPAWN POINTS")]
    [Space(5)]
    public bool useAutoSpawnPoints = true;
    public float spawnHeight = 1f;
    public LayerMask groundLayer = 1;
    
    private int currentOrbCount = 0;
    private float nextSpawnTime;
    
    private void Start()
    {
        if (useAutoSpawnPoints)
        {
            GenerateSpawnPoints();
        }
        
       
        nextSpawnTime = Time.time + spawnInterval;
        
       
        StartCoroutine(SpawnHealthOrbs());
    }
    
    private void GenerateSpawnPoints()
    {
        // Create spawn points throughout the level
        // This is a simple example - you might want to customize this based on your level design
        spawnPoints = new Transform[10];
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject spawnPoint = new GameObject("SpawnPoint_" + i);
            spawnPoint.transform.parent = transform;
            
          
            Vector3 randomPos = new Vector3(
                Random.Range(-20f, 20f),
                spawnHeight,
                Random.Range(-20f, 20f)
            );
            
           
            if (Physics.Raycast(randomPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                randomPos.y = hit.point.y + spawnHeight;
            }
            
            spawnPoint.transform.position = randomPos;
            spawnPoints[i] = spawnPoint.transform;
        }
    }
    
    private IEnumerator SpawnHealthOrbs()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval + Random.Range(-spawnVariation, spawnVariation));
            
            if (currentOrbCount < maxOrbs)
            {
                SpawnHealthOrb();
            }
        }
    }
    
    private void SpawnHealthOrb()
    {
        if (spawnPoints.Length == 0) return;
        
       
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
       
        GameObject orb = Instantiate(healthOrbPrefab, spawnPoint.position, Quaternion.identity);
        currentOrbCount++;
        
       
        HealthOrb orbController = orb.GetComponent<HealthOrb>();
        if (orbController == null)
        {
            orbController = orb.AddComponent<HealthOrb>();
        }
        
        orbController.spawner = this;
        
        Debug.Log("health  spawned at: " + spawnPoint.position);
    }
    
    public void OnOrbDestroyed()
    {
        currentOrbCount--;
        currentOrbCount = Mathf.Max(0, currentOrbCount);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        
        Gizmos.color = Color.green;
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }
    }
}